using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    public static class SettingsCharacterTypes
    {
        // alphabet types and flags
        public const byte CharTypeMask = 0x80; // mask used to determine character type

        public const byte CharAlphabet = 0x00; // character is an alphabet character
        public const byte CharSpecial = 0x80; // character is not an alphabet character

        public const byte CharSpecialInvalid = 0x80; // character is invalid
        public const byte CharSpecialIgnored = 0x81; // character is silently ignored
        public const byte CharSpecialPadding = 0x82; // character is a padding character
        public const byte CharSpecialZ = 0x83;
        public const byte CharSpecialY = 0x84;

        public const byte CharSpecialDescendantReserved = 0x8C;

        // affix types and flags
        public const byte AffixNone = 0x00;
        public const byte AffixStartPrefix = 0x01;
        public const byte AffixStartPostfix = 0x02;

        // SafeCharacters types and flags
        public const byte CharSafeInvalid = 0;
        public const byte CharSafe = 1;
        public const byte CharSafeExcluded = 0x10;
        public const byte CharSafeFlagEscape = 0x20;
        public const byte CharSafeFlagNewLine = 0x40;
        public const byte CharSafeFlagWhitespace = 0x80;
        public const byte CharSafeFlagsMask = 0xF0;


    }
}
