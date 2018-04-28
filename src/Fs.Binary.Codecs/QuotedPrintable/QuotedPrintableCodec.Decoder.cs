using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public partial class QuotedPrintableCodec
    {
        private class Decoder: BinaryDecoder
        {
            private QpDecoder _codecDecoder;

            public Decoder ( QuotedPrintableSettings settings )
            {
                _codecDecoder = new QpDecoder(settings);
            }

            public override void Reset ()
            {
                _codecDecoder.Reset();
            }

            protected override ConvertStatus ConvertData ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount, Span<byte> outputData, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed )
            {
                return _codecDecoder.ConvertData(inputData, inputIndex, inputCount,
                                                 outputData, outputIndex, outputCount,
                                                 flush,
                                                 out inputUsed,
                                                 out outputUsed);
            }
        }
    }
}
