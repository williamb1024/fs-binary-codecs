using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsDecodingCheckUnusedBitsProvider: SettingsBase.SettingsProvider, ISettingsDecodingCheckUnusedBits
    {
        public SettingsDecodingCheckUnusedBitsProvider ( SettingsBase settingsBase ) 
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsDecodingCheckUnusedBitsProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsDecodingCheckUnusedBitsProvider)inheritedSettings;
        }

        public bool DecodingRequireUnusedBitsBeZeros
        {
            get => GetFlag(SettingsFlags.FlagRequireTrailingZeroBits);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagRequireTrailingZeroBits, value);
            }
        }
    }
}
