using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base32
{
    public class Base32Settings : ExposedAlphabetBuilderWithPadding
    {
        private static readonly string[] DefaultAlphabet = new string[]
        {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567",
            "abcdefghijklmnopqrstuvwxyz234567"
        };

        public static readonly Base32Settings Default = new Base32Settings()
        {
            PaddingCharacter = '=',
            IsReadOnly = true
        };

        public Base32Settings ()
            : base(32, DefaultAlphabet)
        {
        }

        public Base32Settings ( Base32Settings inheritedSettings, bool isProtected )
            : base(inheritedSettings, isProtected)
        {
        }

        internal void GetEncoderSettings ( out string alphabet,
                                           out int flags, 
                                           out string lineSeparator,
                                           out int maximumLineLength,
                                           out string initialPrefix,
                                           out string finalPrefix,
                                           out char paddingChar)
        {
            alphabet = GetAlphabet();
            flags = GetFlags();
            lineSeparator = GetLineSeparator();
            maximumLineLength = GetMaximumLineLength();
            initialPrefix = GetEncodingPrefix();
            finalPrefix = GetEncodingPostfix();
            paddingChar = GetPaddingCharacter();
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

        public Base32Settings ToReadOnly () => new Base32Settings(this, true);
    }
}
