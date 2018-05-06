using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsDecodingCheckFinalQuantumProvider: SettingsBase.SettingsProvider, ISettingsDecodingCheckFinalQuantum
    {
        public SettingsDecodingCheckFinalQuantumProvider ( SettingsBase settingsBase ) 
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsDecodingCheckFinalQuantumProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsDecodingCheckFinalQuantumProvider)inheritedSettings;
        }

        public bool DecodingIgnoreInvalidFinalQuantum
        {
            get => GetFlag(SettingsFlags.FlagIgnoreInvalidFinalQuantum);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagIgnoreInvalidFinalQuantum, value);
            }
        }
    }
}
