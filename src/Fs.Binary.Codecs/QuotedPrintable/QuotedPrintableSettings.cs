using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs.Settings;
using Fs.Binary.Codecs.Settings.Providers;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public class QuotedPrintableSettings : SettingsBase
    {
        private static readonly string DefaultAlphabet = "\t !\"#$%&'()*+,-./0123456789:;<>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public static readonly QuotedPrintableSettings Default = new QuotedPrintableSettings()
        {
            SafeCharacters = DefaultAlphabet,
            EncodingLineSeparator = "\r\n",
            ExcludedCharacters = "=\r\n",
            EscapeCharacters = "=",
            NewLineCharacters = "\r\n",
            WhitespaceCharacters = " \t",
            EncodingMaximumLineLength = 76,
            EncodingRequireTerminalLineSeparator = true,
            IsProtected = true
        };

        private readonly SettingsSafeCharactersProvider _settingsSafeCharactersProvider;
        private readonly SettingsEncodingLinesProvider _settingsEncodingLinesProvider;
        private readonly SettingsFlagsProvider _settingsFlagsProvider;

        private string _decodingHardBreak = Environment.NewLine;

        private QuotedPrintableSettings ()
            : base()
        {
            _settingsSafeCharactersProvider = new SettingsSafeCharactersProvider(this);
            _settingsEncodingLinesProvider = new SettingsEncodingLinesProvider(this, 4);
            _settingsFlagsProvider = new SettingsFlagsProvider(this);
        }

        public QuotedPrintableSettings ( QuotedPrintableSettings inheritedSettings )
            : this()
        {
            if (inheritedSettings == null)
                throw new ArgumentNullException(nameof(inheritedSettings));

            InheritSettings(inheritedSettings);

            _decodingHardBreak = inheritedSettings._decodingHardBreak;
        }

        private QuotedPrintableSettings ( QuotedPrintableSettings inheritedSettings, bool isProtected )
            : this(inheritedSettings)
        {
            IsProtected = isProtected;
        }

        internal void GetEncoderSettings ( out ImmutableArray<byte> safeCharacters,
                                           out int flags,
                                           out string lineSeparator,
                                           out int maximumLineLength,
                                           out string hexCharacters )
        {
            safeCharacters = _settingsSafeCharactersProvider.GetSafeCharacters();
            flags = _settingsFlagsProvider.GetFlags();
            lineSeparator = _settingsEncodingLinesProvider.GetEncodingLineSeparator();
            maximumLineLength = _settingsEncodingLinesProvider.GetEncodingMaximumLineLength();
            hexCharacters = Base16.Base16Settings.Default.GetHexEncoding();
        }

        internal void GetDecoderSettings ( out ImmutableArray<byte> safeCharacters,
                                           out ImmutableArray<byte> hexDecodingTable,
                                           out string hardBreakChars,
                                           out int flags )
        {
            safeCharacters = _settingsSafeCharactersProvider.GetSafeCharacters();
            hexDecodingTable = Base16.Base16Settings.Default.GetHexDecoding();
            hardBreakChars = _decodingHardBreak;
            flags = _settingsFlagsProvider.GetFlags();
        }

        public QuotedPrintableSettings ToReadOnly () => (IsProtected) ? this : new QuotedPrintableSettings(this, true);

        internal string ExcludedCharacters { get => _settingsSafeCharactersProvider.ExcludedCharacters; set => _settingsSafeCharactersProvider.ExcludedCharacters = value; }
        internal string WhitespaceCharacters { get => _settingsSafeCharactersProvider.WhitespaceCharacters; set => _settingsSafeCharactersProvider.WhitespaceCharacters = value; }
        internal string EscapeCharacters { get => _settingsSafeCharactersProvider.EscapeCharacters; set => _settingsSafeCharactersProvider.EscapeCharacters = value; }
        internal string NewLineCharacters { get => _settingsSafeCharactersProvider.NewLineCharacters; set => _settingsSafeCharactersProvider.NewLineCharacters = value; }

        public bool DecodingAcceptLFOnlyHardBreaks
        {
            get => _settingsFlagsProvider.GetFlag(SettingsFlags.FlagQpAcceptLFOnlyHardBreaks);
            set
            {
                CheckWritable();
                _settingsFlagsProvider.SetFlag(SettingsFlags.FlagQpAcceptLFOnlyHardBreaks, value);
            }
        }

        public bool DecodingAcceptCROnlyHardBreaks
        {
            get => _settingsFlagsProvider.GetFlag(SettingsFlags.FlagQpAcceptCROnlyHardBreaks);
            set
            {
                CheckWritable();
                _settingsFlagsProvider.SetFlag(SettingsFlags.FlagQpAcceptCROnlyHardBreaks, value);
            }
        }

        // TODO: does this really make any sense for a "binary" codec?
        //public string DecodingHardBreak
        //{
        //    get => _decodingHardBreak;
        //    set
        //    {
        //        if (value == null)
        //            throw new ArgumentNullException(nameof(value));

        //        CheckWritable();
        //        _decodingHardBreak = value;
        //    }
        //}

        public string SafeCharacters { get => _settingsSafeCharactersProvider.SafeCharacters; set => _settingsSafeCharactersProvider.SafeCharacters = value; }
        public bool DecodingIgnoreInvalidCharacters { get => _settingsSafeCharactersProvider.DecodingIgnoreInvalidCharacters; set => _settingsSafeCharactersProvider.DecodingIgnoreInvalidCharacters = value; }
        public string EncodingLineSeparator { get => _settingsEncodingLinesProvider.EncodingLineSeparator; set => _settingsEncodingLinesProvider.EncodingLineSeparator = value; }
        public int EncodingMaximumLineLength { get => _settingsEncodingLinesProvider.EncodingMaximumLineLength; set => _settingsEncodingLinesProvider.EncodingMaximumLineLength = value; }
        public bool EncodingRequireTerminalLineSeparator { get => _settingsEncodingLinesProvider.EncodingRequireTerminalLineSeparator; set => _settingsEncodingLinesProvider.EncodingRequireTerminalLineSeparator = value; }
    }
}
