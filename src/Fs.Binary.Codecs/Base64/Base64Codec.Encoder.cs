using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Base64
{
    public partial class Base64Codec
    {
        private class Encoder : BinaryEncoder
        {
            private Base64Encoder _encoder;

            public Encoder ( Base64Settings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                _encoder = new Base64Encoder(settings);
            }

            public override void Reset ()
            {
                _encoder.Reset();
            }

            protected override ConvertStatus ConvertData ( ReadOnlySpan<byte> inputData, int inputIndex, int inputCount, Span<char> outputData, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed )
            {
                return _encoder.ConvertData(inputData, inputIndex, inputCount,
                                            outputData, outputIndex, outputCount,
                                            flush,
                                            out inputUsed,
                                            out outputUsed);
            }
        }
    }
}
