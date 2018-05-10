using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Streaming
{
    public class BinaryDecoderWriter : TextWriter
    {
        private const int DefaultBufferSize = 1024;

        private readonly BinaryCodec _codec;
        private readonly Stream _outputStream;
        private readonly BinaryDecoder _decoder;
        private readonly bool _closeStream;
        private char[] _inputBuffer;
        private byte[] _outputBuffer;
        private int _inputBufferOffset;
        private int _inputBufferLength;
        private int _outputBufferOffset;
        private int _outputBufferLength;

        public BinaryDecoderWriter ( BinaryCodec codec, Stream outputStream, int? bufferSize = null, bool closeStream = true )
            : base()
        {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));

            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            _codec = codec;
            _decoder = codec.GetDecoder();
            _outputStream = outputStream;
            _closeStream = closeStream;

            _inputBuffer = new char[codec.MinimumInputBuffer];
            _inputBufferOffset = 0;
            _inputBufferLength = _inputBuffer.Length;

            int outputBufferSize = bufferSize ?? DefaultBufferSize;
            if (outputBufferSize < codec.MinimumOutputBuffer)
                outputBufferSize = codec.MinimumOutputBuffer;

            _outputBuffer = new byte[outputBufferSize];
            _outputBufferOffset = 0;
            _outputBufferLength = _outputBuffer.Length;
        }

        protected override void Dispose ( bool disposing )
        {
            try
            {
                if ((disposing) && (!_closeStream))
                    Flush();
            }
            finally
            {
                try
                {
                    if (_closeStream)
                        _outputStream.Close();
                }
                finally
                {
                    _inputBuffer = null;
                    _outputBuffer = null;
                    base.Dispose(disposing);
                }
            }
        }

        public override void Write ( char value )
        {
            Span<char> valueSpan = stackalloc char[1];
            valueSpan[0] = value;

            Write(valueSpan);
        }

        public override void Write ( char[] buffer )
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            Write(buffer.AsSpan());
        }

        public override void Write ( char[] buffer, int index, int count )
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if ((index < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count));

            if (buffer.Length - index < count)
                throw new ArgumentException(Resources.InvalidIndexCountLength);

            Write(buffer.AsSpan(index, count));
        }

        public override void Write ( string value )
        {
            if (value != null) Write(value.AsSpan());
        }

        public override void Flush ()
        {
            ConvertData(_inputBuffer.AsSpan(0, _inputBufferOffset), true);
        }

        private void Write ( ReadOnlySpan<char> value )
        {
            int inputOffset = 0;
            int inputEnd = value.Length;

            // if we have data in the input buffer, then we must copy new data to the buffer and
            // decode from there until we have processed all of the data in the input buffer..

            while (_inputBufferOffset != 0)
            {
                var charsToCopy = Math.Min(_inputBufferLength, inputEnd - inputOffset);
                value.Slice(inputOffset, charsToCopy).CopyTo(_inputBuffer.AsSpan(_inputBufferOffset, _inputBufferLength));

                inputOffset += charsToCopy;
                _inputBufferOffset += charsToCopy;
                _inputBufferLength -= charsToCopy;

                var inputUsed = ConvertData(_inputBuffer.AsSpan(0, _inputBufferOffset), false);
                if (inputUsed == 0)
                {
                    // if there is more data available in value then this is an error. The decoder should always
                    // make progress if inputBuffer is full (which it must be if there is more data available in value)

                    if (inputOffset < inputEnd)
                        throw new InvalidOperationException(Resources.CodecMadeNoProgress);

                    // more data required to continue..
                    return;
                }
                else if (inputUsed < _inputBufferOffset)
                {
                    // shift the input buffer to remove the consumed characters and create space to
                    // get more data on the end..

                    Buffer.BlockCopy(_inputBuffer, inputUsed * sizeof(char),
                                     _inputBuffer, 0,
                                     (_inputBufferOffset - inputUsed) * sizeof(char));

                    _inputBufferOffset -= inputUsed;
                    _inputBufferLength += inputUsed;
                }
                else
                {
                    _inputBufferOffset = 0;
                    _inputBufferLength = _inputBuffer.Length;
                }
            }

            if (inputOffset < inputEnd)
            {
                int inputUsed;

                do
                {
                    inputOffset += (inputUsed = ConvertData(value.Slice(inputOffset, inputEnd - inputOffset), false));

                } while ((inputOffset < inputEnd) && (inputUsed > 0));

                // more input is required to continue, if any input is left over in the buffer then
                // we copy that to our internal buffer..

                if (inputOffset < inputEnd)
                {
                    int charsToCopy = inputEnd - inputOffset;
                    if (charsToCopy > _inputBufferLength)
                        throw new InvalidOperationException(Resources.WouldExceedBuffer);

                    value.Slice(inputOffset, charsToCopy).CopyTo(_inputBuffer.AsSpan(_inputBufferOffset, _inputBufferLength));
                    _inputBufferOffset += charsToCopy;
                    _inputBufferLength -= charsToCopy;
                }
            }
        }

        private int ConvertData ( ReadOnlySpan<char> data, bool flush )
        {
            ConvertStatus convertStatus;
            int inputOffset = 0;
            int inputLength = data.Length;

            do
            {
                convertStatus = _decoder.Convert(data, inputOffset, inputLength,
                                                 _outputBuffer, _outputBufferOffset, _outputBufferLength,
                                                 flush,
                                                 out var inputUsed,
                                                 out var outputUsed);

                inputOffset += inputUsed;
                inputLength -= inputUsed;
                _outputBufferOffset += outputUsed;
                _outputBufferLength -= outputUsed;

                if ((convertStatus == ConvertStatus.OutputRequired) ||
                    (convertStatus == ConvertStatus.Complete))
                {
                    _outputStream.Write(_outputBuffer, 0, _outputBufferOffset);
                    _outputBufferOffset = 0;
                    _outputBufferLength = _outputBuffer.Length;
                }

            } while (convertStatus == ConvertStatus.OutputRequired);

            // return number of characters consumed..
            return inputOffset;
        }

        public virtual Stream BaseStream => _outputStream;
        public override Encoding Encoding => null;
        public virtual BinaryCodec Codec => _codec;
    }
}
