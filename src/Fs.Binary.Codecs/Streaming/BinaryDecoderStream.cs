using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fs.Binary.Codecs.Streaming
{
    public class BinaryDecoderStream : Stream
    {
        private const int DefaultInputBufferLength = 1024;

        private TextReader _inputReader;
        private readonly BinaryDecoder _decoder;

        private long _outputTotal;
        private int _inputBufferOffset;
        private int _inputBufferEnd;
        private ConvertStatus _convertStatus;
        private char[] _inputBuffer;
        private bool _closeReader;
        private bool _isBlocked;
        private bool _isEos;

        public BinaryDecoderStream ( BinaryCodec codec, TextReader inputReader, int? bufferLength = null, bool closeReader = true )
        {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));

            if (inputReader == null)
                throw new ArgumentNullException(nameof(inputReader));

            int inputBufferLength = bufferLength ?? DefaultInputBufferLength;
            if (inputBufferLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferLength));

            inputBufferLength = Math.Max(inputBufferLength, codec.MinimumInputBuffer);

            // all of the codecs currently have a minimumOutputBuffer of 1, this code relies
            // on that knowledge.. so let's check that it's true..
            System.Diagnostics.Debug.Assert(codec.MinimumOutputBuffer == 1);

            _convertStatus = ConvertStatus.InputRequired;
            _decoder = codec.GetDecoder();
            _inputReader = inputReader;
            _inputBuffer = new char[inputBufferLength];
            _closeReader = closeReader;
        }

        protected override void Dispose ( bool disposing )
        {
            try
            {
                if (_closeReader && (_inputReader != null)) _inputReader.Close();
            }
            finally
            {
                _inputReader = null;
                _inputBuffer = null;
                base.Dispose(disposing);
            }
        }

        public override int Read ( byte[] buffer, int offset, int count )
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (buffer.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            if (_inputReader == null)
                throw new ObjectDisposedException(GetType().Name);

            // nothing to read after ConverStatus becomes Complete..
            if (_convertStatus == ConvertStatus.Complete)
                return 0;

            int totalRead = 0;

            while (count > 0)
            {
                if (_convertStatus == ConvertStatus.InputRequired)
                    FillInputBuffer();

                _convertStatus = _decoder.Convert(_inputBuffer, _inputBufferOffset, _inputBufferEnd - _inputBufferOffset,
                                                  buffer, offset, count,
                                                  _isEos,
                                                  out var inputUsed,
                                                  out var outputUsed);

                _inputBufferOffset += inputUsed;
                offset += outputUsed;
                count -= outputUsed;
                totalRead += outputUsed;

                // if we've run out of output buffer or finished converting, then stop looping..
                if ((_convertStatus == ConvertStatus.OutputRequired) || (_convertStatus == ConvertStatus.Complete))
                    break;

                // if we've already had a partial read from the input, then we'll stop and wait
                // for the next call to Read to access the underlying input reader again (unless we
                // haven't produced any output yet, in which case we need to continue reading)

                if ((_isBlocked) && (totalRead > 0))
                    break;
            }

            _isBlocked = false;
            _outputTotal += totalRead;
            return totalRead;
        }

        public override void Write ( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }

        public override void Flush ()
        {
            // ignored..
        }

        public override long Seek ( long offset, SeekOrigin origin )
        {
            if ((offset == 0) && (origin == SeekOrigin.Current))
                return _outputTotal;

            throw new NotSupportedException();
        }

        public override void SetLength ( long value )
        {
            throw new NotSupportedException();
        }

        private void FillInputBuffer ()
        {
            if (_inputBufferOffset > 0)
            {
                // shift the contents of the input buffer such that all available space is at
                // the end of the buffer..

                Buffer.BlockCopy(_inputBuffer, _inputBufferOffset * sizeof(char), _inputBuffer, 0, (_inputBufferEnd - _inputBufferOffset) * sizeof(char));
                _inputBufferEnd -= _inputBufferOffset;
                _inputBufferOffset = 0;
            }

            int charsToRead = _inputBuffer.Length - _inputBufferEnd;
            int charsRead = _inputReader.Read(_inputBuffer, _inputBufferEnd, charsToRead);

            _isEos = (charsRead == 0);
            _isBlocked = ((charsRead != 0) && (charsRead < charsToRead));
            _inputBufferEnd += charsRead;
        }

        public override bool CanRead => (_inputReader != null);

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _outputTotal;

        public override long Position
        {
            get => _outputTotal;
            set => throw new NotSupportedException();
        }
    }
}
