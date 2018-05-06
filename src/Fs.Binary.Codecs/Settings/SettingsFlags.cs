using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    /// <summary>
    /// All of the various flags used by the Settings classes.
    /// </summary>
    /// <remarks>
    /// The flags share the same range of bits, so it's easier to keep them all in one place
    /// rather than having to ensure there is no overlap across multiple classes.
    /// </remarks>
    public static class SettingsFlags
    {
        public const int ForceTerminatingLineSeparator = 0x00000001;
        public const int FlagHasPaddingCharacter = 0x000000002;
        public const int FlagIgnoreInvalidCharacters = 0x00000004;
        public const int FlagIgnoreInvalidPadding = 0x00000008;
        public const int FlagRequirePadding = 0x00000010;
        public const int FlagRequireTrailingZeroBits = 0x00000020;
        public const int FlagIgnoreInvalidFinalQuantum = 0x00000040;
        public const int FlagHasPrefixes = 0x00000080;
        public const int FlagHasPostfixes = 0x00000100;
        public const int FlagRequirePrefix = 0x00000200;
        public const int FlagRequirePostfix = 0x00000400;
        public const int FlagDisablePadding = 0x00000800;
        public const int FlagForceFullQuantums = 0x00001000;
        public const int FlagRequireCompleteFinalQuantum = 0x00002000;
        public const int FlagIgnoreOverflow = 0x00004000;

        // any flags matching this mask are reserved from descendants..
        public const int FlagsDescendantReserved = 0xFF00000;
        public const int FlagsDescendantReservedBase = 0x01000000;

        // Base85 unique flags
        public const int FlagBase85HasZCharacter = FlagsDescendantReservedBase;
        public const int FlagBase85HasYCharacter = FlagsDescendantReservedBase << 1;

        // QP unique flags
        public const int FlagQpAcceptLFOnlyHardBreaks = FlagsDescendantReservedBase;
        public const int FlagQpAcceptCROnlyHardBreaks = FlagsDescendantReservedBase << 1;


    }
}
