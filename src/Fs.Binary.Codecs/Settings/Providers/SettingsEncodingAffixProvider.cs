using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsEncodingAffixProvider: SettingsBase.SettingsProvider, ISettingsEncodingAffixes
    {
        private string _encodingPrefix = String.Empty;
        private string _encodingPostfix = String.Empty;

        public SettingsEncodingAffixProvider ( SettingsBase settingsBase )
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsEncodingAffixProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsEncodingAffixProvider)inheritedSettings;
            _encodingPrefix = inheritSettings._encodingPrefix;
            _encodingPostfix = inheritSettings._encodingPostfix;
        }

        public string GetEncodingPrefix ()
        {
            return _encodingPrefix;
        }

        public string GetEncodingPostfix ()
        {
            return _encodingPostfix;
        }

        public string EncodingPrefix
        {
            get => _encodingPrefix;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                _encodingPrefix = value;
            }
        }

        public string EncodingPostfix
        {
            get => _encodingPostfix;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                _encodingPostfix = value;
            }
        }

        public int EncodingAffixLength
        {
            get => _encodingPrefix.Length + _encodingPostfix.Length;
        }
    }
}
