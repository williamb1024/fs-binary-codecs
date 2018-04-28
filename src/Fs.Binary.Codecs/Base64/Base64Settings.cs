using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base64
{
    public class Base64Settings : ExposedAlphabetBuilderWithPadding
    {
        private static readonly string[] DefaultAlphabet = new string[]
        {
           "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        };

        public static readonly Base64Settings Default = new Base64Settings()
        {
            PaddingCharacter = '=',
            IsReadOnly = true
        };

        public Base64Settings ()
            : base(64, DefaultAlphabet)
        {
        }

        public Base64Settings ( Base64Settings inheritedSettings, bool isProtected )
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

        public Base64Settings ToReadOnly () => new Base64Settings(this, true);
    }
}
