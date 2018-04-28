using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Base85
{
    public partial class Base85Codec
    {
        private struct Base85Encoder
        {
            private static readonly uint[] Divisors =
                {
                    0u,
                    0u,
                    0u,
                    0u,

                    85u * 85u * 85u * 85u,
                    85u * 85u * 85u,
                    85u * 85u,
                    85u,
                    1u
                };

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
                BeginWritingAbbreviation,
                WritingAbbreviation,
                BeginWritingPartial,
                WritingPartial,
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

            private uint _encodedBits;
            private uint _encodedCount;
            private int _currentLineLength;
            private int _currentSeparatorIndex;
            private int _currentAffixIndex;
            private char? _charZ;
            private char? _charY;
            private State _currentState;
            private State _previousState;

            public Base85Encoder ( Base85Settings settings )
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
                    out _charZ,
                    out _charY
                );

                Reset();
            }

            public void Reset ()
            {
                _currentState = State.Reset;
                _previousState = State.ReturnToPreviousState;
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
                            while ((_encodedCount < 4) && (inputIndex < inputEnd))
                            {
                                _encodedBits = (_encodedBits << 8) | (inputData[inputIndex++]);
                                _encodedCount++;
                                inputUsed++;
                            }

                            if (_encodedCount == 4) goto case State.BeginWriting;
                            if (flush) goto case State.BeginFlushing;
                            return ConvertStatus.InputRequired;

                        case State.BeginWriting:
                            if (((_encodedBits == 0u) && (_charZ.HasValue)) || ((_encodedBits == 0x20202020u) && (_charY.HasValue)))
                                goto case State.BeginWritingAbbreviation;

                            _currentState = State.Writing;
                            goto case State.Writing;

                        case State.Writing:
                            while ((_encodedCount < 9) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                outputData[outputIndex++] = _alphabet[(int)(_encodedBits / Divisors[_encodedCount++] % 85)];
                                outputUsed++;
                                unchecked { _currentLineLength++; };
                            }

                            if (_encodedCount == 9) goto case State.BeginReading;
                            return ConvertStatus.OutputRequired;

                        case State.BeginWritingAbbreviation:
                            _currentState = State.WritingAbbreviation;
                            goto case State.WritingAbbreviation;

                        case State.WritingAbbreviation:
                            if (outputIndex < outputEnd)
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                // we only enter this state if the encodedBits value is correct and the
                                // associated character has a value (so no need to check the character here). If
                                // support for more abbreviations is added, this code will have to change..

                                outputData[outputIndex++] = (_encodedBits == 0u) ? _charZ.Value : _charY.Value;
                                outputUsed++;
                                unchecked { _currentLineLength++; };

                                // return to reading state..
                                goto case State.BeginReading;
                            }

                            // more output space needed
                            return ConvertStatus.OutputRequired;

                        case State.BeginWritingPartial:
                            // partial writes do not support abbreviations..
                            _currentState = State.WritingPartial;
                            goto case State.WritingPartial;

                        case State.WritingPartial:
                            while ((((_encodedCount & 0xF0) < 0x50)) && (outputIndex < outputEnd))
                            {
                                if ((hasSeparators) && (_currentLineLength == _maximumLineLength))
                                {
                                    SetPreviousState(_currentState);
                                    goto case State.BeginSeparator;
                                }

                                outputData[outputIndex++] = _alphabet[(int)(_encodedBits / Divisors[4 + (_encodedCount & 0x0F)] % 85)];
                                outputUsed++;
                                _encodedCount += 0x0011;
                                unchecked { _currentLineLength++; };
                            }

                            if ((_encodedCount & 0xF0) == 0x50) goto case State.BeginWritingPostfix;
                            return ConvertStatus.OutputRequired;

                        case State.BeginFlushing:
                            if (_encodedCount == 0) goto case State.BeginWritingPostfix;

                            _encodedBits <<= 8 * (int)(4u - _encodedCount);

                            if ((_flags & Base85Settings.FlagForceFullQuantums) != 0)
                            {
                                // encoding complete quantums (btoa) -- caller is responsible for 
                                // ensuring the actual length is written somewhere..

                                _encodedCount = 4;
                                goto case State.BeginWriting;
                            }

                            // trim to minimum characters (2, 3, or 4) -- ascii85
                            _encodedCount = (4u - _encodedCount) << 4;
                            goto case State.BeginWritingPartial;

                        case State.BeginFinalSeparator:
                            if ((!hasSeparators) || ((_flags & Base85Settings.FlagIncludeTerminatingLineSeparator) == 0)) goto case State.Finished;

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
