using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsEncodingLinesProvider : SettingsBase.SettingsProvider, ISettingsEncodingLines
    {
        private string _lineSeparator = String.Empty;
        private int _maximumLineLength = 76;
        private int _minimumLineLength = 1;

        public SettingsEncodingLinesProvider ( SettingsBase settingsBase )
            : this(settingsBase, 1)
        {
        }

        public SettingsEncodingLinesProvider ( SettingsBase settingsBase, int minimumLineLength )
            : base(settingsBase)
        {
            if (minimumLineLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLineLength));

            _minimumLineLength = minimumLineLength;
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsEncodingLinesProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsEncodingLinesProvider)inheritedSettings;
            _lineSeparator = inheritSettings._lineSeparator;
            _maximumLineLength = inheritSettings._maximumLineLength;
        }

        internal string GetEncodingLineSeparator ()
        {
            return _lineSeparator;
        }

        internal int GetEncodingMaximumLineLength ()
        {
            return _maximumLineLength;
        }

        public string EncodingLineSeparator
        {
            get => _lineSeparator;
            set
            {
                CheckWritable();
                _lineSeparator = value ?? String.Empty;
            }
        }

        public int EncodingMaximumLineLength
        {
            get => _maximumLineLength;
            set
            {
                if (value < _minimumLineLength)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckWritable();
                _maximumLineLength = value;
            }
        }
        
        public bool EncodingRequireTerminalLineSeparator
        {
            get => GetFlag(SettingsFlags.ForceTerminatingLineSeparator);
            set => SetFlag(SettingsFlags.ForceTerminatingLineSeparator, value);
        }
    }
}
