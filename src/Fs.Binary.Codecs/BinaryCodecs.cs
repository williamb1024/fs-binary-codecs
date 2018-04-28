﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs.Base16;
using Fs.Binary.Codecs.Base32;
using Fs.Binary.Codecs.Base64;
using Fs.Binary.Codecs.Base85;

namespace Fs.Binary.Codecs
{
    public static class BinaryCodecs
    {
        public static BinaryCodec Base16Standard => _base16Standard.Value;
        private static readonly Lazy<BinaryCodec> _base16Standard =
            new Lazy<BinaryCodec>(() => new Base16Codec(Base16Settings.Default));

        public static BinaryCodec Base32Standard => _base32Standard.Value;
        private static readonly Lazy<BinaryCodec> _base32Standard =
            new Lazy<BinaryCodec>(() => new Base32Codec(Base32Settings.Default));

        public static BinaryCodec Base64Standard => _base64Standard.Value;
        private static readonly Lazy<BinaryCodec> _base64Standard =
            new Lazy<BinaryCodec>(() => new Base64Codec(Base64Settings.Default));

        public static BinaryCodec Base64Mime => _base64Mime.Value;
        private static readonly Lazy<BinaryCodec> _base64Mime =
            new Lazy<BinaryCodec>(() => new Base64Codec(new Base64Settings
            {
                PaddingCharacter = '=',
                DecodingIgnoredCharacters = "\r\n \t\v",
                DecodingIgnoreInvalidCharacters = true,
                DecodingIgnoreInvalidPadding = true,
                EncodingLineSeparator = "\r\n",
                EncodingMaximumLineLength = 76,
                IsReadOnly = true
            }));

        public static BinaryCodec Base85Standard => _base85Standard.Value;
        private static readonly Lazy<BinaryCodec> _base85Standard =
            new Lazy<BinaryCodec>(() => new Base85Codec(Base85Settings.Default));

        public static BinaryCodec Ascii85 => _ascii85.Value;
        private static readonly Lazy<BinaryCodec> _ascii85 =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings
            {
                DecodingIgnoredCharacters = "\r\n \t\v",
                DecodingIgnoreInvalidCharacters = true,
                DecodingPrefixes = new string[] { "<~" },
                DecodingPostfixes = new string[] { "~>" },
                EncodingPrefix = "<~",
                EncodingPostfix = "~>",
                EncodingCompleteFinalQuantum = false,
                FourZerosAbbreviation = 'z'
            }));

        public static BinaryCodec BtoA => _btoa.Value;
        private static readonly Lazy<BinaryCodec> _btoa =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings
            {
                DecodingIgnoredCharacters = "\r\n \t\v",
                EncodingCompleteFinalQuantum = true,
                FourZerosAbbreviation = 'z',
                FourSpacesAbbreviation = 'y'
            }));

        public static BinaryCodec Z85 => _z85.Value;
        private static readonly Lazy<BinaryCodec> _z85 =
            new Lazy<BinaryCodec>(() => new Base85Codec(new Base85Settings
            {
                AlphabetCharacters = new string[] { "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#" },
                DecodingIgnoredCharacters = "\r\n \t\v",
                EncodingCompleteFinalQuantum = true,
                DecodingRequireCompleteFinalQuantum = true
            }));

    }
}