using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsAlphabetProvider : SettingsBase.SettingsProvider, ISettingsAlphabet
    {
        private const int AlphabetTableLength = 128;
        private static readonly ImmutableArray<byte> EmptyTable;

        private int _alphabetLength;
        private ImmutableArray<string> _alphabetStrings = ImmutableArray<string>.Empty;
        private ImmutableDictionary<char, byte> _specialCharacters = ImmutableDictionary<char, byte>.Empty;
        private ImmutableArray<byte> _decodingTable = EmptyTable;
        private string _ignorableCharacters = String.Empty;

        static SettingsAlphabetProvider ()
        {
            var arrayBuilder = ImmutableArray.CreateBuilder<byte>(AlphabetTableLength);
            arrayBuilder.Count = AlphabetTableLength;

            for (int iIndex = 0; iIndex < AlphabetTableLength; iIndex++)
                arrayBuilder[iIndex] = SettingsCharacterTypes.CharSpecialInvalid;

            EmptyTable = arrayBuilder.MoveToImmutable();
        }

        public SettingsAlphabetProvider ( SettingsBase settingsBase, int alphabetLength )
            : base(settingsBase)
        {
            if (alphabetLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(alphabetLength));

            _alphabetLength = alphabetLength;
        }

        public void ChangeSpecialCharacter ( ref char? charField, int charFlag, char? newCharacter, byte specialType )
        {
            if (charField == newCharacter)
                return;

            var newBuilder = _specialCharacters.ToBuilder();
            if (charField.HasValue)
            {
                if ((newBuilder.TryGetValue(charField.Value, out var charType)) &&
                    (charType != specialType))
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(charField));

                newBuilder.Remove(charField.Value);
            }

            if (newCharacter.HasValue)
            {
                if (newBuilder.ContainsKey(newCharacter.Value))
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(newCharacter));

                newBuilder.Add(newCharacter.Value, specialType);
            }

            try
            {
                RebuildTable(_alphabetStrings, _ignorableCharacters, newBuilder.ToImmutable());

                charField = newCharacter;
                if (charFlag != 0)
                    SetFlag(charFlag, charField.HasValue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(Resources.AlphabetConflict, ex);
            }
        }

        public string GetEncodingAlphabet ()
        {
            return _alphabetStrings[0];
        }

        public ImmutableArray<byte> GetDecodingTable ()
        {
            return _decodingTable;
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsAlphabetProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsAlphabetProvider)inheritedSettings;
            _alphabetLength = inheritSettings._alphabetLength;
            _alphabetStrings = inheritSettings._alphabetStrings;
            _decodingTable = inheritSettings._decodingTable;
            _ignorableCharacters = inheritSettings._ignorableCharacters;
            _specialCharacters = inheritSettings._specialCharacters;
        }

        private void ValidateAlphabets ( string[] alphabets, string parameterName )
        {
            if (alphabets == null)
                throw new ArgumentNullException(parameterName);

            if (alphabets.Length == 0)
                throw new ArgumentException(Resources.AlphabetMustHaveAtleastOneElement, parameterName);

            for (int iIndex = 0; iIndex < alphabets.Length; iIndex++)
            {
                var alphabetString = alphabets[iIndex];
                if ((alphabetString == null) || (alphabetString.Length != _alphabetLength))
                    throw new ArgumentException(String.Format(Resources.AlphabetMustExactLength, _alphabetLength), parameterName);

                for (int alphabetIndex = 0; alphabetIndex < alphabetString.Length; alphabetIndex++)
                    if (alphabetString[alphabetIndex] >= 128)
                        throw new ArgumentException(Resources.AlphabetOrdinalMustBeByte);
            }
        }

        private void ValidateIgnorableCharacters ( string ignorableCharacters, string parameterName )
        {
            if (ignorableCharacters == null)
                throw new ArgumentNullException(parameterName);

            for (int iIndex = 0; iIndex < ignorableCharacters.Length; iIndex++)
                if (ignorableCharacters[iIndex] >= 128)
                    throw new ArgumentException(Resources.AlphabetOrdinalMustBeByte);
        }

        private ImmutableArray<byte> BuildTable ( ImmutableArray<string> alphabets, string ignorableCharacters, ImmutableDictionary<char, byte> specialCharacters )
        {
            var alphabetTable = ImmutableArray.CreateBuilder<byte>(AlphabetTableLength);
            alphabetTable.Count = AlphabetTableLength;
            for (int iIndex = 0; iIndex < AlphabetTableLength; iIndex++)
                alphabetTable[iIndex] = SettingsCharacterTypes.CharSpecialInvalid;

            // combine all of the supplied alphabets into the table, assigning each alphabet character
            // to it's index.. the character cannot be assigned to multiple indicies, but one index may
            // be assigned to multiple characters.

            for (int alphabetIndex = 0; alphabetIndex < alphabets.Length; alphabetIndex++)
            {
                var alphabetString = alphabets[alphabetIndex];
                for (int iIndex = 0; iIndex < alphabetString.Length; iIndex++)
                {
                    char alphabetChar = alphabetString[iIndex];
                    if ((alphabetChar >= alphabetTable.Count) ||
                        ((alphabetTable[alphabetChar] != SettingsCharacterTypes.CharSpecialInvalid) &&
                         (alphabetTable[alphabetChar] != iIndex)))
                        throw new ArgumentException(Resources.AlphabetConflict);

                    alphabetTable[alphabetChar] = (byte)iIndex;
                }
            }

            // add special characters..
            foreach (var specialPair in specialCharacters)
            {
                char specialChar = specialPair.Key;
                if ((specialChar >= alphabetTable.Count) ||
                    ((alphabetTable[specialChar] != SettingsCharacterTypes.CharSpecialInvalid) &&
                     (alphabetTable[specialChar] != specialPair.Value)))
                    throw new ArgumentException(Resources.AlphabetConflict);

                alphabetTable[specialChar] = specialPair.Value;
            }

            // add ignorable characters, 
            for (int iIndex = 0; iIndex < ignorableCharacters.Length; iIndex++)
            {
                char ignoredChar = ignorableCharacters[iIndex];
                if ((ignoredChar >= alphabetTable.Count) ||
                    ((alphabetTable[ignoredChar] != SettingsCharacterTypes.CharSpecialInvalid) &&
                     (alphabetTable[ignoredChar] != SettingsCharacterTypes.CharSpecialIgnored)))
                    throw new ArgumentException(Resources.AlphabetConflict);

                alphabetTable[ignoredChar] = SettingsCharacterTypes.CharSpecialIgnored;
            }

            return alphabetTable.MoveToImmutable();
        }

        private void RebuildTable ( ImmutableArray<string> alphabets, string ignorableCharacters, ImmutableDictionary<char, byte> specialCharacters )
        {
            // rebuild the decoding table..
            _decodingTable = BuildTable(alphabets, ignorableCharacters, specialCharacters);

            // save all of the incoming values as the base values..
            _alphabetStrings = alphabets;
            _ignorableCharacters = ignorableCharacters;
            _specialCharacters = specialCharacters;
        }

        public string[] Alphabets
        {
            get => _alphabetStrings.ToArray();
            set
            {
                ValidateAlphabets(value, nameof(value));
                CheckWritable();

                try
                {
                    RebuildTable(value.ToImmutableArray<string>(), _ignorableCharacters, _specialCharacters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(Resources.AlphabetConflict, nameof(value), ex);
                }
            }
        }

        public string DecodingIgnorableCharacters
        {
            get => _ignorableCharacters;
            set
            {
                ValidateIgnorableCharacters(value, nameof(value));
                CheckWritable();

                try
                {
                    RebuildTable(_alphabetStrings, value, _specialCharacters);
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
    }
}
