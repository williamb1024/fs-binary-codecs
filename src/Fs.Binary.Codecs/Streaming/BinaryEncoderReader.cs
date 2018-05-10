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
        private readonly Stream _inputStream;
        private readonly bool _closeStream;

        public BinaryEncoderReader ( BinaryCodec codec, Stream inputStream, int? bufferSize = null, bool closeStream = true )
        {
            _closeStream = closeStream;
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
