using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Streaming
{
    public class BinaryEncoderReader : TextReader
    {
        private const int DefaultBufferSize = 4096;

        private readonly Stream _inputStream;
        private readonly BinaryCodec _codec;
        private readonly bool _closeStream;

        public BinaryEncoderReader ( BinaryCodec codec, Stream inputStream, int? bufferSize = null, bool closeStream = true )
        {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));

            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            _codec = codec;
            _inputStream = inputStream;
            _closeStream = closeStream;

            int actualBufferSize = bufferSize ?? DefaultBufferSize;
            //if (actualBufferSize < _codec.MinimumInputBuffer)


        }

        protected override void Dispose ( bool disposing )
        {
            try
            {
                if ((_closeStream) && (disposing))
                    _inputStream.Close();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int Read ( char[] buffer, int index, int count )
        {


            return base.Read(buffer, index, count);
        }
    }
}
