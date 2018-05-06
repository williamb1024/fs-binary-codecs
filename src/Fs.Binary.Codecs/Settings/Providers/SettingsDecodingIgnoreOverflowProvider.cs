using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsDecodingIgnoreOverflowProvider: SettingsBase.SettingsProvider, ISettingsDecodingIgnoreOverflow
    {
        public SettingsDecodingIgnoreOverflowProvider ( SettingsBase settingsBase)
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsDecodingIgnoreOverflowProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsDecodingIgnoreOverflowProvider)inheritedSettings;
        }

        public bool DecodingIgnoreOverflow
        {
            get => GetFlag(SettingsFlags.FlagIgnoreOverflow);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagIgnoreOverflow, value);
            }
        }
    }
}
