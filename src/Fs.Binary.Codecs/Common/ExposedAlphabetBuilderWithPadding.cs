using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs.Common
{
    public class ExposedAlphabetBuilderWithPadding: ExposedAlphabetBuilder
    {
        protected ExposedAlphabetBuilderWithPadding ( int alphabetLength, string[] alphabet )
            : base(alphabetLength, alphabet)
        {
        }

        protected ExposedAlphabetBuilderWithPadding ( ExposedAlphabetBuilder inheritedBuilder, bool isProtected )
            : base(inheritedBuilder, isProtected)
        {
        }

        public new char? PaddingCharacter
        {
            get => base.PaddingCharacter;
            set => base.PaddingCharacter = value;
        }

        public new bool DecodingIgnoreInvalidPadding
        {
            get => base.DecodingIgnoreInvalidPadding;
            set => base.DecodingIgnoreInvalidPadding = value;
        }

        public new bool DecodingRequirePadding
        {
            get => base.DecodingRequirePadding;
            set => base.DecodingRequirePadding = value;
        }
    }
}
