using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Base85
{
    public partial class Base85Codec : BinaryCodec
    {
        private const int PadInfoRequiredPadding = 0;
        private const int PadInfoUnusedBits = 1;
        private const int PadInfoBytesToChars = 2;
        private const int PadInfoCharsToBytes = 3;

        private static readonly byte[,] PadInfo = new byte[,]
        {
            { 0, 0, 2, 1, 0 },
            { 0, 6, 4, 2, 0 },
            { 0, 2, 3, 4, 0 },
            { 0, 0, 1, 2, 3 }
        };

        public Base85Codec ( Base85Settings settings )
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Settings = settings.ToReadOnly();
        }

        public override BinaryEncoder GetEncoder ()
        {
            return new Encoder(Settings);
        }

        public override BinaryDecoder GetDecoder ()
        {
            return new Decoder(Settings);
        }

        public override int GetCharCount ( ReadOnlySpan<byte> bytes )
        {
            var encoder = new Base85Encoder(Settings);
            Span<char> outputBuffer = stackalloc char[1024];

            int inputOffset = 0;
            int inputCount = bytes.Length;
            int outputLength = 0;
            ConvertStatus convertStatus;

            do
            {
                convertStatus = encoder.ConvertData(bytes, inputOffset, inputCount,
                                                    outputBuffer, 0, outputBuffer.Length,
                                                    true,
                                                    out var inputUsed,
                                                    out var outputUsed);

                outputLength += outputUsed;
                inputOffset += inputUsed;
                inputCount -= inputUsed;

            } while (!convertStatus);

            return outputLength;
        }

        public override int GetByteCount ( ReadOnlySpan<char> chars )
        {
            var decoder = new Base85Decoder(Settings);
            Span<byte> outputBuffer = stackalloc byte[1024];

            int inputOffset = 0;
            int inputCount = chars.Length;
            int outputLength = 0;
            ConvertStatus convertStatus;

            do
            {
                convertStatus = decoder.ConvertData(chars, inputOffset, inputCount,
                                                    outputBuffer, 0, outputBuffer.Length,
                                                    true,
                                                    out var inputUsed,
                                                    out var outputUsed);

                outputLength += outputUsed;
                inputOffset += inputUsed;
                inputCount -= inputUsed;

            } while (!convertStatus);

            return outputLength;
        }

        public override char[] GetChars ( ReadOnlySpan<byte> bytes )
        {
            // GetChars process the data in two passes, in order to avoid allocating
            // temporary storage for the result (which would have to be resized)

            int charsRequired = GetCharCount(bytes);
            if (charsRequired == 0)
                return Array.Empty<char>();

            var outputChars = new char[charsRequired];

            var encoder = new Base85Encoder(Settings);
            var convertStatus = encoder.ConvertData(bytes, 0, bytes.Length,
                                                    outputChars.AsSpan(), 0, outputChars.Length,
                                                    true,
                                                    out var inputUsed,
                                                    out var outputUsed);

            System.Diagnostics.Debug.Assert(convertStatus);
            System.Diagnostics.Debug.Assert(inputUsed == bytes.Length);
            System.Diagnostics.Debug.Assert(outputUsed == outputChars.Length);

            return outputChars;
        }

        public override unsafe string GetString ( ReadOnlySpan<byte> bytes )
        {
            var encoder = new Base85Encoder(Settings);
            Span<char> outputBuffer = stackalloc char[1024];
            StringBuilder sb = new StringBuilder();

            int inputOffset = 0;
            int inputCount = bytes.Length;
            ConvertStatus convertStatus;

            do
            {
                convertStatus = encoder.ConvertData(bytes, inputOffset, inputCount,
                                                    outputBuffer, 0, outputBuffer.Length,
                                                    true,
                                                    out var inputUsed,
                                                    out var outputUsed);

                if (outputUsed > 0)
                {
                    fixed (char* charPtr = &outputBuffer[0])
                        sb.Append(charPtr, outputUsed);
                }

                inputOffset += inputUsed;
                inputCount -= inputUsed;

            } while (!convertStatus);

            return sb.ToString();
        }

        public override byte[] GetBytes ( ReadOnlySpan<char> chars )
        {
            // GetBytes converts in two passes so that it can perform a single allocation
            // for the final array of bytes...

            int bytesRequired = GetByteCount(chars);
            if (bytesRequired == 0)
                return Array.Empty<byte>();

            var outputBytes = new byte[bytesRequired];

            var decoder = new Base85Decoder(Settings);
            var convertStatus = decoder.ConvertData(chars, 0, chars.Length,
                                                    outputBytes.AsSpan(), 0, outputBytes.Length,
                                                    true,
                                                    out var inputUsed,
                                                    out var outputUsed);

            System.Diagnostics.Debug.Assert(convertStatus);
            System.Diagnostics.Debug.Assert(inputUsed == chars.Length);
            System.Diagnostics.Debug.Assert(outputUsed == outputBytes.Length);

            return outputBytes;
        }

        public override int GetMaxByteCount ( int charCount )
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount));

            // if we support Z or Y abbreviations, then every character could be 5 bytes..
            if ((Settings.ZeroQuantumCharacter.HasValue) || (Settings.SpacesQuantumCharacter.HasValue))
                return charCount * 5;

            // otherwise every 5 characters becomes 4 bytes...
            return ((charCount / 5) * 4) + PadInfo[PadInfoCharsToBytes, (charCount % 5)];
        }

        public override int GetMaxCharCount ( int byteCount )
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount));

            int maxChars = ((byteCount >> 2) * 5) + Settings.EncodingAffixLength;
            int leftOverBytes = (byteCount & 0x03);
            if (leftOverBytes == 0)
                return maxChars;

            if (!Settings.EncodingTruncateFinalQuantum)
                maxChars += 5;
            else
                maxChars += PadInfo[PadInfoBytesToChars, leftOverBytes];

            return maxChars;
        }

        public override int MinimumInputBuffer => Settings.DecodingMinimumInputBuffer;
        public override int MinimumOutputBuffer => 1;

        public Base85Settings Settings { get; }
    }
}
