using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsSafeCharactersProvider : SettingsBase.SettingsProvider, ISettingsSafeCharacters
    {
        private const int TableSize = 128;

        private string _safeCharacters = String.Empty;
        private string _excludedCharacters = String.Empty;
        private string _whitespaceCharacters = String.Empty;
        private string _newLineCharacters = String.Empty;
        private string _escapeCharacters = String.Empty;
        private ImmutableArray<byte> _decodingTable = ImmutableArray<byte>.Empty;

        public SettingsSafeCharactersProvider ( SettingsBase settingsBase )
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsSafeCharactersProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsSafeCharactersProvider)inheritedSettings;
        }

        public ImmutableArray<byte> GetSafeCharacters ()
        {
            return _decodingTable;
        }

        private ImmutableArray<byte> BuildTable ( string safeCharacters, string excludedCharacters, string whitespaceCharacters, string escapeCharacters, string newLineCharacters )
        {
            var decodingTable = ImmutableArray.CreateBuilder<byte>(TableSize);
            decodingTable.Count = TableSize;

            // excluded characters cannot be a "safe" character..
            for (int iIndex = 0; iIndex < excludedCharacters.Length; iIndex++)
            {
                char excludedChar = excludedCharacters[iIndex];
                if (excludedChar > 127)
                    throw new ArgumentException(Resources.AlphabetOrdinalMustBeByte);

                if (decodingTable[excludedChar] != SettingsCharacterTypes.CharSafeInvalid)
                    throw new ArgumentException(Resources.AlphabetConflict);

                // flag the character as excluded... 
                decodingTable[excludedChar] = SettingsCharacterTypes.CharSafeExcluded;
            }

            // safe characters (or printable)
            for (int iIndex = 0; iIndex < safeCharacters.Length; iIndex++)
            {
                char safeChar = safeCharacters[iIndex];
                if (safeChar > 127)
                    throw new ArgumentException(Resources.AlphabetOrdinalMustBeByte);

                if (decodingTable[safeChar] != SettingsCharacterTypes.CharSafeInvalid)
                    throw new ArgumentException(Resources.AlphabetConflict);

                decodingTable[safeChar] = SettingsCharacterTypes.CharSafe;
            }

            void Apply ( string characters, byte characterFlag )
            {
                for (int iIndex = 0; iIndex < characters.Length; iIndex++)
                {
                    char characterChar = characters[iIndex];
                    if (characterChar > 127)
                        throw new ArgumentException(Resources.AlphabetOrdinalMustBeByte);

                    decodingTable[characterChar] = (byte)(decodingTable[characterChar] | characterFlag);
                }
            }

            // apply flags..
            Apply(whitespaceCharacters, SettingsCharacterTypes.CharSafeFlagWhitespace);
            Apply(escapeCharacters, SettingsCharacterTypes.CharSafeFlagEscape);
            Apply(newLineCharacters, SettingsCharacterTypes.CharSafeFlagNewLine);

            return decodingTable.MoveToImmutable();
        }

        private void RebuildTable ( string safeCharacters, string excludedCharacters, string whitespaceCharacters, string escapeCharacters, string newLineCharacters )
        {
            _decodingTable = BuildTable(safeCharacters, excludedCharacters, whitespaceCharacters, escapeCharacters, newLineCharacters);

            _safeCharacters = safeCharacters;
            _excludedCharacters = excludedCharacters;
        }

        public string SafeCharacters
        {
            get => _safeCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                try
                {
                    RebuildTable(value, _excludedCharacters, _whitespaceCharacters, _escapeCharacters, _newLineCharacters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }

        public bool DecodingIgnoreInvalidCharacters
        {
            get => GetFlag(SettingsFlags.FlagIgnoreInvalidCharacters);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagIgnoreInvalidCharacters, value);
            }
        }

        public string ExcludedCharacters
        {
            get => _excludedCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                try
                {
                    RebuildTable(_safeCharacters, value, _whitespaceCharacters, _escapeCharacters, _newLineCharacters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }

        public string WhitespaceCharacters
        {
            get => _whitespaceCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                try
                {
                    RebuildTable(_safeCharacters, _excludedCharacters, value, _escapeCharacters, _newLineCharacters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }

        public string EscapeCharacters
        {
            get => _escapeCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                try
                {
                    RebuildTable(_safeCharacters, _excludedCharacters, _whitespaceCharacters, value, _newLineCharacters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }

        public string NewLineCharacters
        {
            get => _newLineCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                try
                {
                    RebuildTable(_safeCharacters, _excludedCharacters, _whitespaceCharacters, _escapeCharacters, value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }
    }
}
