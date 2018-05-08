using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Settings;

namespace Fs.Binary.Codecs.Base32
{
    public partial class Base32Codec
    {
        private struct Base32Decoder
        {
            private enum State
            {
                Reset,

                BeginReadingPrefix,
                ReadingPrefix,

                BeginReading,
                ContinueReading,
                Reading,

                BeginWriting,
                Writing,

                BeginReadingPadding,
                ReadingPadding,

                Flushing,

                Finished,
                ReturnToPreviousState
            }

            private readonly ImmutableArray<byte> _decodingTable;
            private readonly ImmutableArray<byte> _decodingAffixTable;
            private readonly ImmutableArray<string> _decodingPrefixes;
            private readonly ImmutableArray<string> _decodingPostfixes;
            private readonly int _flags;

            private ulong _decodedBits;
            private uint _decodedCount;
            private State _currentState;
            private State _previousState;
            private bool _postfixRead;
            private byte _paddingRead;
            private byte _paddingRequired;

            public Base32Decoder ( Base32Settings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                // initialize all fields to default values..
                this = default;

                settings.GetDecoderSettings(
                    out _decodingTable,
                    out _decodingAffixTable,
                    out _decodingPrefixes,
                    out _decodingPostfixes,
                    out _flags
                );

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
                            if ((_flags & SettingsFlags.FlagHasPrefixes) == 0) goto case State.BeginReading;
                            _currentState = State.ReadingPrefix;
                            goto case State.ReadingPrefix;

                        case State.ReadingPrefix:
                            {
                                int matchLength = AffixMatcher.TryMatchAffix(inputData, inputIndex, inputEnd, flush, _decodingPrefixes);
                                if (matchLength < 0) // need more data..
                                    return ConvertStatus.InputRequired;

                                if ((matchLength == 0) && ((_flags & SettingsFlags.FlagRequirePrefix) != 0))
                                    throw new FormatException(Resources.DecoderPrefixRequired);

                                inputIndex += matchLength;
                                inputUsed += matchLength;
                                goto case State.BeginReading;
                            }

                        case State.BeginReading:
                            _decodedBits = _decodedCount = 0;
                            _currentState = State.Reading;
                            goto case State.Reading;

                        case State.ContinueReading:
                            _currentState = State.Reading;
                            goto case State.Reading;

                        case State.Reading:
                            while ((_decodedCount < 8) && (inputIndex < inputEnd))
                            {
                                char inputChar = inputData[inputIndex++];
                                if (inputChar >= 128)
                                {
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
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
                                if ((decodedValue & SettingsCharacterTypes.CharTypeMask) == SettingsCharacterTypes.CharAlphabet)
                                {
                                    _decodedBits = (_decodedBits << 5) | (uint)(decodedValue & 0x1F);
                                    _decodedCount++;
                                }
                                else if (decodedValue != SettingsCharacterTypes.CharSpecialIgnored)
                                {
                                    // padding characters are handled in the reading padding state..
                                    if (decodedValue == SettingsCharacterTypes.CharSpecialPadding)
                                        goto case State.BeginReadingPadding;

                                    // all other characters are considered invalid..
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
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
                            while ((_decodedCount < 13) && (outputIndex < outputEnd))
                            {
                                outputData[outputIndex++] = (byte)((_decodedBits & 0xFF00000000ul) >> 32);
                                outputUsed++;
                                _decodedBits <<= 8;
                                _decodedCount++;
                            }

                            if (_decodedCount == 13) goto case State.BeginReading;
                            return ConvertStatus.OutputRequired;

                        case State.BeginReadingPadding:
                            // determine the amount of padding that we should expect if this is the
                            // final quantum of the input..

                            _paddingRead = 1;
                            _paddingRequired = Base32Codec.PadInfo[PadInfoDecoding, _decodedCount];

                            if ((_paddingRequired == 0) && ((_flags & SettingsFlags.FlagIgnoreInvalidPadding) == 0))
                                throw new FormatException(Resources.DecoderInvalidPadding);

                            // reading state doesn't "use" the first padding character, so we have to..
                            inputUsed++;
                            _currentState = State.ReadingPadding;
                            goto case State.ReadingPadding;

                        case State.ReadingPadding:
                            while (inputIndex < inputEnd)
                            {
                                char inputChar = inputData[inputIndex++];
                                if (inputChar >= 128)
                                {
                                    // character is invalid, if we're ignoring invalid characters then we can just keep
                                    // going otherwise, now is the time to fail..

                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                                        throw new FormatException(Resources.DecoderInvalidCharacter);

                                    inputUsed++;
                                    continue;
                                }

                                do
                                {
                                    // check the input character against the affix table, if it is the start of 
                                    // a possible postfix, try to match it..

                                    if ((_decodingAffixTable[inputChar] & 0x02) != 0)
                                    {
                                        // match the input data against the postfixes..
                                        int matchLength = AffixMatcher.TryMatchAffix(inputData, inputIndex - 1, inputEnd, flush, _decodingPostfixes);

                                        if (matchLength < 0) return ConvertStatus.InputRequired; // need more data to determine
                                        if (matchLength == 0) break; // definative non-match

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
                                if (decodedValue == SettingsCharacterTypes.CharSpecialPadding)
                                {
                                    _paddingRead++;
                                    if (_paddingRead > _paddingRequired)
                                    {
                                        if ((_flags & SettingsFlags.FlagIgnoreInvalidPadding) == 0)
                                            throw new FormatException(Resources.DecoderInvalidPadding);

                                        // reset _paddingRead so that we don't overflow it..
                                        _paddingRead = (byte)(_paddingRequired + 1);
                                    }

                                    inputUsed++;
                                    continue;
                                }
                                else if ((decodedValue & SettingsCharacterTypes.CharTypeMask) == SettingsCharacterTypes.CharAlphabet)
                                {
                                    // all of the padding that we've read is invalid...
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidPadding) == 0)
                                        throw new FormatException(Resources.DecoderInvalidPadding);

                                    // go back to the reading state..
                                    _paddingRead = 0;
                                    inputIndex--;

                                    goto case State.ContinueReading;
                                }
                                else if ((decodedValue == SettingsCharacterTypes.CharSpecialIgnored) || ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) != 0))
                                {
                                    // always ignored character, or invalid character that we are ignoring...
                                    inputUsed++;
                                    continue;
                                }

                                // anything else is an invalid character..
                                throw new FormatException(Resources.DecoderInvalidCharacter);
                            }

                            // we only reach this point if we've run out of input..
                            if (flush) goto case State.Flushing;
                            return ConvertStatus.InputRequired;

                        case State.Flushing:
                            {
                                bool invalidQuantum = (PadInfo[PadInfoUnusedBits, _decodedCount] >= 5);
                                if (invalidQuantum)
                                {
                                    // an incorrect number of characters in the last quantum..
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidFinalQuantum) == 0)
                                        throw new FormatException(Resources.DecoderTruncatedQuantum);

                                    // if we require padding, then this is an error state, because there is no correct
                                    // padding for an invalid last quantum..

                                    if ((_flags & SettingsFlags.FlagRequirePadding) != 0)
                                        throw new FormatException(Resources.DecoderInvalidPadding);

                                    // remove characters from the quantum to make it "valid" and adjust the decoded bits
                                    // appropriately..

                                    var toRemove = PadInfo[PadInfoRemove, _decodedCount];
                                    _decodedCount -= toRemove;
                                    _decodedBits >>= (toRemove * 5);
                                }

                                if ((_flags & SettingsFlags.FlagRequirePadding) != 0)
                                {
                                    // valid padding is required, there are few problems here. First is that
                                    // we may have an invalid quantum, which means any padding is invalid 

                                    var requiredPadding = PadInfo[PadInfoDecoding, _decodedCount];
                                    if (_paddingRead < requiredPadding)
                                        throw new FormatException(Resources.DecoderPaddingRequired);
                                    else if ((_paddingRead > requiredPadding) && ((_flags & SettingsFlags.FlagIgnoreInvalidPadding) == 0))
                                        throw new FormatException(Resources.DecoderInvalidPadding);
                                }
                            }

                            if (_decodedCount == 0) goto case State.Finished;

                            if ((_flags & SettingsFlags.FlagRequireTrailingZeroBits) != 0)
                            {
                                // ensure that the unused trailing bits are all zero, this can detect truncation
                                // in some cases ..

                                uint unusedBits = (1u << PadInfo[PadInfoUnusedBits, _decodedCount]) - 1u;
                                if ((_decodedBits & unusedBits) != 0)
                                    throw new FormatException(Resources.DecoderUnusedBitNonZero);
                            }

                            // adjust decodedBits and decodedCount so that we can flush the final quantum..
                            _decodedBits <<= (int)(40u - (5u * _decodedCount));
                            _decodedCount = 13u - ((5u * _decodedCount) >> 3);

                            goto case State.BeginWriting;

                        case State.Finished:
                            if (((_flags & SettingsFlags.FlagRequirePostfix) != 0) && (!_postfixRead))
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

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private void SetPreviousState ( State state )
            {
                System.Diagnostics.Debug.Assert(_previousState == State.ReturnToPreviousState);
                _previousState = state;
            }
        }
    }
}
