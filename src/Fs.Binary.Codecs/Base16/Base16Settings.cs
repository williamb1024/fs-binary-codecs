using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Settings;
using Fs.Binary.Codecs.Settings.Providers;

namespace Fs.Binary.Codecs.Base16
{
    public class Base16Settings : SettingsBase
    {
        private const int AlphabetSize = 16;
        private static readonly string[] DefaultAlphabet = new string[]
        {
            "0123456789ABCDEF",
            "0123456789abcdef",
        };

        public static readonly Base16Settings Default = new Base16Settings()
        {
            Alphabets = DefaultAlphabet,
            IsProtected = true
        };

        private readonly SettingsFlagsProvider _flagsProvider;
        private readonly SettingsAlphabetProvider _alphabetProvider;
        private readonly SettingsDecodingAffixesProvider _decodingAffixes;
        private readonly SettingsEncodingAffixProvider _encodingAffixes;
        private readonly SettingsEncodingLinesProvider _encodingLines;
        private readonly SettingsDecodingCheckFinalQuantumProvider _decodingFinalQuantumProvider;

        private Base16Settings ()
            : base()
        {
            _alphabetProvider = new SettingsAlphabetProvider(this, AlphabetSize);
            _decodingAffixes = new SettingsDecodingAffixesProvider(this);
            _encodingAffixes = new SettingsEncodingAffixProvider(this);
            _encodingLines = new SettingsEncodingLinesProvider(this);
            _decodingFinalQuantumProvider = new SettingsDecodingCheckFinalQuantumProvider(this);
            _flagsProvider = new SettingsFlagsProvider(this);
        }

        public Base16Settings ( Base16Settings inheritedSettings )
            : this()
        {
            if (inheritedSettings == null)
                throw new ArgumentNullException(nameof(inheritedSettings));

            InheritSettings(inheritedSettings);
        }

        private Base16Settings ( Base16Settings inheritedSettings, bool isProtected )
            : this(inheritedSettings)
        {
            IsProtected = isProtected;
        }

        internal void GetEncoderSettings ( out string alphabet,
                                           out int flags,
                                           out string lineSeparator,
                                           out int maximumLineLength,
                                           out string initialPrefix,
                                           out string finalPrefix )
        {
            alphabet = _alphabetProvider.GetEncodingAlphabet();
            flags = _flagsProvider.GetFlags();
            lineSeparator = _encodingLines.GetEncodingLineSeparator();
            maximumLineLength = _encodingLines.GetEncodingMaximumLineLength();
            initialPrefix = _encodingAffixes.GetEncodingPrefix();
            finalPrefix = _encodingAffixes.GetEncodingPostfix();
        }

        internal void GetDecoderSettings ( out ImmutableArray<byte> decodingTable,
                                           out ImmutableArray<byte> decodingAffixTable,
                                           out ImmutableArray<string> decodingPrefixes,
                                           out ImmutableArray<string> decodingPostfixes,
                                           out int flags )
        {
            decodingTable = _alphabetProvider.GetDecodingTable();
            decodingAffixTable = _decodingAffixes.GetDecodingTable();
            decodingPrefixes = _decodingAffixes.GetDecodingPrefixes();
            decodingPostfixes = _decodingAffixes.GetDecodingPostfixes();
            flags = _flagsProvider.GetFlags();
        }

        // GetHexEncoding / GetHexDecoding are used by other encodings to encode/decode hexadecimal values
        internal string GetHexEncoding () => _alphabetProvider.GetEncodingAlphabet();
        internal ImmutableArray<byte> GetHexDecoding () => _alphabetProvider.GetDecodingTable();

        public Base16Settings ToReadOnly () => (IsProtected) ? this : new Base16Settings(this, true);

        public string[] Alphabets { get => _alphabetProvider.Alphabets; set => _alphabetProvider.Alphabets = value; }
        public string DecodingIgnorableCharacters { get => _alphabetProvider.DecodingIgnorableCharacters; set => _alphabetProvider.DecodingIgnorableCharacters = value; }
        public bool DecodingIgnoreInvalidCharacters { get => _alphabetProvider.DecodingIgnoreInvalidCharacters; set => _alphabetProvider.DecodingIgnoreInvalidCharacters = value; }
        public string[] DecodingPrefixes { get => _decodingAffixes.DecodingPrefixes; set => _decodingAffixes.DecodingPrefixes = value; }
        public string[] DecodingPostfixes { get => _decodingAffixes.DecodingPostfixes; set => _decodingAffixes.DecodingPostfixes = value; }
        public bool DecodingPrefixRequired { get => _decodingAffixes.DecodingPrefixRequired; set => _decodingAffixes.DecodingPrefixRequired = value; }
        public bool DecodingPostfixRequired { get => _decodingAffixes.DecodingPostfixRequired; set => _decodingAffixes.DecodingPostfixRequired = value; }
        public int DecodingMinimumInputBuffer => _decodingAffixes.DecodingMinimumInputBuffer;
        public string EncodingPrefix { get => _encodingAffixes.EncodingPrefix; set => _encodingAffixes.EncodingPrefix = value; }
        public string EncodingPostfix { get => _encodingAffixes.EncodingPostfix; set => _encodingAffixes.EncodingPostfix = value; }
        public int EncodingAffixLength => _encodingAffixes.EncodingAffixLength;
        public string EncodingLineSeparator { get => _encodingLines.EncodingLineSeparator; set => _encodingLines.EncodingLineSeparator = value; }
        public int EncodingMaximumLineLength { get => _encodingLines.EncodingMaximumLineLength; set => _encodingLines.EncodingMaximumLineLength = value; }
        public bool EncodingRequireTerminalLineSeparator { get => _encodingLines.EncodingRequireTerminalLineSeparator; set => _encodingLines.EncodingRequireTerminalLineSeparator = value; }
        public bool DecodingIgnoreInvalidFinalQuantum { get => _decodingFinalQuantumProvider.DecodingIgnoreInvalidFinalQuantum; set => _decodingFinalQuantumProvider.DecodingIgnoreInvalidFinalQuantum = value; }
    }
}
