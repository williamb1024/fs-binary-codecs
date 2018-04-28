using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base85
{
    public partial class Base85Codec
    {
        private class Decoder : BinaryDecoder
        {
            private Base85Decoder _codecDecoder;

            public Decoder ( Base85Settings settings )
            {
                _codecDecoder = new Base85Decoder(settings);
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
