using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs;
using Fs.Binary.Codecs.Base16;
using Fs.Binary.Codecs.Base32;
using Fs.Binary.Codecs.Base64;
using Fs.Binary.Codecs.Base85;

namespace TestConsoleApp
{
    class Program
    {
        private static void Decode ( BinaryCodec encoding, TextReader inputReader, Stream outputStream )
        {
            char[] inputBuffer = new char[encoding.MinimumInputBuffer];
            byte[] outputBuffer = new byte[encoding.MinimumOutputBuffer];

            bool readEof = false;
            int inputBufferEnd = 0;
            int outputBufferEnd = 0;
            int inputBufferUsed;
            int outputBufferUsed;

            BinaryDecoder decoder = encoding.GetDecoder();
            while (true)
            {
                if ((inputBufferEnd < inputBuffer.Length) && (!readEof))
                {
                    int charsRead = inputReader.Read(inputBuffer, inputBufferEnd, inputBuffer.Length - inputBufferEnd);
                    if (charsRead == 0)
                        readEof = true;

                    inputBufferEnd += charsRead;
                }

                // stop when we've read EOF and Convert returns true..
                bool finished = ((decoder.Convert(inputBuffer, 0, inputBufferEnd,
                                                  outputBuffer, 0, outputBuffer.Length,
                                                  readEof,
                                                  out inputBufferUsed,
                                                  out outputBufferUsed)) && (readEof));

                // dump any output produced to outputWriter..
                outputStream.Write(outputBuffer, 0, outputBufferUsed);

                if (finished)
                    break;

                // shift input as needed..
                if (inputBufferUsed != 0)
                {
                    if (inputBufferUsed < inputBufferEnd)
                    {
                        Buffer.BlockCopy(inputBuffer, inputBufferUsed * sizeof(char), inputBuffer, 0, (inputBufferEnd - inputBufferUsed) * sizeof(char));
                        inputBufferEnd -= inputBufferUsed;
                    }
                    else
                        inputBufferEnd = 0;
                }
            }
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

        static string Encode ( BinaryCodec encoding, byte[] bytes )
        {
            var inputStream = new MemoryStream(bytes, false);
            var outputWriter = new StringWriter();

            Encode(encoding, inputStream, outputWriter);
            return outputWriter.ToString();
        }

        static byte[] Decode ( BinaryCodec encoding, string encodedText )
        {
            var inputReader = new StringReader(encodedText);
            var outputStream = new MemoryStream();
            Decode(encoding, inputReader, outputStream);
            return outputStream.ToArray();
        }

        static void RoundTriping ( BinaryCodec encoding )
        {
            var random = new Random(10);

            for (int inputLength = 1; inputLength <= 1024; inputLength++)
            {
                var inputBytes = new byte[inputLength];
                for (int iIndex = 0; iIndex < inputBytes.Length; iIndex++)
                    inputBytes[iIndex] = (byte)random.Next(256);

                var encodedStringWriter = new StringWriter();

                Encode(encoding,
                       new MemoryStream(inputBytes),
                       encodedStringWriter);

                var encodedText = encodedStringWriter.ToString();
                var outputStream = new MemoryStream();

                Console.Write($"{inputLength} bytes: ");
                Console.WriteLine(encodedText);

                Decode(encoding,
                       new StringReader(encodedText),
                       outputStream);

                var decodedBytes = outputStream.ToArray();
                if (decodedBytes.Length != inputLength)
                {
                    Console.WriteLine($"{inputLength} - decoded {decodedBytes.Length} bytes");
                    return;
                }

                for (int iIndex = 0; iIndex < inputLength; iIndex++)
                {
                    if (decodedBytes[iIndex] != inputBytes[iIndex])
                    {
                        Console.WriteLine($"{inputLength} - incorrect byte at {iIndex}");
                        return;
                    }
                }
            }
        }

        private static void StreamTest ( BinaryCodec codec )
        {
            var random = new Random(10);

            byte[] inputData = new byte[10240];
            for (int iIndex = 0; iIndex < inputData.Length; iIndex++)
                inputData[iIndex] = (byte)random.Next(256);

            StringWriter outputWriter = new StringWriter();
            using (BinaryEncoderStream codecStream = new BinaryEncoderStream(codec, outputWriter))
            {
                int inputOffset = 0;
                int inputEnd = 0;

                while (inputOffset < inputData.Length)
                {
                    inputEnd = Math.Min(inputData.Length, inputEnd + 12); // (byte)random.Next(1, 128));

                    codecStream.Write(inputData, inputOffset, inputEnd - inputOffset);
                    inputOffset = inputEnd;
                }
            } 

            var encodedText = outputWriter.ToString();
            var decodedBytes = Decode(codec, encodedText);
            var inputLength = inputData.Length;

            if (decodedBytes.Length != inputLength)
            {
                Console.WriteLine($"{inputLength} - decoded {decodedBytes.Length} bytes");
                return;
            }

            for (int iIndex = 0; iIndex < inputLength; iIndex++)
            {
                if (decodedBytes[iIndex] != inputData[iIndex])
                {
                    Console.WriteLine($"{inputLength} - incorrect byte at {iIndex}");
                    return;
                }
            }



        }

        static void DecodeTest ()
        {
            var inputString = "A/"; //"=====";// ================";
            var inputReader = new StringReader(inputString);
            var outputStream = new MemoryStream();

            Decode(BinaryCodecs.Base64Standard, inputReader, outputStream);
        }


        static void SpanTest ()
        {
        }

        static void Main ( string[] args )
        {
            //StreamTest(Base85Codec.Standard);

            //var sb = new StringBuilder();
            //for (int iIndex = 33; iIndex <= 60; iIndex++)
            //    sb.Append((char)iIndex);

            //for (int iIndex = 62; iIndex <= 126; iIndex++ )
            //    sb.Append((char)iIndex);

            //if (sb.ToString() == "")
            //    ;

            RoundTriping(BinaryCodecs.Ascii85);

            var rand = new Random(10);

            byte[] bs = new byte[400];
            for (int i = 0; i < bs.Length; i++)
                bs[i] = (byte)rand.Next(256);

            for (int i = 0; i < 8; i++)
                Console.Write("1234567890");

            Console.WriteLine();
            for (int i = 0; i < 8; i++)
                Console.Write($"         {i+1}");

            Console.WriteLine();


            Console.WriteLine(BinaryCodecs.QuotedPrintable.GetBytes("          "));

            Console.WriteLine(BinaryCodecs.QuotedPrintable.GetString(bs));

            //Console.WriteLine(Base85Codec.Standard.GetString(new byte[] { 0, 0, 0, 0 }));
            ////            Console.WriteLine(Encode(Base32BinaryEncoding.Standard, new byte[] { 0x31, 0x32, 0x33 }));
            //byte[] bs = null;
            //////bs = Decode(new Base16BinaryEncoding(new Base16Settings { DecodingIgnoreInvalidFinalQuantum = true }), "F0F");
            //bs = Decode(Base85Codec.Standard, "o)");
            //Console.WriteLine($"{bs.Length}");

            //bs = Encoding.ASCII.GetBytes("\0\0\0\0");
            //var encoded = Encode(Base85Codec.Standard, bs);
            //Console.WriteLine(encoded);

            //var bs2 = Decode(Base85Codec.Standard, encoded);

            Console.WriteLine("Done");
            Console.ReadLine();

        }
    }
}
