using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Tests
{
    public static class TestHelpers
    {
        public static string Encode ( BinaryCodec encoding, byte[] bytes )
        {
            var inputStream = new MemoryStream(bytes, false);
            var outputWriter = new StringWriter();

            Encode(encoding, inputStream, outputWriter);
            return outputWriter.ToString();
        }

        public static string Encode ( BinaryEncoder encoder, int inputBufferSize, byte[] bytes, int outputBufferSize )
        {
            var inputStream = new MemoryStream(bytes, false);
            var outputWriter = new StringWriter();

            Encode(encoder, inputBufferSize, inputStream, outputBufferSize, outputWriter);
            return outputWriter.ToString();
        }

        private static void Encode ( BinaryCodec encoding, Stream inputStream, TextWriter outputWriter )
        {
            byte[] inputBuffer = new byte[encoding.MinimumInputBuffer];
            char[] outputBuffer = new char[encoding.MinimumOutputBuffer];

            bool readEof = false;
            int inputBufferEnd = 0;
            int inputBufferUsed;
            int outputBufferUsed;

            BinaryEncoder encoder = encoding.GetEncoder();

            while (true)
            {
                if ((inputBufferEnd < inputBuffer.Length) && (!readEof))
                {
                    int bytesRead = inputStream.Read(inputBuffer, inputBufferEnd, inputBuffer.Length - inputBufferEnd);
                    if (bytesRead == 0)
                        readEof = true;

                    inputBufferEnd += bytesRead;
                }

                // stop when we've read EOF and Convert returns true..
                bool finished = ((encoder.Convert(inputBuffer, 0, inputBufferEnd,
                                                  outputBuffer, 0, outputBuffer.Length,
                                                  readEof,
                                                  out inputBufferUsed,
                                                  out outputBufferUsed)) && (readEof));

                // dump any output produced to outputWriter..
                outputWriter.Write(outputBuffer, 0, outputBufferUsed);

                if (finished)
                    break;

                // shift input as needed..
                if (inputBufferUsed != 0)
                {
                    if (inputBufferUsed < inputBufferEnd)
                    {
                        Buffer.BlockCopy(inputBuffer, inputBufferUsed, inputBuffer, 0, inputBufferEnd - inputBufferUsed);
                        inputBufferEnd -= inputBufferUsed;
                    }
                    else
                        inputBufferEnd = 0;
                }
            }
        }

        public static void Encode ( BinaryEncoder encoder, int inputBufferSize, Stream inputStream, int outputBufferSize, TextWriter outputWriter )
        {
            byte[] inputBuffer = new byte[inputBufferSize];
            char[] outputBuffer = new char[outputBufferSize];

            bool readEof = false;
            int inputBufferEnd = 0;
            int inputBufferUsed;
            int outputBufferUsed;

            while (true)
            {
                if ((inputBufferEnd < inputBuffer.Length) && (!readEof))
                {
                    int bytesRead = inputStream.Read(inputBuffer, inputBufferEnd, inputBuffer.Length - inputBufferEnd);
                    if (bytesRead == 0)
                        readEof = true;

                    inputBufferEnd += bytesRead;
                }

                // stop when we've read EOF and Convert returns true..
                bool finished = ((encoder.Convert(inputBuffer, 0, inputBufferEnd,
                                                  outputBuffer, 0, outputBuffer.Length,
                                                  readEof,
                                                  out inputBufferUsed,
                                                  out outputBufferUsed)) && (readEof));

                // dump any output produced to outputWriter..
                outputWriter.Write(outputBuffer, 0, outputBufferUsed);

                if (finished)
                    break;

                // shift input as needed..
                if (inputBufferUsed != 0)
                {
                    if (inputBufferUsed < inputBufferEnd)
                    {
                        Buffer.BlockCopy(inputBuffer, inputBufferUsed, inputBuffer, 0, inputBufferEnd - inputBufferUsed);
                        inputBufferEnd -= inputBufferUsed;
                    }
                    else
                        inputBufferEnd = 0;
                }
            }
        }
    }
}
