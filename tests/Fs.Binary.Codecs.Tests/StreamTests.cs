using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs;
using Fs.Binary.Codecs.Streaming;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fs.Binary.Codecs.Tests
{
    [TestClass]
    public class StreamTests
    {
        private static string EncodeThroughStream ( BinaryCodec codec, byte[] data )
        {
            using (var stringWriter = new StringWriter())
            {
                using (var codecStream = new BinaryEncoderStream(codec, stringWriter))
                {
                    var random = new Random(5);

                    int inputOffset = 0;
                    int inputRemaining = data.Length;

                    while (inputRemaining > 0)
                    {
                        int inputToWrite = random.Next(1, inputRemaining);

                        codecStream.Write(data, inputOffset, inputToWrite);
                        inputOffset += inputToWrite;
                        inputRemaining -= inputToWrite;
                    }

                    codecStream.Flush();
                }

                return stringWriter.ToString();
            }
        }

        private static byte[] DecodeThroughStream ( BinaryCodec codec, string inputData )
        {
            using (var memoryStream = new MemoryStream())
            {
                var random = new Random(5);
                byte[] readBuffer = new byte[1024];

                using (var codecStream = new BinaryDecoderStream(codec, new StringReader(inputData)))
                {
                    while (true)
                    {
                        int bytesToRead = random.Next(1, readBuffer.Length);
                        int bytesRead = codecStream.Read(readBuffer, 0, bytesToRead);
                        if (bytesRead == 0)
                            break;

                        memoryStream.Write(readBuffer, 0, bytesRead);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private static byte[] DecodeThroughTextWriter ( BinaryCodec codec, string inputData )
        {
            using (var memoryStream = new MemoryStream())
            {
                var random = new Random(5);
                int inputOffset = 0;
                int inputLength = inputData.Length;

                using (var codecWriter = new BinaryDecoderWriter(codec, memoryStream))
                {
                    while (true)
                    {
                        int charsToProcess = (inputLength > 0) ? random.Next(1, inputLength) : 0;
                        if (charsToProcess == 0)
                            break;

                        codecWriter.Write(inputData.Substring(inputOffset, charsToProcess));
                        inputOffset += charsToProcess;
                        inputLength -= charsToProcess;
                    }

                    codecWriter.Flush();
                }

                return memoryStream.ToArray();
            }
        }

        private static byte[] GenerateData ( int dataLength )
        {
            if (dataLength < 0)
                throw new ArgumentOutOfRangeException(nameof(dataLength));

            if (dataLength == 0)
                return Array.Empty<byte>();

            // we always generate the same data, just more of it..
            var generator = new Random(10);
            var data = new byte[dataLength];
            generator.NextBytes(data);

            return data;
        }

        private static void StreamingTest ( BinaryCodec codec, int maxSize )
        {
            for (int currentSize = 0; currentSize <= maxSize; currentSize++)
            {
                var inputData = GenerateData(currentSize);
                var encodedData = EncodeThroughStream(codec, inputData);
                var writerDecodedData = DecodeThroughTextWriter(codec, encodedData);

                Assert.AreEqual(currentSize, writerDecodedData.Length, "Writer Decoded Length != input length");
                CollectionAssert.AreEqual(inputData, writerDecodedData, "Writer Decoded Data != input data");

                var outputData = codec.GetBytes(encodedData);

                Assert.AreEqual(outputData.Length, inputData.Length, "Output and Input must be same length.");
                CollectionAssert.AreEqual(inputData, outputData, "Output and Input data differ.");

                var decodedData = DecodeThroughStream(codec, encodedData);

                Assert.AreEqual(inputData.Length, decodedData.Length, "Decoded and Input length must match.");
                CollectionAssert.AreEqual(inputData, decodedData, "Decoded and Input data differ.");
            }
        }

        [TestMethod]
        public void Base16Streaming ()
        {
            StreamingTest(BinaryCodecs.Base16Standard, 8192);
        }

        [TestMethod]
        public void Base32Streaming ()
        {
            StreamingTest(BinaryCodecs.Base32Standard, 8192);
        }

        [TestMethod]
        public void Base64Streaming ()
        {
            StreamingTest(BinaryCodecs.Base64Standard, 8192);
        }

        [TestMethod]
        public void Ascii85Streaming ()
        {
            StreamingTest(BinaryCodecs.Ascii85, 8192);
        }

        [TestMethod]
        public void QpStreaming ()
        {
            StreamingTest(BinaryCodecs.QuotedPrintable, 8192);
        }
    }
}
