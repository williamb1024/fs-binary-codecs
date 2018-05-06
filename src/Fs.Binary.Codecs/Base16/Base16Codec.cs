using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fs.Binary.Codecs.Base16
{
    public partial class Base16Codec : BinaryCodec
    {
        public Base16Codec ( Base16Settings settings )
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
            var encoder = new Base16Encoder(Settings);
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
            var decoder = new Base16Decoder(Settings);
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

            var encoder = new Base16Encoder(Settings);
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
            var encoder = new Base16Encoder(Settings);
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

            var decoder = new Base16Decoder(Settings);
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

            return (charCount >> 1);
        }

        public override int GetMaxCharCount ( int byteCount )
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount));

            return (byteCount << 1) + Settings.EncodingAffixLength;
        }

        public override int MinimumInputBuffer => Settings.DecodingMinimumInputBuffer;
        public override int MinimumOutputBuffer => 1;

        public Base16Settings Settings { get; }
    }
}
