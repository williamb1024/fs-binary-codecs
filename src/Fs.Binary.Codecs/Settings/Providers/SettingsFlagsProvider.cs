using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsFlagsProvider: SettingsBase.SettingsProvider
    {
        private int _settingFlags;

        public SettingsFlagsProvider ( SettingsBase settingsBase)
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsFlagsProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsFlagsProvider)inheritedSettings;
            _settingFlags = inheritSettings._settingFlags;
        }

        internal int GetFlags ()
        {
            return _settingFlags;
        }

        public new bool GetFlag ( int testFlags )
        {
            return (_settingFlags & testFlags) == testFlags;
        }

        public new void SetFlag ( int changeFlags, bool enableFlags )
        {
            if (changeFlags == 0)
                return;

            _settingFlags = (_settingFlags & ~changeFlags) | ((enableFlags) ? changeFlags : 0);
        }

        public void SetFlags ( int changeFlags, int flagValues )
        {
            if (changeFlags == 0)
                return;

            _settingFlags = (_settingFlags & ~changeFlags) | (flagValues & changeFlags);
        }
    }
}
