using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fs.Binary.Codecs.Base85;

namespace Fs.Binary.Codecs.Tests
{
    [TestClass]
    public class Base85Tests
    {
        private static readonly BinaryCodec Codec = BinaryCodecs.Base85Standard;
        private static readonly BinaryCodec Ascii85 = BinaryCodecs.Ascii85;
        private static readonly BinaryCodec BtoA = BinaryCodecs.BtoA;

        [TestMethod]
        public void Base85GetCharsAndGetBytesUpTo2048Bytes ()
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
        public void Base85MaxCharCountAndMaxByteCountReturnCorrectishValues ()
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
        public void Base85GetCharCountAndGetByteCountReturnCorrectValues ()
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

        [TestMethod]
        public void Ascii85MaxCharCountAndMaxByteCountReturnCorrectishValues ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Ascii85.GetString(block);
                Assert.IsNotNull(encodedText);

                Assert.IsTrue(encodedText.Length <= Ascii85.GetMaxCharCount(blockSize));
                Assert.IsTrue(Ascii85.GetMaxByteCount(encodedText.Length) >= blockSize);
            }

        }

        [TestMethod]
        public void Ascii85GetCharsAndGetBytesUpTo2048Bytes ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Ascii85.GetString(block);
                Assert.IsNotNull(encodedText);

                byte[] decodedBytes = Ascii85.GetBytes(encodedText);
                CollectionAssert.AreEqual(block, decodedBytes);
            }
        }

        [TestMethod]
        public void Ascii85GetCharCountAndGetByteCountReturnCorrectValues ()
        {
            var random = new Random(10);

            for (int blockSize = 0; blockSize < 2048; blockSize++)
            {
                byte[] block = new byte[blockSize];
                for (int iIndex = 0; iIndex < blockSize; iIndex++)
                    block[iIndex] = (byte)random.Next(256);

                string encodedText = Ascii85.GetString(block);
                Assert.IsNotNull(encodedText);

                Assert.IsTrue(encodedText.Length == Ascii85.GetCharCount(block));
                Assert.IsTrue(Ascii85.GetByteCount(encodedText) == blockSize);
            }
        }

        [TestMethod]
        public void Ascii85NoAbbreviationInFinalQuantum ()
        {
            Assert.AreEqual("<~z!!~>", Ascii85.GetString(new byte[] { 0, 0, 0, 0, 0 }));
        }

        [TestMethod]
        public void BtoAAbbreviates ()
        {
            Assert.AreEqual("z", BtoA.GetString(new byte[] { 0, 0, 0, 0 }));
            Assert.AreEqual("y", BtoA.GetString(new byte[] { 0x20, 0x20, 0x20, 0x20 }));
            Assert.AreEqual("zy", BtoA.GetString(new byte[] { 0, 0, 0, 0, 0x20, 0x20, 0x20, 0x20 }));
        }
    }
}
