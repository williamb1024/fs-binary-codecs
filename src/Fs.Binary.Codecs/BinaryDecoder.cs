using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs
{
    public abstract class BinaryDecoder
    {
        public ConvertStatus Convert ( char[] inputData, int inputIndex, int inputCount,
                                       byte[] outputData, int outputIndex, int outputCount,
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

        public ConvertStatus Convert ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount,
                                       Span<byte> outputData, int outputIndex, int outputCount,
                                       bool flush,
                                       out int inputUsed,
                                       out int outputUsed )
        {
            // TODO: ...
            if (inputData == null)
                ;

            if ((inputData.IsEmpty) || (outputData.IsEmpty))
                throw new ArgumentException();

            inputUsed = outputUsed = 0;
            return default;
        }

        public abstract void Reset ();

        protected virtual ConvertStatus ConvertData ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount, 
                                                      Span<byte> outputData, int outputIndex, int outputCount, 
                                                      bool flush,
                                                      out int inputUsed,
                                                      out int outputUsed )
        {
            // TODO: make abstract..
            throw new NotSupportedException();
        }

        protected virtual ConvertStatus ConvertData ( char[] inputData, int inputIndex, int inputCount,
                                                       byte[] outputData, int outputIndex, int outputCount,
                                                       bool flush,
                                                       out int inputUsed,
                                                       out int outputUsed )
        {
            // TODO: remove me completely...
            throw new NotImplementedException();
        }
    }
}
