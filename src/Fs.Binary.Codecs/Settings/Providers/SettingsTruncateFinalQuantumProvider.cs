using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsTruncateFinalQuantumProvider : SettingsBase.SettingsProvider, ISettingsTruncateFinalQuantum
    {
        public SettingsTruncateFinalQuantumProvider ( SettingsBase settingsBase )
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsTruncateFinalQuantumProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsTruncateFinalQuantumProvider)inheritedSettings;
        }

        public bool EncodingTruncateFinalQuantum
        {
            get => !GetFlag(SettingsFlags.FlagForceFullQuantums);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagForceFullQuantums, !value);
            }
        }

        public bool DecodingRequireCompleteFinalQuantum
        {
            get => GetFlag(SettingsFlags.FlagRequireCompleteFinalQuantum);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagRequireCompleteFinalQuantum, value);
            }
        }
    }
}
