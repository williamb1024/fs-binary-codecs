using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs
{
    public abstract class BinaryEncoder
    {
        public ConvertStatus Convert ( byte[] inputData, int inputIndex, int inputCount,
                                       char[] outputData, int outputIndex, int outputCount,
                                       bool flush,
                                       out int inputUsed,
                                       out int outputUsed )
        {
            if ((inputData == null) || (outputData == null))
                throw new ArgumentNullException((inputData == null) ? nameof(inputData) : nameof(outputData));

            if ((inputIndex < 0) || (outputIndex < 0))
                throw new ArgumentOutOfRangeException((inputIndex < 0) ? nameof(inputIndex) : nameof(outputIndex));

            if ((inputCount < 0) || (outputCount < 0))
                throw new ArgumentOutOfRangeException((inputCount < 0) ? nameof(inputCount) : nameof(outputCount));

            if ((inputData.Length - inputIndex < inputCount) || (outputData.Length - outputIndex < outputCount))
                throw new ArgumentException(Resources.InvalidIndexCountLength);

            return ConvertData(inputData.AsSpan(), inputIndex, inputCount,
                               outputData.AsSpan(), outputIndex, outputCount,
                               flush,
                               out inputUsed,
                               out outputUsed);
        }

        public abstract void Reset ();

        protected abstract ConvertStatus ConvertData ( ReadOnlySpan<byte> inputData, int inputIndex, int inputCount,
                                                       Span<char> outputData, int outputIndex, int outputCount,
                                                       bool flush,
                                                       out int inputUsed,
                                                       out int outputUsed );
    }
}
