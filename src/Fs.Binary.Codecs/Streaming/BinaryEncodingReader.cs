using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Streaming
{
    public class BinaryEncodingReader: TextReader
    {
        public BinaryEncodingReader ( BinaryCodec codec, Stream inputStream, int? bufferSize = null, bool closeStream = true )
        {
        }
    }
}
