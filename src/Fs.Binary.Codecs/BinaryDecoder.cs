using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs
{
    /// <summary>
    /// A base class for all binary-to-text decoders.
    /// </summary>
    public abstract class BinaryDecoder
    {
        /// <summary>
        /// Decodes a sequence of characters into a sequence of bytes.
        /// </summary>
        /// <param name="inputData">The array containing the sequence of characters to decode.</param>
        /// <param name="inputIndex">The index of the first character to decode.</param>
        /// <param name="inputCount">The number of characters to decode.</param>
        /// <param name="outputData">The array where the output bytes will be stored.</param>
        /// <param name="outputIndex">The index of the first output byte.</param>
        /// <param name="outputCount">The number of bytes that can be stored in the output.</param>
        /// <param name="flush"><c>true</c> if no more input will be available; otherwise, <c>false</c>.</param>
        /// <param name="inputUsed">On return, the number of characters consumed from <paramref name="inputData"/>.</param>
        /// <param name="outputUsed">On return, the number of bytes written to <paramref name="outputData"/>.</param>
        /// <returns>A <see cref="ConvertStatus"/> indicating the status of the conversion.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputData"/> or <paramref name="outputData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="inputIndex"/> or <paramref name="outputIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputData"/>, <paramref name="inputIndex"/> and <paramref name="inputCount"/> create an invalid combination. -or- 
        /// <paramref name="outputData"/>, <paramref name="outputIndex"/> and <paramref name="outputCount"/> create an invalid combination.</exception>
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

        /// <summary>
        /// Decodes a sequence of characters into a sequence of bytes.
        /// </summary>
        /// <param name="inputData">The array containing the sequence of characters to decode.</param>
        /// <param name="inputIndex">The index of the first character to decode.</param>
        /// <param name="inputCount">The number of characters to decode.</param>
        /// <param name="outputData">The array where the output bytes will be stored.</param>
        /// <param name="outputIndex">The index of the first output byte.</param>
        /// <param name="outputCount">The number of bytes that can be stored in the output.</param>
        /// <param name="flush"><c>true</c> if no more input will be available; otherwise, <c>false</c>.</param>
        /// <param name="inputUsed">On return, the number of characters consumed from <paramref name="inputData"/>.</param>
        /// <param name="outputUsed">On return, the number of bytes written to <paramref name="outputData"/>.</param>
        /// <returns>A <see cref="ConvertStatus"/> indicating the status of the conversion.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputData"/> or <paramref name="outputData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="inputIndex"/> or <paramref name="outputIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputData"/>, <paramref name="inputIndex"/> and <paramref name="inputCount"/> create an invalid combination. -or- 
        /// <paramref name="outputData"/>, <paramref name="outputIndex"/> and <paramref name="outputCount"/> create an invalid combination.</exception>
        public ConvertStatus Convert ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount,
                                       Span<byte> outputData, int outputIndex, int outputCount,
                                       bool flush,
                                       out int inputUsed,
                                       out int outputUsed )
        {
            if ((inputIndex < 0) || (outputIndex < 0))
                throw new ArgumentOutOfRangeException((inputIndex < 0) ? nameof(inputIndex) : nameof(outputIndex));

            if ((inputCount < 0) || (outputCount < 0))
                throw new ArgumentOutOfRangeException((inputCount < 0) ? nameof(inputCount) : nameof(outputCount));

            if ((inputData.Length - inputIndex < inputCount) || (outputData.Length - outputIndex < outputCount))
                throw new ArgumentException(Resources.InvalidIndexCountLength);

            return ConvertData(inputData, inputIndex, inputCount,
                               outputData, outputIndex, outputCount,
                               flush,
                               out inputUsed,
                               out outputUsed);
        }

        /// <summary>
        /// Resets the state of the decoder.
        /// </summary>
        public abstract void Reset ();

        /// <summary>
        /// Decodes a sequence of characters into a sequence of bytes.
        /// </summary>
        /// <param name="inputData">The array containing the sequence of characters to decode.</param>
        /// <param name="inputIndex">The index of the first character to decode.</param>
        /// <param name="inputCount">The number of characters to decode.</param>
        /// <param name="outputData">The array where the output bytes will be stored.</param>
        /// <param name="outputIndex">The index of the first output byte.</param>
        /// <param name="outputCount">The number of bytes that can be stored in the output.</param>
        /// <param name="flush"><c>true</c> if no more input will be available; otherwise, <c>false</c>.</param>
        /// <param name="inputUsed">On return, the number of characters consumed from <paramref name="inputData"/>.</param>
        /// <param name="outputUsed">On return, the number of bytes written to <paramref name="outputData"/>.</param>
        /// <returns>A <see cref="ConvertStatus"/> indicating the status of the conversion.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputData"/> or <paramref name="outputData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="inputIndex"/> or <paramref name="outputIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputData"/>, <paramref name="inputIndex"/> and <paramref name="inputCount"/> create an invalid combination. -or- 
        /// <paramref name="outputData"/>, <paramref name="outputIndex"/> and <paramref name="outputCount"/> create an invalid combination.</exception>
        protected abstract ConvertStatus ConvertData ( ReadOnlySpan<char> inputData, int inputIndex, int inputCount,
                                                      Span<byte> outputData, int outputIndex, int outputCount,
                                                      bool flush,
                                                      out int inputUsed,
                                                      out int outputUsed );
    }
}
