using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base32
{
    public partial class Base32Codec
    {
        private class Decoder : BinaryDecoder
        {
            private Base32Decoder _codecDecoder;

            public Decoder ( Base32Settings settings )
            {
                _codecDecoder = new Base32Decoder(settings);
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
