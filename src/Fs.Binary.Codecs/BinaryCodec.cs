using System;
using System.Collections.Generic;
using System.Text;
using Fs.Binary.Codecs.Base16;
using Fs.Binary.Codecs.Base32;
using Fs.Binary.Codecs.Base64;
using Fs.Binary.Codecs.Base85;

namespace Fs.Binary.Codecs
{
    /// <summary>
    /// Base class for all Binary to Text encodings.
    /// </summary>
    public abstract class BinaryCodec
    {
        /// <summary>
        /// Calculates the number of characters produced by encoding all of the bytes from the specified
        /// byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to encode.</param>
        /// <returns>The number of characters produced by encoding the specified sequence of bytes.</returns>
        public virtual int GetCharCount ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetCharCount(bytes.AsSpan());
        }

        /// <summary>
        /// Calculates the number of characters produced by encoding a range of bytes from the specified
        /// byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to encoding.</param>
        /// <param name="offset">The index to the first byte to encode.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <returns>The number of characters produced by encoding the specified sequence.</returns>
        public virtual int GetCharCount ( byte[] bytes, int offset, int count )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (bytes.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetCharCount(bytes.AsSpan(offset, count));
        }

        /// <summary>
        /// Calculates the number of characters produced by encoding a specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <returns>The number of characters produced by encoding the sequence of bytes.</returns>
        public abstract int GetCharCount ( ReadOnlySpan<byte> bytes );

        /// <summary>
        /// Calculates the number of bytes produced by decoding the specified sequence of characters.
        /// </summary>
        /// <param name="chars">The sequence of characters to decode.</param>
        /// <returns>The number of bytes produced by decoding the sequence of characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="FormatException">The sequence of characters is invalid.</exception>
        public virtual int GetByteCount ( char[] chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetByteCount(chars.AsSpan());
        }

        /// <summary>
        /// Calculates the number of bytes produced by decoding the specified sequence of characters.
        /// </summary>
        /// <param name="chars">The array containing the characters to decode.</param>
        /// <param name="offset">The index of the first character to decode.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>The number of characters produced by decoding the sequence of characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="chars"/> is invalid.</exception>
        /// <exception cref="FormatException">The sequence of characters is invalid.</exception>
        public virtual int GetByteCount ( char[] chars, int offset, int count )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (chars.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetByteCount(chars.AsSpan(offset, count));
        }

        /// <summary>
        /// Calculates the number of bytes produced by decoding the specified string of characters.
        /// </summary>
        /// <param name="chars">The string of characters to decode.</param>
        /// <returns>The number of bytes produced by decoding the string of characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="FormatException">The sequence of characters is invalid.</exception>
        public virtual int GetByteCount ( string chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetByteCount(chars.AsSpan());
        }

        /// <summary>
        /// Calculates the number of bytes produced by decoding the specified sequence of characters.
        /// </summary>
        /// <param name="chars">The string containing the sequence of characters to decode.</param>
        /// <param name="offset">The index of the first character to decode.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>The number of bytes produced by decoding the specified sequence of characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="chars"/> is invalid.</exception>
        /// <exception cref="FormatException">The sequence of characters is invalid.</exception>
        public virtual int GetByteCount ( string chars, int offset, int count )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (chars.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetByteCount(chars.AsSpan(offset, count));
        }

