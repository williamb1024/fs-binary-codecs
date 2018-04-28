using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public partial class QuotedPrintableCodec
    {
        private class Encoder: BinaryEncoder
        {
            private QpEncoder _encoder;

            public Encoder ( QuotedPrintableSettings settings )
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                _encoder = new QpEncoder(settings);
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
