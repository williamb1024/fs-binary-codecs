using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fs.Binary.Codecs;

namespace Fs.Binary.Codecs.Tests
{
    [TestClass]
    public class Base16Tests
    {
        private static BinaryCodec Codec = BinaryCodecs.Base16Standard;

        [TestMethod]
        public void TestMethod1 ()
        {
            Assert.AreEqual("00", TestHelpers.Encode(Codec, new byte[] { 0 }));
            Assert.AreEqual("0001", TestHelpers.Encode(Codec, new byte[] { 0, 1 }));
            Assert.AreEqual("000102", TestHelpers.Encode(Codec, new byte[] { 0, 1, 2 }));
            Assert.AreEqual("00010203", TestHelpers.Encode(Codec, new byte[] { 0, 1, 2, 3 }));
        }

        [TestMethod]
        public void Base16GetCharsAndGetBytesUpTo2048Bytes ()
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
        public void Base16MaxCharCountAndMaxByteCountReturnCorrectishValues ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Codec.GetString(block);
                Assert.IsNotNull(encodedText);

                Assert.IsTrue(encodedText.Length <= Codec.GetMaxCharCount(blockSize));
                Assert.IsTrue(Codec.GetMaxByteCount(encodedText.Length) >= blockSize);
            }
        }

        [TestMethod]
        public void Base16GetCharCountAndGetByteCountReturnCorrectValues ()
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