        /// <summary>
        /// Calculates the number of bytes produced by decoding the specified sequence of characters.
        /// </summary>
        /// <param name="chars">The sequence of characters to decode.</param>
        /// <returns>The number of bytes produced by decoding the specified sequence of characters.</returns>
        /// <exception cref="FormatException">The sequence of characters is invalid.</exception>
        public abstract int GetByteCount ( ReadOnlySpan<char> chars );

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <returns>A character array containing the results of encoding the bytes.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="bytes"/> parameter is <c>null</c>.</exception>
        public virtual char[] GetChars ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetChars(bytes.AsSpan());
        }

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">A byte array containing the bytes to encode.</param>
        /// <param name="offset">The index of the first byte to encode.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <returns>A character array containing the results of encoding the bytes.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="bytes"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="bytes"/> is invalid.</exception>
        public virtual char[] GetChars ( byte[] bytes, int offset, int count )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (bytes.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetChars(bytes.AsSpan(offset, count));
        }

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <returns>A character array containing the results of encoding the sequence of bytes.</returns>
        public abstract char[] GetChars ( ReadOnlySpan<byte> bytes );

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <returns>A string containing the results of encoding the sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="bytes"/> parameter is <c>null</c>.</exception>
        public virtual string GetString ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetString(bytes.AsSpan());
        }

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <param name="offset">The index of the first byte to encode.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <returns>A string containing the results of encoding the sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="bytes"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="bytes"/> is invalid.</exception>
        public virtual string GetString ( byte[] bytes, int offset, int count )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (bytes.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetString(bytes.AsSpan(offset, count));
        }

        /// <summary>
        /// Encodes the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to encode.</param>
        /// <returns>A string containing the results of encoding the sequence of bytes.</returns>
        public abstract string GetString ( ReadOnlySpan<byte> bytes );

        /// <summary>
        /// Decodes the specified sequence of characters.
        /// </summary>
        /// <param name="chars">The sequence of characters to decode.</param>
        /// <returns>A byte array containing the results of decoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        public virtual byte[] GetBytes ( char[] chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetBytes(chars.AsSpan());
        }

        /// <summary>
        /// Decodes a set of characters from the specified character array into a sequence of bytes.
        /// </summary>
        /// <param name="chars">The character array containing the characters to decode.</param>
        /// <param name="offset">The index of the first character to decode.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>A byte array containing the results of decoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="chars"/> is invalid.</exception>
        public virtual byte[] GetBytes ( char[] chars, int offset, int count )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (chars.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetBytes(chars.AsSpan(offset, count));
        }

        /// <summary>
        /// Decodes all of the characters in the specified string into a sequence of bytes.
        /// </summary>
        /// <param name="chars">The string containing the characters to decode.</param>
        /// <returns>A byte array containing the results of decoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        public virtual byte[] GetBytes ( string chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetBytes(chars.AsSpan());
        }

        /// <summary>
        /// Decodes the specified sequence of characters into an array of bytes.
        /// </summary>
        /// <param name="chars">The characters to decode.</param>
        /// <param name="offset">The index of the first character to decode.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>A byte array containing the results of decoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="chars"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameter is less than zero.</exception>
        /// <exception cref="ArgumentException">The combination of <paramref name="offset"/>, <paramref name="count"/> and <paramref name="chars"/> is invalid.</exception>
        public virtual byte[] GetBytes ( string chars, int offset, int count )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if ((offset < 0) || (count < 0))
                throw new ArgumentOutOfRangeException((offset < 0) ? nameof(offset) : nameof(count));

            if (chars.Length - offset < count)
                throw new ArgumentException(Resources.InvalidOffsetCountLength);

            return GetBytes(chars.AsSpan(offset, count));
        }

        /// <summary>
        /// Decodes the specified sequence of characters into an array of bytes.
        /// </summary>
        /// <param name="chars">The characters to decode.</param>
        /// <returns>A byte array containing the results of decoding the specified characters.</returns>
        public abstract byte[] GetBytes ( ReadOnlySpan<char> chars );

        /// <summary>
        /// Calculates the maximum number of bytes produced by decoding the specified number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to decode.</param>
        /// <returns>The maximum number of bytes produced by decoding the specified number of characters.</returns>
        public abstract int GetMaxByteCount ( int charCount );

        /// <summary>
        /// Calculates the maximum number of characters produced by encoding the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to encode.</param>
        /// <returns>The maximum number of characters produced by encoding the specified number of bytes.</returns>
        public abstract int GetMaxCharCount ( int byteCount );

        /// <summary>
        /// Obtains an encoder that converts a sequence of bytes into an encoded sequence of characters.
        /// </summary>
        /// <returns>A <see cref="BinaryEncoder"/> that converts a sequence of bytes in to an encoded sequence of characters.</returns>
        public abstract BinaryEncoder GetEncoder ();

        /// <summary>
        /// Obtains a decoder that converts a sequence of encoded characters into a sequence of bytes.
        /// </summary>
        /// <returns>A <see cref="BinaryDecoder"/> that converts a sequence of characters in to a decoded sequence of byte.</returns>
        public abstract BinaryDecoder GetDecoder ();

        /// <summary>
        /// Gets the minimum amount of input data required to ensure that the codec will always make
        /// forward progress.
        /// </summary>
        public abstract int MinimumInputBuffer { get; }

        /// <summary>
        /// Gets the minimum amout of output buffer required to ensure that the codec will always
        /// make forward progress.
        /// </summary>
        public abstract int MinimumOutputBuffer { get; }
    }
}
