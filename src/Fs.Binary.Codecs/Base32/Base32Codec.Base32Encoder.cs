using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Base32
{
    public partial class Base32Codec
    {
        private struct Base32Encoder
        {
            private enum State
            {
                Reset,

                BeginWritingPrefix,
                WritingPrefix,

                BeginWritingPostfix,
                WritingPostfix,

                BeginReading,
                Reading,

                BeginWriting,
                Writing,

                BeginWritingPadding,
                WritingPadding,

                BeginFlushing,
                BeginFinalSeparator,
                BeginSeparator,
                WritingSeparator,

                Finished,
                ReturnToPreviousState
            }

            private readonly string _alphabet;
            private readonly string _lineSeparator;
            private readonly string _initialAffix;
            private readonly string _finalAffix;
            private readonly int _maximumLineLength;
            private readonly int _flags;
            private readonly char _paddingChar;

            private ulong _encodedBits;
            private uint _encodedCount;
            private int _currentLineLength;
            private int _currentSeparatorIndex;
            private int _currentAffixIndex;
            private int _requiredPadding;
            private State _currentState;
            private State _previousState;

            public Base32Encoder ( Base32Settings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                // initialize to defaults..
                this = default;

                settings.GetEncoderSettings(
                    out _alphabet,
                    out _flags,
                    out _lineSeparator,
                    out _maximumLineLength,
                    out _initialAffix,
                    out _finalAffix,
                    out _paddingChar
                );

                Reset();
            }

            public void Reset ()
            {
                _currentState = State.Reset;
                _previousState = State.ReturnToPreviousState;
                _requiredPadding = 0;
            }

            public ConvertStatus ConvertData ( ReadOnlySpan<byte> inputData, int inputIndex, int inputCount, Span<char> outputData, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed )
            {
                var inputEnd = inputIndex + inputCount;
                var outputEnd = outputIndex + outputCount;
                var hasSeparators = (_lineSeparator != null);
                inputUsed = outputUsed = 0;

                while (true)
                {
                    switch (_currentState)
                    {
                        case State.Reset:
                            Reset();
                            goto case State.BeginWritingPrefix;

                        case State.BeginWritingPrefix:
                            _currentAffixIndex = 0;
                            _currentState = State.WritingPrefix;
                            goto case State.WritingPrefix;

                        case State.WritingPrefix:
                            while ((_currentAffixIndex < _initialAffix.Length) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                outputData[outputIndex++] = _initialAffix[_currentAffixIndex++];
                                outputUsed++;
                                unchecked { _currentLineLength++; };
                            }

                            if (_currentAffixIndex == _initialAffix.Length) goto case State.BeginReading;
                            return ConvertStatus.OutputRequired;

                        case State.BeginWritingPostfix:
                            _currentAffixIndex = 0;
                            _currentState = State.WritingPostfix;
                            goto case State.WritingPostfix;

                        case State.WritingPostfix:
                            while ((_currentAffixIndex < _finalAffix.Length) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                outputData[outputIndex++] = _finalAffix[_currentAffixIndex++];
                                outputUsed++;
                                unchecked { _currentLineLength++; };
                            }

                            if (_currentAffixIndex == _finalAffix.Length) goto case State.BeginFinalSeparator;
                            return ConvertStatus.OutputRequired;

                        case State.BeginReading:
                            _encodedBits = _encodedCount = 0;
                            _currentState = State.Reading;
                            goto case State.Reading;

                        case State.Reading:
                            while ((_encodedCount < 5) && (inputIndex < inputEnd))
                            {
                                _encodedBits = (_encodedBits << 8) | (inputData[inputIndex++]);
                                _encodedCount++;
                                inputUsed++;
                            }

                            if (_encodedCount == 5) goto case State.BeginWriting;
                            if (flush) goto case State.BeginFlushing;
                            return ConvertStatus.InputRequired;

                        case State.BeginWriting:
                            _currentState = State.Writing;
                            goto case State.Writing;

                        case State.Writing:
                            while ((_encodedCount < 13) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(State.Writing);
                                    goto case State.BeginSeparator;
                                }

                                outputData[outputIndex++] = _alphabet[(int)((_encodedBits & 0xF800000000ul) >> 35)];
                                outputUsed++;
                                _encodedBits <<= 5;
                                _encodedCount++;
                                unchecked { _currentLineLength++; };
                            }

                            if (_encodedCount == 13) goto case State.BeginReading;
                            return ConvertStatus.OutputRequired;

                        case State.BeginWritingPadding:
                            _currentAffixIndex = 0;
                            _currentState = State.WritingPadding;
                            goto case State.WritingPadding;

                        case State.WritingPadding:
                            while ((_currentAffixIndex < _requiredPadding) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                _currentAffixIndex++;
                                outputData[outputIndex++] = _paddingChar;
                                outputUsed++;
                                unchecked { _currentLineLength++; };
                            }

                            if (_currentAffixIndex == _requiredPadding) goto case State.BeginWritingPostfix;
                            return ConvertStatus.OutputRequired;

                        case State.BeginFlushing:
                            if (_requiredPadding != 0) goto case State.BeginWritingPadding;
                            if (_encodedCount == 0) goto case State.BeginWritingPostfix;

                            if ((_flags & Base32Settings.FlagHasPaddingCharacter) != 0)
                                _requiredPadding = PadInfo[PadInfoEncoding, _encodedCount];

                            _encodedBits <<= 8 * (int)(5u - _encodedCount);
                            _encodedCount = 13u - PadInfo[PadInfoEncodingLength, _encodedCount];

                            goto case State.BeginWriting;

                        case State.BeginFinalSeparator:
                            if ((!hasSeparators) || ((_flags & Base32Settings.FlagIncludeTerminatingLineSeparator) == 0)) goto case State.Finished;

                            SetPreviousState(State.Finished);
                            goto case State.BeginSeparator;

                        case State.BeginSeparator:
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
