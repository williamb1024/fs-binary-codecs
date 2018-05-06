using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Settings;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public partial class QuotedPrintableCodec
    {
        internal struct QpEncoder
        {
            private enum State
            {
                Reset,

                //BeginWritingPrefix,
                //WritingPrefix,
                //BeginWritingPostfix,
                //WritingPostfix,

                BeginReading,
                Reading,

                BeginWriting,
                TryFastPathWriting,
                WritingTick,
                WritingHigh,
                WritingLow,

                BeginFlushing,

                BeginSeparator,
                WritingSeparator,

                Finished,
                ReturnToPreviousState
            }

            private readonly ImmutableArray<byte> _safeCharacters;
            private readonly string _hexCharacters;
            private readonly string _lineSeparator;
            private readonly int _maximumLineLength;
            private readonly int _flags;

            private byte _currentByte;
            private int _currentByteIndex;
            private int _currentLineLength;
            private int _currentSeparatorIndex;

            private State _currentState;
            private State _previousState;

            public QpEncoder ( QuotedPrintableSettings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                // initialize all fields to defaults..
                this = default;

                settings.GetEncoderSettings(
                    out _safeCharacters,
                    out _flags,
                    out _lineSeparator,
                    out _maximumLineLength,
                    out _hexCharacters);

                // decrement maximumLineLength to account for required soft-breaks
                // at the end of lines..

                _maximumLineLength--;

                Reset();
            }

            public void Reset ()
            {
                _currentLineLength = 0;
                _currentState = State.Reset;
                _previousState = State.ReturnToPreviousState;
            }

            public ConvertStatus ConvertData ( ReadOnlySpan<byte> inputData, int inputIndex, int inputCount, Span<char> outputData, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed )
            {
                var inputEnd = inputIndex + inputCount;
                var outputEnd = outputIndex + outputCount;
                var hasSeparators = (_lineSeparator != null) && (_lineSeparator.Length != 0);
                inputUsed = outputUsed = 0;

                while (true)
                {
                    switch (_currentState)
                    {
                        case State.Reset:
                            Reset();
                            goto case State.BeginReading;

                        case State.BeginReading:
                            _currentState = State.Reading;
                            goto case State.Reading;

                        case State.Reading:
                            while (inputIndex < inputEnd)
                            {
                                byte inputByte = inputData[inputIndex++];
                                byte inputType = (inputByte < 128) ? _safeCharacters[inputByte] : (byte)SettingsCharacterTypes.CharSafeExcluded;
                                if ((inputType & ~SettingsCharacterTypes.CharSafeFlagsMask) == SettingsCharacterTypes.CharSafe)
                                {
                                    if (outputIndex < outputEnd)
                                    {
                                        // if we've have separator and we've reached the end of the line, or the current
                                        // character is whitespace and it would be the last character on a line, then
                                        // write a line separator..

                                        if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                        {
                                            // backup inputIndex so that we read this character
                                            inputIndex--;

                                            // write a separator..
                                            SetPreviousState(State.Reading);
                                            goto case State.BeginSeparator;
                                        }

                                        // character is a safe character, write it directly to the output..
                                        outputData[outputIndex++] = (char)inputByte;
                                        inputUsed++;
                                        outputUsed++;
                                        unchecked { _currentLineLength++; }
                                    }
                                    else
                                        // more output space is required..
                                        return ConvertStatus.OutputRequired;
                                }
                                else // non-safe character..
                                {
                                    _currentByteIndex = 0;
                                    _currentByte = inputByte;
                                    inputUsed++;

                                    goto case State.BeginWriting;
                                }
                            }

                            if (flush) goto case State.BeginFlushing;
                            return ConvertStatus.InputRequired;

                        case State.BeginWriting:
                            if ((hasSeparators) && (_currentLineLength + 3 > _maximumLineLength))
                            {
                                // the encoded output would exceed the maximum line length, write a separator
                                // then write the encoded character..

                                SetPreviousState(State.TryFastPathWriting);
                                goto case State.BeginSeparator;
                            }

                            goto case State.TryFastPathWriting;

                        case State.TryFastPathWriting:
                            if (outputEnd - outputIndex >= 3)
                            {
                                outputData[outputIndex++] = '=';
                                outputData[outputIndex++] = _hexCharacters[(_currentByte >> 4) & 0x0F];
                                outputData[outputIndex++] = _hexCharacters[_currentByte & 0x0F];

                                outputUsed += 3;
                                unchecked { _currentLineLength += 3; }

                                goto case State.BeginReading;
                            }

                            goto case State.WritingTick;

                        case State.WritingTick:
                            if (outputIndex < outputEnd)
                            {
                                outputData[outputIndex++] = '=';
                                outputUsed++;
                                unchecked { _currentLineLength++; }

                                goto case State.WritingHigh;
                            }

                            _currentState = State.WritingTick;
                            return ConvertStatus.OutputRequired;

                        case State.WritingHigh:
                            if (outputIndex < outputEnd)
                            {
                                outputData[outputIndex++] = _hexCharacters[(_currentByte >> 4) & 0x0F];
                                outputUsed++;
                                unchecked { _currentLineLength++; }

                                goto case State.WritingLow;
                            }

                            _currentState = State.WritingHigh;
                            return ConvertStatus.OutputRequired;

                        case State.WritingLow:
                            if (outputIndex < outputEnd)
                            {
                                outputData[outputIndex++] = _hexCharacters[_currentByte & 0x0F];
                                outputUsed++;
                                unchecked { _currentLineLength++; }

                                goto case State.BeginReading;
                            }

                            _currentState = State.WritingLow;
                            return ConvertStatus.OutputRequired;

                        case State.BeginFlushing:
                            // ignore force terminal line separator, it's always required if we have
                            // separators..

                            if ((hasSeparators) && (_currentLineLength != 0))
                            {
                                SetPreviousState(State.BeginFlushing);
                                goto case State.BeginSeparator;
                            }

                            goto case State.Finished;

                        case State.BeginSeparator:
                            if (outputIndex == outputEnd)
                            {
                                _currentState = State.BeginSeparator;
                                return ConvertStatus.OutputRequired;
                            }

                            outputData[outputIndex++] = '=';
                            outputUsed++;

                            _currentLineLength = _currentSeparatorIndex = 0;
                            _currentState = State.WritingSeparator;
                            goto case State.WritingSeparator;

                        case State.WritingSeparator:
                            while ((_currentSeparatorIndex < _lineSeparator.Length) && (outputIndex < outputEnd))
                            {
                                outputData[outputIndex++] = _lineSeparator[_currentSeparatorIndex++];
                                outputUsed++;
                            }

                            if (_currentSeparatorIndex == _lineSeparator.Length) goto case State.ReturnToPreviousState;
                            return ConvertStatus.OutputRequired;

                        case State.Finished:
                            _currentState = State.Reset;
                            return ConvertStatus.Complete;

                        case State.ReturnToPreviousState:
                            System.Diagnostics.Debug.Assert(_previousState != State.ReturnToPreviousState);
                            _currentState = _previousState;
                            _previousState = State.ReturnToPreviousState;
                            continue;

                        default:
                            break;
                    }

                    throw new InvalidOperationException("Unreachable code, reached.");
                }
            }

            private void SetPreviousState ( State previousState )
            {
                System.Diagnostics.Debug.Assert(_previousState == State.ReturnToPreviousState);
                _previousState = previousState;
            }
        }
    }
}
