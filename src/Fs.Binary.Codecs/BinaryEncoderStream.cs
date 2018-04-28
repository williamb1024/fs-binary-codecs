using System;
using System.IO;

namespace Fs.Binary.Codecs
{
    public class BinaryEncoderStream : Stream
    {
        private const int DefaultOutputBufferSize = 1024;

        private readonly TextWriter _outputWriter;
        private readonly BinaryEncoder _encoder;

        private char[] _outputBuffer;
        private byte[] _inputBuffer;
        private long _inputTotal; 
        private int _inputBufferEnd;
        private int _inputBufferLength;
        private bool _closeWriter;
        private bool _hasFlushed;

        public BinaryEncoderStream ( BinaryCodec codec, TextWriter outputWriter, int? bufferLength = null, bool closeWriter = true )
        {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));

            if (outputWriter == null)
                throw new ArgumentNullException(nameof(outputWriter));

            int outputBufferLength = bufferLength ?? DefaultOutputBufferSize;
            if ((outputBufferLength <= 0) || (outputBufferLength < codec.MinimumOutputBuffer))
                throw new ArgumentOutOfRangeException(nameof(bufferLength));

            _encoder = codec.GetEncoder();
            _outputWriter = outputWriter;
            _inputBufferLength = codec.MinimumInputBuffer;
            _outputBuffer = new char[outputBufferLength];
            _closeWriter = closeWriter;
        }

        protected override void Dispose ( bool disposing )
        {
            try
            {
                if (disposing)
                {
                    try
                    {
                        Flush();
                    }
                    finally
                    {
                        if (_closeWriter) _outputWriter.Close();
                    }
                }
            }
            finally
            {
                _outputBuffer = null;
                _inputBuffer = null;
                base.Dispose(disposing);
            }
        }

        public override void Write ( byte[] buffer, int offset, int count )
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (buffer.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            // can't make progress without input...
            if (count == 0)
                return;

            // keep track of the number of bytes we've "written"
            _inputTotal += count;

            int inputUsed;
            int outputUsed;

            while (true)
            {
                if (_inputBufferEnd == 0)
                {
                    // there is no data in the inputBuffer, we can convert directly from the supplied
                    // buffer..

                    while (true)
                    {
                        var result = _encoder.Convert(buffer, offset, count,
                                                      _outputBuffer, 0, _outputBuffer.Length,
                                                      false,
                                                      out inputUsed,
                                                      out outputUsed);

                        // write any output that was produced..
                        if (outputUsed > 0) _outputWriter.Write(_outputBuffer, 0, outputUsed);

                        // adjust counters..
                        offset += inputUsed;
                        if ((count -= inputUsed) == 0)
                            return;

                        // if we need more input to continue, then we're done..
                        if (result == ConvertStatus.InputRequired)
                            break;
                    }

                    // if we reach this code, there is unused data in buffer that we need to save
                    // for the next call to Write(). For all of the current BaseXX converters, this
                    // should never happen.

                    System.Diagnostics.Debug.Assert(count < _inputBufferLength);

                    if (_inputBuffer == null)
                        _inputBuffer = new byte[_inputBufferLength];

                    System.Diagnostics.Debug.Assert(_inputBufferEnd == 0);
                    Buffer.BlockCopy(buffer, offset, _inputBuffer, _inputBufferEnd, count);
                    _inputBufferEnd += count;

                    // no more data will be handled..
                    return;
                }
                else
                {
                    // we have existing data in the inputBuffer, add incoming data to the end
                    // and try to convert. The goal is to get out of this loop and return to
                    // converting directly from the buffer parameter.

                    int inputBufferOffset = 0;
                    int bytesToCopy = Math.Min(count, _inputBuffer.Length - _inputBufferEnd);
                    System.Diagnostics.Debug.Assert(bytesToCopy > 0);

                    // copy data from buffer to inputBuffer and adjust offset/count values..
                    Buffer.BlockCopy(buffer, offset, _inputBuffer, _inputBufferEnd, bytesToCopy);
                    _inputBufferEnd += bytesToCopy;

                    while (true)
                    {
                        var result = _encoder.Convert(_inputBuffer, inputBufferOffset, _inputBufferEnd - inputBufferOffset,
                                                      _outputBuffer, 0, _outputBuffer.Length,
                                                      false,
                                                      out inputUsed,
                                                      out outputUsed);

                        if (outputUsed > 0) _outputWriter.Write(_outputBuffer, 0, outputUsed);

                        inputBufferOffset += inputUsed;
                        if (result == ConvertStatus.InputRequired)
                        {
                            // encoder needs more input, if we've processed enough of the inputBuffer to overlap
                            // the incoming buffer then we want to switch to the other encoding loop.. no point in
                            // copying data if we don't have to.

                            if (inputBufferOffset >= _inputBufferEnd - bytesToCopy)
                            {
                                int bytesUsed = inputBufferOffset - (_inputBufferEnd - bytesToCopy);

                                // reset inputBuffer and adjust offset, if count is zero after adjustment just
                                // return (we've handled all of the input)

                                _inputBufferEnd = 0;
                                offset += bytesUsed;
                                if ((count -= bytesUsed) == 0)
                                    return;

                                break;
                            }

                            // we didn't process enough input to overlap the incoming buffer, we have to make
                            // room in our inputBuffer for new data and keep trying..

                            Buffer.BlockCopy(_inputBuffer, inputBufferOffset, _inputBuffer, 0, _inputBufferEnd - inputBufferOffset);
                            _inputBufferEnd -= inputBufferOffset;
                            inputBufferOffset = 0;

                            // copy new data into the buffer.. we have to update bytesToCopy as well to ensure that
                            // we keep track of where the incoming buffer overlaps the inputBuffer.

                            int newBytesToCopy = Math.Min(count - bytesToCopy, _inputBuffer.Length - _inputBufferEnd);
                            if (newBytesToCopy == 0)
                                return;

                            Buffer.BlockCopy(buffer, offset + bytesToCopy, _inputBuffer, _inputBufferEnd, newBytesToCopy);
                            _inputBufferEnd += newBytesToCopy;
                            bytesToCopy += newBytesToCopy;
                        }
                    }
                }
            }
        }

        public override int Read ( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }

        public override void Flush ()
        {
            if (_hasFlushed)
                return;

            _hasFlushed = true;
            byte[] inputBuffer = _inputBuffer ?? Array.Empty<byte>();
            int inputBufferOffset = 0;

            ConvertStatus result = default(ConvertStatus);
            do
            {
                result = _encoder.Convert(inputBuffer, inputBufferOffset, _inputBufferEnd,
                                          _outputBuffer, 0, _outputBuffer.Length,
                                          true,
                                          out var inputUsed,
                                          out var outputUsed);

                if (outputUsed > 0) _outputWriter.Write(_outputBuffer, 0, outputUsed);

            } while (!result);
        }

        public override long Seek ( long offset, SeekOrigin origin )
        {
            if ((offset == 0L) && (origin == SeekOrigin.Current))
                return _inputTotal;

            throw new NotSupportedException();
        }

        public override void SetLength ( long value )
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => !_hasFlushed;

        public override long Length => _inputTotal;

        public override long Position
        {
            get => _inputTotal;
            set => throw new NotSupportedException();
        }
    }
}
