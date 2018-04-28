using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base16
{
    public class Base16Settings : ExposedAlphabetBuilder
    {
        private static readonly string[] DefaultAlphabet = new string[]
        {
            "0123456789ABCDEF",
            "0123456789abcdef",
        };

        public static readonly Base16Settings Default = new Base16Settings()
        {
            IsReadOnly = true
        };

        public Base16Settings ()
                : base(16, DefaultAlphabet)
        {
        }

        public Base16Settings ( Base16Settings inheritedSettings, bool isProtected )
            : base(inheritedSettings, isProtected)
        {
        }

        internal void GetEncoderSettings ( out string alphabet,
                                           out int flags,
                                           out string lineSeparator,
                                           out int maximumLineLength,
                                           out string initialPrefix,
                                           out string finalPrefix )
        {
            alphabet = GetAlphabet();
            flags = GetFlags();
            lineSeparator = GetLineSeparator();
            maximumLineLength = GetMaximumLineLength();
            initialPrefix = GetEncodingPrefix();
            finalPrefix = GetEncodingPostfix();
        }

        internal void GetDecoderSettings ( out ImmutableArray<byte> decodingTable,
                                           out ImmutableArray<byte> decodingAffixTable,
                                           out ImmutableArray<string> decodingPrefixes,
                                           out ImmutableArray<string> decodingPostfixes,
                                           out int flags )
        {
            decodingTable = GetDecodingTable();
            decodingAffixTable = GetDecodingAffixTable();
            decodingPrefixes = GetDecodingPrefixes();
            decodingPostfixes = GetDecodingPostfixes();
            flags = GetFlags();
        }

        public Base16Settings ToReadOnly () => new Base16Settings(this, true);
    }
}
