﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Settings;

namespace Fs.Binary.Codecs.Base16
{
    public partial class Base16Codec
    {
        private class Decoder: BinaryDecoder
        {
            private Base16Decoder _codecDecoder;

            public Decoder ( Base16Settings settings )
            {
                _codecDecoder = new Base16Decoder(settings);
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
