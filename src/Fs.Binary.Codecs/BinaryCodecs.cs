﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs.Base16;
using Fs.Binary.Codecs.Base32;
using Fs.Binary.Codecs.Base64;
using Fs.Binary.Codecs.Base85;
using Fs.Binary.Codecs.QuotedPrintable;

namespace Fs.Binary.Codecs
{
    /// <summary>
    /// A container of <see cref="BinaryCodec"/> instances that are preconfigured to
    /// common settings.
    /// </summary>
    public static class BinaryCodecs
    {
        /// <summary>
        /// A Base16 codec using RFC-4648 rules.
        /// </summary>
        public static BinaryCodec Base16Standard => _base16Standard.Value;
        private static readonly Lazy<BinaryCodec> _base16Standard =
            new Lazy<BinaryCodec>(() => new Base16Codec(Base16Settings.Default));

        /// <summary>
        /// A Base32 codec using RFC-4648 rules.
        /// </summary>
        public static BinaryCodec Base32Standard => _base32Standard.Value;
        private static readonly Lazy<BinaryCodec> _base32Standard =
            new Lazy<BinaryCodec>(() => new Base32Codec(Base32Settings.Default));

        /// <summary>
        /// A Base64 codec using RFC-4648 rules.
        /// </summary>
        public static BinaryCodec Base64Standard => _base64Standard.Value;
        private static readonly Lazy<BinaryCodec> _base64Standard =
            new Lazy<BinaryCodec>(() => new Base64Codec(Base64Settings.Default));

        /// <summary>
        /// A Base64 codec using RFC-2045 rules.
        /// </summary>
        public static BinaryCodec Base64Mime => _base64Mime.Value;
        private static readonly Lazy<BinaryCodec> _base64Mime =
            new Lazy<BinaryCodec>(() => new Base64Codec(new Base64Settings(Base64Settings.Default)
            {
                DecodingIgnorableCharacters = "\r\n \t\v",
                DecodingIgnoreInvalidCharacters = true,
                DecodingIgnoreInvalidPadding = true,
                EncodingLineSeparator = "\r\n",
                EncodingMaximumLineLength = 76,
                IsProtected = true
            }));

        /// <summary>
        /// A Base64 codec using RFC-4648 rules for URL safety.
        /// </summary>
        public static BinaryCodec Base64UrlSafe => _base64UrlSafe.Value;
        private static readonly Lazy<BinaryCodec> _base64UrlSafe =
            new Lazy<BinaryCodec>(() => new Base64Codec(new Base64Settings(Base64Settings.Default)
            {
                Alphabets = new string[] { "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_" },
                DecodingIgnorableCharacters = "\r\n \t\v",
                DecodingIgnoreInvalidCharacters = true,
                DecodingIgnoreInvalidPadding = true,
                EncodingNoPadding = true,
                IsProtected = true
            }));

        /// <summary>
        /// A Base85 codec.
        /// </summary>
        public static BinaryCodec Base85Standard => _base85Standard.Value;
        private static readonly Lazy<BinaryCodec> _base85Standard =
            new Lazy<BinaryCodec>(() => new Base85Codec(Base85Settings.Default));

        /// <summary>
        /// A ASCII85 (base85) codec.
        /// </summary>
        public static BinaryCodec Ascii85 => _ascii85.Value;
        private static readonly Lazy<BinaryCodec> _ascii85 =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings(Base85Settings.Default)
            {
                DecodingIgnorableCharacters = "\r\n \t\v",
                DecodingIgnoreInvalidCharacters = true,
                DecodingPrefixes = new string[] { "<~" },
                DecodingPostfixes = new string[] { "~>" },
                EncodingPrefix = "<~",
                EncodingPostfix = "~>",
                EncodingTruncateFinalQuantum = true,
                ZeroQuantumCharacter = 'z'
            }));

        /// <summary>
        /// BtoA (base85) codec.
        /// </summary>
        public static BinaryCodec BtoA => _btoa.Value;
        private static readonly Lazy<BinaryCodec> _btoa =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings(Base85Settings.Default)
            {
                DecodingIgnorableCharacters = "\r\n \t\v",
                EncodingTruncateFinalQuantum = false,
                DecodingRequireCompleteFinalQuantum = true,
                ZeroQuantumCharacter = 'z',
                SpacesQuantumCharacter = 'y'
            }));

        /// <summary>
        /// Z85 (base85) codec.
        /// </summary>
        public static BinaryCodec Z85 => _z85.Value;
        private static readonly Lazy<BinaryCodec> _z85 =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings(Base85Settings.Default)
            {
                Alphabets = new string[] { "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#" },
                DecodingIgnorableCharacters = "\r\n \t\v",
                EncodingTruncateFinalQuantum = false,
                DecodingRequireCompleteFinalQuantum = true
            }));

        /// <summary>
        /// Quoted Printable using RFC-2048 rules.
        /// </summary>
        public static BinaryCodec QuotedPrintable => _quotedPrintable.Value;
        private static readonly Lazy<BinaryCodec> _quotedPrintable =
            new Lazy<BinaryCodec>(() => new QuotedPrintableCodec(QuotedPrintableSettings.Default));
    }
}
