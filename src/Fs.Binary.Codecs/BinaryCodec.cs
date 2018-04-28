using System;
using System.Collections.Generic;
using System.Text;
using Fs.Binary.Codecs.Base16;
using Fs.Binary.Codecs.Base32;
using Fs.Binary.Codecs.Base64;
using Fs.Binary.Codecs.Base85;

namespace Fs.Binary.Codecs
{
    public abstract class BinaryCodec
    {
        public virtual int GetCharCount ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetCharCount(bytes.AsSpan());
        }

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

        public abstract int GetCharCount ( ReadOnlySpan<byte> bytes );

        public virtual int GetByteCount ( char[] chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetByteCount(chars.AsSpan());
        }

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

        public virtual int GetByteCount ( string chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetByteCount(chars.AsSpan());
        }

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

        public abstract int GetByteCount ( ReadOnlySpan<char> chars );

        public virtual char[] GetChars ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetChars(bytes.AsSpan());
        }

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

        public abstract char[] GetChars ( ReadOnlySpan<byte> bytes );

        public virtual string GetString ( byte[] bytes )
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return GetString(bytes.AsSpan());
        }

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

        public abstract string GetString ( ReadOnlySpan<byte> bytes );

        public virtual byte[] GetBytes ( char[] chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetBytes(chars.AsSpan());
        }

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

        public virtual byte[] GetBytes ( string chars )
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            return GetBytes(chars.AsSpan());
        }

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

        public abstract byte[] GetBytes ( ReadOnlySpan<char> chars );

        public abstract int GetMaxByteCount ( int charCount );
        public abstract int GetMaxCharCount ( int byteCount );

        public abstract BinaryEncoder GetEncoder ();
        public abstract BinaryDecoder GetDecoder ();

        public abstract int MinimumInputBuffer { get; }
        public abstract int MinimumOutputBuffer { get; }
    }
}
