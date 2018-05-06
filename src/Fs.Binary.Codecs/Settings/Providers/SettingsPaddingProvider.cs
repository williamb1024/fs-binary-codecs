using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsPaddingProvider : SettingsBase.SettingsProvider, ISettingsPadding
    {
        private char? _paddingChar;

        public SettingsPaddingProvider ( SettingsBase settingsBase )
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsPaddingProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsPaddingProvider)inheritedSettings;
            _paddingChar = inheritSettings._paddingChar;
        }

        internal char? GetPaddingCharacter () => _paddingChar;

        public char? PaddingCharacter
        {
            get => _paddingChar;
            set
            {
                CheckWritable();
                ((SettingsAlphabetProvider)GetSettingsProvider(typeof(SettingsAlphabetProvider)))
                    .ChangeSpecialCharacter(ref _paddingChar, SettingsFlags.FlagHasPaddingCharacter, value, SettingsCharacterTypes.CharSpecialPadding);
            }
        }

        public bool EncodingNoPadding
        {
            get => GetFlag(SettingsFlags.FlagDisablePadding);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagDisablePadding, value);
            }
        }

        public bool DecodingIgnoreInvalidPadding
        {
            get => GetFlag(SettingsFlags.FlagIgnoreInvalidPadding);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagIgnoreInvalidPadding, value);
            }
        }

        public bool DecodingRequirePadding
        {
            get => GetFlag(SettingsFlags.FlagRequirePadding);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagRequirePadding, value);
            }
        }
    }
}
