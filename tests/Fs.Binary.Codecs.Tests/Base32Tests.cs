using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fs.Binary.Codecs.Tests
{
    [TestClass]
    public class Base32Tests
    {
        private static readonly BinaryCodec Codec = BinaryCodecs.Base32Standard;

        [TestMethod]
        public void Base32GetCharsAndGetBytesUpTo2048Bytes ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Codec.GetString(block);
                Assert.IsNotNull(encodedText);

                byte[] decodedBytes = Codec.GetBytes(encodedText);
                CollectionAssert.AreEqual(block, decodedBytes);
            }
        }

        [TestMethod]
        public void Base32MaxCharCountAndMaxByteCountReturnCorrectishValues ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Codec.GetString(block);
                Assert.IsNotNull(encodedText);

                Assert.IsTrue(encodedText.Length <= Codec.GetMaxCharCount(blockSize), $"GetMaxCharCount invalid at {blockSize}");
                Assert.IsTrue(Codec.GetMaxByteCount(encodedText.Length) >= blockSize, $"GetMaxByteCount invalid at {blockSize}");
            }
        }

        [TestMethod]
        public void Base32GetCharCountAndGetByteCountReturnCorrectValues ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Codec.GetString(block);
                Assert.IsNotNull(encodedText);

                Assert.IsTrue(encodedText.Length == Codec.GetCharCount(block));
                Assert.IsTrue(Codec.GetByteCount(encodedText) == blockSize);
            }
        }

    }
}
