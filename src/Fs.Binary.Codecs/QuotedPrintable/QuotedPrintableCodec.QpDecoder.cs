using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Settings;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public partial class QuotedPrintableCodec
    {
        private struct QpDecoder
        {
            private struct CharacterCount
            {
                public char Type;
                public int Count;

                public CharacterCount ( char charType )
                {
                    Type = charType;
                    Count = 1;
                }
            }

            private enum State
            {
                Reset,

                BeginPassThrough,
                PassThrough,
                BeginReadLinearWhiteSpace,
                ReadLinearWhiteSpace,
                BeginWriteLinearWhiteSpace,
                WriteLinearWhiteSpace,

                BeginHardBreak,
                HardBreak,

                BeginWriteHardBreak,
                WriteHardBreak,


                BeginEscape,
                Escape,
                EscapeLowNibble,
                EscapeCR,
                EscapeLF,
                EscapeWrite,




                Flushing,

                Finished,
                ReturnToPreviousState
            }

            private Queue<CharacterCount> _whitespaceCounts;
            private readonly ImmutableArray<byte> _decodingTable;
            private readonly ImmutableArray<byte> _hexDecodingTable;
            private readonly string _hardBreakBytes;
            private readonly int _flags;

            private int _currentOffset;
            private byte _currentByte;
            private CharacterCount _currentCharacterCount;
            private State _currentState;
            private State _previousState;

            public QpDecoder ( QuotedPrintableSettings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                // initialize all fields to default values..
                this = default;

                settings.GetDecoderSettings(
                    out _decodingTable,
                    out _hexDecodingTable,
                    out _hardBreakBytes,
                    out _flags);

                Reset();
            }

            public void Reset ()
            {
                _currentState = State.Reset;
                _previousState = State.ReturnToPreviousState;
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
                            goto case State.PassThrough;


                        case State.BeginPassThrough:
                            _currentState = State.PassThrough;
                            goto case State.PassThrough;

                        case State.PassThrough:
                            while (inputIndex < inputEnd)
                            {
                                char inputChar = inputData[inputIndex];
                                byte inputType = (inputChar < 128) ? _decodingTable[inputChar] : SettingsCharacterTypes.CharSafeInvalid;

                                if ((inputType & SettingsCharacterTypes.CharSafe) != 0)
                                {
                                    // whitespace must be handled separately..
                                    if ((inputChar == ' ') || (inputChar == '\t'))
                                        goto case State.BeginReadLinearWhiteSpace;

                                    if (outputIndex == outputEnd)
                                        return ConvertStatus.OutputRequired;

                                    // input character is safe, goes directly to output data..
                                    outputData[outputIndex++] = (byte)inputChar;
                                    inputIndex++;
                                    inputUsed++;
                                    outputUsed++;

                                    continue;
                                }

                                if (inputChar == '\r') goto case State.BeginHardBreak;

                            }

                            if (flush) goto case State.Finished;
                            return ConvertStatus.InputRequired;

                        case State.BeginReadLinearWhiteSpace:
                            _currentCharacterCount = new CharacterCount(inputData[inputIndex++]);
                            _currentState = State.ReadLinearWhiteSpace;
                            inputUsed++;
                            goto case State.ReadLinearWhiteSpace;

                        case State.ReadLinearWhiteSpace:
                            while (inputIndex < inputEnd)
                            {
                                char inputChar = inputData[inputIndex];
                                if ((inputChar == ' ') || (inputChar == '\t'))
                                {
                                    AddWhitespace(inputChar);
                                    inputIndex++;
                                    inputUsed++;
                                    continue;
                                }

                                // whitespace followed by anything other than a CR is important..
                                if (inputChar != '\r') goto case State.BeginWriteLinearWhiteSpace;
                                    
                                // return to pass through state to handle this character..
                                goto case State.BeginPassThrough;
                            }

                            if (flush) goto case State.Finished; // TODO: must write any whitespace to output.. then we can finish..
                            return ConvertStatus.InputRequired;

                        case State.BeginWriteLinearWhiteSpace:
                            if ((_whitespaceCounts != null) && (_whitespaceCounts.Count > 0))
                            {
                                _whitespaceCounts.Enqueue(_currentCharacterCount);
                                _currentCharacterCount = _whitespaceCounts.Dequeue();
                            }

                            _currentState = State.WriteLinearWhiteSpace;
                            goto case State.WriteLinearWhiteSpace;

                        case State.WriteLinearWhiteSpace:
                            while (outputIndex < outputEnd)
                            {
                                byte charValue = (byte)_currentCharacterCount.Type;
                                int whiteToWrite = Math.Min(outputEnd - outputIndex, _currentCharacterCount.Count);
                                for (int iIndex = outputIndex; iIndex < outputIndex + whiteToWrite; iIndex++)
                                    outputData[iIndex] = charValue;

                                outputIndex += whiteToWrite;
                                outputUsed += whiteToWrite;

                                if ((_currentCharacterCount.Count -= whiteToWrite) == 0)
                                {
                                    if ((_whitespaceCounts != null) && (_whitespaceCounts.Count > 0))
                                        _currentCharacterCount = _whitespaceCounts.Dequeue();
                                    else
                                        // done writing whitespace, return to the pass through state..
                                        goto case State.BeginPassThrough;
                                }
                            }

                            return ConvertStatus.OutputRequired;

                        case State.BeginHardBreak:
                            inputIndex++;
                            inputUsed++;
                            _currentState = State.HardBreak;
                            goto case State.HardBreak;

                        case State.HardBreak:
                            if (inputIndex < inputEnd)
                            {
                                if (inputData[inputIndex] == '\n')
                                {
                                    // formal hard break, consume the LF character..
                                    inputIndex++;
                                    inputUsed++;
                                }
                                else if ((_flags & SettingsFlags.FlagQpAcceptCROnlyHardBreaks) == 0)
                                {
                                    // CR not followed by LF and we're not accepting CR only as a break.. 
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                                        throw new FormatException(Resources.DecoderGenericInvalidCharacter);

                                    // ignore the CR and process the current character..
                                    goto case State.BeginPassThrough;
                                }

                                // hard break read, write out the hard break value
                                goto case State.BeginWriteHardBreak;
                            }

                            if (flush)
                            {
                                if ((_flags & SettingsFlags.FlagQpAcceptCROnlyHardBreaks) == 0)
                                {
                                    if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                                        throw new FormatException(Resources.DecoderGenericInvalidCharacter);

                                    // ignore the CR, goto Finished state..
                                    goto case State.Finished;
                                }

                                // treat CR as formal hard break..
                                goto case State.BeginWriteHardBreak;
                            }

                            return ConvertStatus.InputRequired;

                        case State.BeginWriteHardBreak:
                            _currentOffset = 0;
                            _currentState = State.WriteHardBreak;
                            goto case State.WriteHardBreak;

                        case State.WriteHardBreak:
                            while ((_currentOffset < _hardBreakBytes.Length) && (outputIndex < outputEnd))
                            {
                                outputData[outputIndex++] = (byte)_hardBreakBytes[_currentOffset++];
                                outputUsed++;
                            }

                            if (_currentOffset == _hardBreakBytes.Length) goto case State.BeginPassThrough;
                            return ConvertStatus.OutputRequired;

                        //case State.BeginEscape:
                        //    _currentByte = 0;
                        //    _currentState = State.Escape;
                        //    goto case State.Escape;

                        //case State.Escape:
                        //    if (inputIndex < inputEnd)
                        //    {
                        //        char inputChar = inputData[inputIndex++];
                        //        byte inputValue = (inputChar < 128) ? _hexDecodingTable[inputChar] : SettingsCharacterTypes.CharSpecialInvalid;
                        //        if ((inputValue & SettingsCharacterTypes.CharSpecial) == 0)
                        //        {
                        //            inputUsed++;
                        //            _currentByte = (byte)(inputValue << 4);
                        //            _currentState = State.EscapeLowNibble;
                        //            goto case State.EscapeLowNibble;
                        //        }

                        //        if (inputChar == '\r') goto case State.EscapeCR;
                        //        if (inputChar == '\n') goto case State.EscapeLF;

                        //        // any other value is invalid..
                        //        if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                        //            throw new FormatException(Resources.DecoderHexDigitOrLineBreakExpected);

                        //        // re-read the current input character, discard the escape character..
                        //        inputIndex--;
                        //        goto case State.BeginReading;
                        //    }

                        //    // TODO: must check flush here..
                        //    return ConvertStatus.InputRequired;

                        //case State.EscapeLowNibble:
                        //    if (inputIndex < inputEnd)
                        //    {
                        //        char inputChar = inputData[inputIndex++];
                        //        byte inputValue = (inputChar < 128) ? _hexDecodingTable[inputChar] : SettingsCharacterTypes.CharSpecialInvalid;
                        //        if ((inputValue & SettingsCharacterTypes.CharSpecial) == 0)
                        //        {
                        //            inputUsed++;
                        //            _currentByte |= (byte)(inputValue & 0x0F);
                        //            _currentState = State.EscapeWrite;
                        //            goto case State.EscapeWrite;
                        //        }

                        //        // not a hex character, fail if not ignoring invalid characters..
                        //        if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                        //            throw new FormatException(Resources.DecoderHexDigitOrLineBreakExpected);

                        //        // adjust inputIndex to reread the current character and return to
                        //        // reading..

                        //        inputIndex--;
                        //        goto case State.BeginReading;
                        //    }

                        //    // TODO: must check flush here..
                        //    return ConvertStatus.InputRequired;

                        //case State.EscapeCR:
                        //    if (inputIndex < inputEnd)
                        //    {
                        //        char inputChar = inputData[inputIndex];
                        //        if (inputChar == '\n') // soft-break?
                        //        {
                        //            inputIndex++;
                        //            inputUsed += 2;
                        //            goto case State.BeginReading;
                        //        }

                        //        if ((_flags & SettingsFlags.FlagQpAcceptCROnlyHardBreaks) != 0)
                        //        {

                        //        }

                        //    }

                        //    break;

                        //case State.EscapeLF:
                        //    if ((_flags & SettingsFlags.FlagQpAcceptLFOnlyHardBreaks) == 0)
                        //    {
                        //        if ((_flags & SettingsFlags.FlagIgnoreInvalidCharacters) == 0)
                        //            throw new FormatException(Resources.DecoderGenericInvalidCharacter);

                        //        // ignoring invalid characters... do not re-read LF..
                        //        inputUsed++;
                        //        goto case State.BeginReading;
                        //    }

                        //    break;

                        //case State.EscapeWrite:
                        //    break;

                        case State.Finished:
                            //if (((_flags & QuotedPrintableSettings.FlagRequirePostfix) != 0) && (!_postfixRead))
                            //    throw new FormatException(Resources.DecoderPostfixRequired);

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
            private void AddWhitespace ( char whitespaceType )
            {
                if (_currentCharacterCount.Type == whitespaceType)
                    _currentCharacterCount.Count++;
                else
                {
                    if (_whitespaceCounts == null)
                        _whitespaceCounts = new Queue<CharacterCount>();

                    _whitespaceCounts.Enqueue(_currentCharacterCount);
                    _currentCharacterCount = new CharacterCount(whitespaceType);
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
