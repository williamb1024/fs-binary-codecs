using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Base64
{
    public partial class Base64Codec : BinaryCodec
    {
        private const int PadInfoRequiredPadding = 0;
        private const int PadInfoUnusedBits = 1;
        private const int PadInfoEncodedBytes = 2;
        private const int PadInfoBytesToChars = 3;

        private static readonly byte[,] PadInfo = new byte[,]
        {
            { 0, 0, 2, 1 },
            { 0, 6, 4, 2 },
            { 0, 0, 1, 2 },
            { 0, 2, 3, 4 },
        };

        public Base64Codec ( Base64Settings settings )
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
            var encoder = new Base64Encoder(Settings);
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
            var decoder = new Base64Decoder(Settings);
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

            var encoder = new Base64Encoder(Settings);
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
            var encoder = new Base64Encoder(Settings);
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

            var decoder = new Base64Decoder(Settings);
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

            // every 4 characters is 3 bytes, plus straglers..
            return ((charCount >> 2) * 3) + PadInfo[PadInfoEncodedBytes, charCount & 0x03];
        }

        public override int GetMaxCharCount ( int byteCount )
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount));

            int maxChars = ((byteCount / 3) * 4) + Settings.EncodingAffixLength;

            int leftOverBytes = byteCount % 3;
            if (leftOverBytes == 0)
                return maxChars;

            if (Settings.PaddingCharacter.HasValue)
                maxChars += 4;
            else
                maxChars += PadInfo[PadInfoBytesToChars, leftOverBytes];

            return maxChars;
        }

        public override int MinimumInputBuffer => Settings.DecodingMinimumInputBuffer;
        public override int MinimumOutputBuffer => 1;

        public Base64Settings Settings { get; }
    }
}
