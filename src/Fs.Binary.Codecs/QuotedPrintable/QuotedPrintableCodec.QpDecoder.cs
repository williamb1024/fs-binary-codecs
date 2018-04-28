using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public partial class QuotedPrintableCodec
    {
        private struct QpDecoder
        {
            private enum State
            {
                Reset,

                BeginReadingPrefix,
                ReadingPrefix,

                BeginReading,
                Reading,

                BeginWriting,
                Writing,

                Flushing,

                Finished,
                ReturnToPreviousState
            }

            private readonly ImmutableArray<byte> _decodingTable;
            private readonly ImmutableArray<byte> _decodingAffixTable;
            private readonly ImmutableArray<string> _decodingPrefixes;
            private readonly ImmutableArray<string> _decodingPostfixes;
            private readonly int _flags;

            private uint _decodedBits;
            private uint _decodedCount;
            private State _currentState;
            private State _previousState;
            private bool _postfixRead;

            public QpDecoder ( QuotedPrintableSettings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                // initialize all fields to default values..
                this = default;

                //settings.GetDecoderSettings(
                //    out _decodingTable,
                //    out _decodingAffixTable,
                //    out _decodingPrefixes,
                //    out _decodingPostfixes,
                //    out _flags
                //);

                Reset();
            }

            public void Reset ()
            {
                _currentState = State.Reset;
                _previousState = State.ReturnToPreviousState;
                _postfixRead = false;
            }

            public ConvertStatus ConvertData ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount, Span<byte> outputData, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed )
            {
                int inputEnd = inputIndex + inputCount;
                int outputEnd = outputIndex + outputCount;
                inputUsed = outputUsed = 0;

                while (true)
                {
                    switch (_currentState)
                    {
                        case State.Reset:
                            Reset();
                            goto case State.BeginReadingPrefix;

                        case State.BeginReadingPrefix:
                            if ((_flags & QuotedPrintableSettings.FlagHasPrefixes) == 0) goto case State.BeginReading;
                            _currentState = State.ReadingPrefix;
                            goto case State.ReadingPrefix;

                        case State.ReadingPrefix:
                            {
                                int matchLength = AffixMatcher.TryMatchAffix(inputData, inputIndex, inputEnd, flush, _decodingPrefixes);
                                if (matchLength < 0) // need more data..
                                    return ConvertStatus.InputRequired;

                                if ((matchLength == 0) && ((_flags & QuotedPrintableSettings.FlagRequirePrefix) != 0))
                                    throw new FormatException(Resources.DecoderPrefixRequired);

                                inputIndex += matchLength;
                                inputUsed += matchLength;
                                goto case State.BeginReading;
                            }

                        case State.BeginReading:
                            _decodedBits = _decodedCount = 0;
                            _currentState = State.Reading;
                            goto case State.Reading;

                        case State.Reading:
                            while ((_decodedCount < 8) && (inputIndex < inputEnd))
                            {
                                char inputChar = inputData[inputIndex++];
                                if (inputChar >= 128)
                                {
                                    if ((_flags & QuotedPrintableSettings.FlagIgnoreInvalidCharacters) == 0)
                                        throw new FormatException(Resources.DecoderInvalidCharacter);

                                    // ignoring invalid characters..
                                    inputUsed++;
                                    continue;
                                }

                                do
                                {
                                    // check the character against the postfix characters, this happens before we
                                    // try to decode (in case the postfix overlaps the alphabet -or- does not)

                                    if ((_decodingAffixTable[inputChar] & 0x02) != 0)
                                    {
                                        int matchLength = AffixMatcher.TryMatchAffix(inputData, inputIndex - 1, inputEnd, flush, _decodingPostfixes);

                                        // incomplete match, need more data...
                                        if (matchLength < 0) return ConvertStatus.InputRequired;
                                        if (matchLength == 0) break;

                                        // any match must use all of the remaining input..
                                        if (matchLength != inputEnd - inputIndex + 1) break;

                                        // have a match and it's at what appears to be the end of the input, but
                                        // we can't actually accept it unless we're flushing..

                                        if (!flush) return ConvertStatus.InputRequired;

                                        // this is a good match, adjust the indices and move to flushing..
                                        inputIndex += matchLength - 1;
                                        inputUsed += matchLength;
                                        _postfixRead = true;

                                        goto case State.Flushing;
                                    }

                                } while (false);

                                // inputChar is always 127 or less here (due to check above)
                                byte decodedValue = _decodingTable[inputChar];
                                if ((decodedValue & QuotedPrintableSettings.CharTypeMask) == QuotedPrintableSettings.CharAlphabet)
                                {
                                    _decodedBits = (_decodedBits << 4) | (uint)(decodedValue & 0x0F);
                                    _decodedCount++;
                                }
                                else if (decodedValue != QuotedPrintableSettings.CharSpecialIgnored)
                                {
                                    // all non-ignored characters are treated as invalid..
                                    if ((_flags & QuotedPrintableSettings.FlagIgnoreInvalidCharacters) == 0)
                                        throw new FormatException(Resources.DecoderInvalidCharacter);
                                }

                                inputUsed++;
                            }

                            if (_decodedCount == 8) goto case State.BeginWriting;
                            if (flush) goto case State.Flushing;
                            return ConvertStatus.InputRequired;

                        case State.BeginWriting:
                            _currentState = State.Writing;
                            goto case State.Writing;

                        case State.Writing:
                            while ((_decodedCount < 12) && (outputIndex < outputEnd))
                            {
                                outputData[outputIndex++] = (byte)((_decodedBits & 0xFF000000) >> 24);
                                outputUsed++;
                                _decodedBits <<= 8;
                                _decodedCount++;
                            }

                            if (_decodedCount == 12) goto case State.BeginReading;
                            return ConvertStatus.OutputRequired;

                        case State.Flushing:
                            if ((_decodedCount & 0x01) != 0)
                            {
                                // an odd number of input characters is invalid..
                                if ((_flags & QuotedPrintableSettings.FlagIgnoreInvalidFinalQuantum) == 0)
                                    throw new FormatException(Resources.DecoderTruncatedQuantum);

                                _decodedBits >>= 4;
                                _decodedCount--;
                            }

                            if (_decodedCount == 0) goto case State.Finished;

                            _decodedBits <<= (int)(8 * (4u - (_decodedCount >> 1)));
                            _decodedCount = 12u - (_decodedCount >> 1);

                            goto case State.BeginWriting;

                        case State.Finished:
                            if (((_flags & QuotedPrintableSettings.FlagRequirePostfix) != 0) && (!_postfixRead))
                                throw new FormatException(Resources.DecoderPostfixRequired);

                            _currentState = State.Reset;
                            return ConvertStatus.Complete;

                        case State.ReturnToPreviousState:
                            System.Diagnostics.Debug.Assert(_previousState != State.ReturnToPreviousState);
                            _currentState = _previousState;
                            _previousState = State.ReturnToPreviousState;
                            continue;
                    }

                    throw new InvalidOperationException("Unreachable code, reached.");
                }
            }

            private void SetPreviousState ( State state )
            {
                System.Diagnostics.Debug.Assert(_previousState == State.ReturnToPreviousState);
                _previousState = state;
            }
        }
    }
}
