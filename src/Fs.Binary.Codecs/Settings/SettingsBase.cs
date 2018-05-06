using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs.Settings.Providers;

namespace Fs.Binary.Codecs.Settings
{
    public abstract class SettingsBase
    {
        private SettingsProvider _settingsProviderHead;

        protected SettingsBase ()
        {
        }

        protected void InheritSettings ( SettingsBase settingsBase )
        {
            if (settingsBase == null)
                throw new ArgumentNullException(nameof(settingsBase));

            if (GetType() != settingsBase.GetType())
                throw new ArgumentException("Cannot inherit from a different type.");

            SettingsProvider localProvider = _settingsProviderHead;
            SettingsProvider remoteProvider = settingsBase._settingsProviderHead;

            while (localProvider != null)
            {
                localProvider.InheritSettings(remoteProvider);

                localProvider = localProvider.NextProvider;
                remoteProvider = remoteProvider.NextProvider;
            }
        }

        protected IEnumerable<SettingsProvider> GetProviders ()
        {
            SettingsProvider currentProvider = _settingsProviderHead;
            while (currentProvider != null)
            {
                yield return currentProvider;
                currentProvider = currentProvider.NextProvider;
            }
        }

        protected void CheckWritable ()
        {
            if (IsProtected)
                throw new InvalidOperationException(Resources.InstanceIsReadOnly);
        }

        private SettingsProvider AddProvider ( SettingsProvider provider )
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            SettingsProvider nextProvider = _settingsProviderHead;
            _settingsProviderHead = provider;
            return nextProvider;
        }

        public bool IsProtected { get; internal set; }

        public abstract class SettingsProvider
        {
            private readonly SettingsBase _settingsBase;
            private readonly SettingsProvider _nextProvider;

            protected SettingsProvider ( SettingsBase settingsBase )
            {
                if (settingsBase == null)
                    throw new ArgumentNullException(nameof(settingsBase));

                _settingsBase = settingsBase;
                _nextProvider = _settingsBase.AddProvider(this);
            }

            public abstract void InheritSettings ( SettingsProvider inheritedSettings );

            protected void CheckWritable ()
            {
                _settingsBase.CheckWritable();
            }

            protected SettingsProvider GetSettingsProvider ( Type providerType )
            {
                var provider = Settings.GetProviders()
                    .FirstOrDefault(p => p.GetType() == providerType);

                return provider ?? throw new NotSupportedException();
            }

            protected bool GetFlag ( int settingsFlag )
            {
                return ((SettingsFlagsProvider)GetSettingsProvider(typeof(SettingsFlagsProvider))).GetFlag(settingsFlag);
            }

            protected void SetFlag ( int settingsFlag, bool setFlag )
            {
                ((SettingsFlagsProvider)GetSettingsProvider(typeof(SettingsFlagsProvider))).SetFlag(settingsFlag, setFlag);
            }

            internal SettingsProvider NextProvider { get { return _nextProvider; } }

            protected SettingsBase Settings => _settingsBase;
        }
    }
}
