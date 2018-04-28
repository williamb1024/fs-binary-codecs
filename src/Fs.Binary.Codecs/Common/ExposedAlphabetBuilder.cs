using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Common
{
    public class ExposedAlphabetBuilder : AlphabetBuilder
    {
        protected ExposedAlphabetBuilder ( int alphabetLength, string[] alphabet )
            : base(alphabetLength, alphabet)
        {
        }

        protected ExposedAlphabetBuilder ( ExposedAlphabetBuilder inheritedBuilder, bool isProtected ) 
            : base(inheritedBuilder, isProtected)
        {
        }

        public new string[] AlphabetCharacters
        {
            get => base.AlphabetCharacters;
            set => base.AlphabetCharacters = value;
        }

        public new string[] DecodingPrefixes
        {
            get => base.DecodingPrefixes;
            set => base.DecodingPrefixes = value;
        }

        public new string[] DecodingPostfixes
        {
            get => base.DecodingPostfixes;
            set => base.DecodingPostfixes = value;
        }

        public new string DecodingIgnoredCharacters
        {
            get => base.DecodingIgnoredCharacters;
            set => base.DecodingIgnoredCharacters = value;
        }

        public new string EncodingLineSeparator
        {
            get => base.EncodingLineSeparator;
            set => base.EncodingLineSeparator = value;
        }

        public new int EncodingMaximumLineLength
        {
            get => base.EncodingMaximumLineLength;
            set => base.EncodingMaximumLineLength = value;
        }

        public new bool EncodingIncludeTerminatingLineSeparator
        {
            get => base.EncodingIncludeTerminatingLineSeparator;
            set => base.EncodingIncludeTerminatingLineSeparator = value;
        }

        public new string EncodingPrefix
        {
            get => base.EncodingPrefix;
            set => base.EncodingPrefix = value;
        }

        public new string EncodingPostfix
        {
            get => base.EncodingPostfix;
            set => base.EncodingPostfix = value;
        }

        public new bool DecodingIgnoreInvalidCharacters
        {
            get => base.DecodingIgnoreInvalidCharacters;
            set => base.DecodingIgnoreInvalidCharacters = value;
        }

        public new bool DecodingRequireTrailingZeroBits
        {
            get => base.DecodingRequireTrailingZeroBits;
            set => base.DecodingRequireTrailingZeroBits = value;
        }

        public new bool DecodingIgnoreInvalidFinalQuantum
        {
            get => base.DecodingIgnoreInvalidFinalQuantum;
            set => base.DecodingIgnoreInvalidFinalQuantum = value;
        }

        public new bool DecodingPrefixRequired
        {
            get => base.DecodingPrefixRequired;
            set => base.DecodingPrefixRequired = value;
        }

        public new bool DecodingPostfixRequired
        {
            get => base.DecodingPostfixRequired;
            set => base.DecodingPostfixRequired = value;
        }
    }
}
