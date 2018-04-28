using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Fs.Binary.Codecs.Common
{
    public abstract class AlphabetBuilder
    {
        private static readonly string[] DefaultAlphabet = new string[] { String.Empty };
        private static readonly ImmutableArray<byte> EmptyAffixes;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharAlphabet = 0x00; // character is an alphabet character
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecial = 0x80; // character is not an alphabet character
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharTypeMask = 0x80; // mask used to determine character type

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecialInvalid = 0x80; // character is invalid
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecialIgnored = 0x81; // character is silently ignored
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecialPadding = 0x82; // character is a padding character

        protected const byte CharSpecialDescendantReserved = 0x8C;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagIncludeTerminatingLineSeparator = 0x00000001;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagHasPaddingCharacter = 0x000000002;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagIgnoreInvalidCharacters = 0x00000004;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagIgnoreInvalidPadding = 0x00000008;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagRequirePadding = 0x00000010;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagRequireTrailingZeroBits = 0x00000020;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagIgnoreInvalidFinalQuantum = 0x00000040;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagHasPrefixes = 0x00000080;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagHasPostfixes = 0x00000100;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagRequirePrefix = 0x00000200;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagRequirePostfix = 0x00000400;

        // any flags matching this mask are reserved from descendants..
        protected const int FlagsDescendantReserved = 0xFF00000;
        protected const int FlagsDescendantReservedBase = 0x01000000;

        private readonly int _alphabetLength;
        private ImmutableArray<string> _alphabetCharacters;
        private string _ignoredCharacters;
        private ImmutableDictionary<char, byte> _specialCharacters;
        private ImmutableArray<byte> _decodingTable;
        private ImmutableArray<string> _decodingPrefixes;
        private ImmutableArray<string> _decodingPostfixes;
        private ImmutableArray<byte> _decodingAffixTable;
        private string _lineSeparator;
        private string _encodingPrefix;
        private string _encodingPostfix;
        private int _maximumLineLength;
        private int _minimumInputBuffer;
        private int _flags;
        private char? _paddingCharacter;
        private bool _isProtected;

        static AlphabetBuilder ()
        {
            var arrayBuilder = ImmutableArray.CreateBuilder<byte>(128);
            arrayBuilder.Count = 128;
            EmptyAffixes = arrayBuilder.MoveToImmutable();
        }

        protected AlphabetBuilder ( string alphabet )
        {
            if (alphabet == null)
                throw new ArgumentNullException(nameof(alphabet));

            _alphabetLength = -1;
            _ignoredCharacters = String.Empty;
            _specialCharacters = ImmutableDictionary<char, byte>.Empty;
            _lineSeparator = String.Empty;
            _maximumLineLength = 76;
            _encodingPrefix = String.Empty;
            _encodingPostfix = String.Empty;
            _decodingPostfixes = ImmutableArray<string>.Empty;
            _decodingPrefixes = ImmutableArray<string>.Empty;
            _decodingAffixTable = EmptyAffixes;
            _minimumInputBuffer = 1;
            _isProtected = false;
            _flags = 0;

            AlphabetCharacters = new string[] { alphabet };
        }

        protected AlphabetBuilder ( int alphabetLength, string[] alphabet )
        {
            if ((alphabetLength <= 0) || (alphabetLength > 128))
                throw new ArgumentOutOfRangeException(nameof(alphabetLength));

            _alphabetLength = alphabetLength;
            _ignoredCharacters = String.Empty;
            _specialCharacters = ImmutableDictionary<char, byte>.Empty;
            _lineSeparator = String.Empty;
            _maximumLineLength = 76;
            _encodingPrefix = String.Empty;
            _encodingPostfix = String.Empty;
            _decodingPostfixes = ImmutableArray<string>.Empty;
            _decodingPrefixes = ImmutableArray<string>.Empty;
            _decodingAffixTable = EmptyAffixes;
            _minimumInputBuffer = 1;
            _isProtected = false;
            _flags = 0;

            // build everything..
            AlphabetCharacters = alphabet;
        }

        protected AlphabetBuilder ( AlphabetBuilder inheritedBuilder, bool isProtected )
        {
            if (inheritedBuilder == null)
                throw new ArgumentNullException(nameof(inheritedBuilder));

            _alphabetLength = inheritedBuilder._alphabetLength;
            _alphabetCharacters = inheritedBuilder._alphabetCharacters;
            _ignoredCharacters = inheritedBuilder._ignoredCharacters;
            _specialCharacters = inheritedBuilder._specialCharacters;
            _decodingTable = inheritedBuilder._decodingTable;
            _lineSeparator = inheritedBuilder._lineSeparator;
            _maximumLineLength = inheritedBuilder._maximumLineLength;
            _encodingPrefix = inheritedBuilder._encodingPrefix;
            _encodingPostfix = inheritedBuilder._encodingPostfix;
            _paddingCharacter = inheritedBuilder._paddingCharacter;
            _decodingPostfixes = inheritedBuilder._decodingPostfixes;
            _decodingPrefixes = inheritedBuilder._decodingPrefixes;
            _decodingAffixTable = inheritedBuilder._decodingAffixTable;
            _minimumInputBuffer = inheritedBuilder._minimumInputBuffer;
            _flags = inheritedBuilder._flags;
            _isProtected = isProtected;
        }

        protected int GetFlags () => _flags;
        protected string GetAlphabet () => _alphabetCharacters[0];
        protected string GetLineSeparator () => !String.IsNullOrEmpty(_lineSeparator) ? _lineSeparator : null;
        protected int GetMaximumLineLength () => _maximumLineLength;
        protected string GetEncodingPrefix () => _encodingPrefix;
        protected string GetEncodingPostfix () => _encodingPostfix;
        protected char GetPaddingCharacter () => _paddingCharacter ?? '\0';
        protected ImmutableArray<byte> GetDecodingTable () => _decodingTable;
        protected ImmutableArray<byte> GetDecodingAffixTable () => _decodingAffixTable;
        protected ImmutableArray<string> GetDecodingPrefixes () => _decodingPrefixes;
        protected ImmutableArray<string> GetDecodingPostfixes () => _decodingPostfixes;

        protected bool GetFlag ( int flag )
        {
            return (_flags & flag) == flag;
        }

        protected void SetFlag ( int flag, bool set )
        {
            _flags = (_flags & (~flag)) | (set ? flag : 0);
        }

        protected void AddSpecialCharacters ( IEnumerable<KeyValuePair<char, byte>> specialCharacters )
        {
            if (specialCharacters == null)
                throw new ArgumentNullException(nameof(specialCharacters));

            try
            {
                var newCharacters = _specialCharacters.AddRange(specialCharacters);
                BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
                _specialCharacters = newCharacters;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("TODO: charcater conflict", nameof(specialCharacters), ex);
            }
        }

        protected void RemoveSpecialCharacters ( IEnumerable<char> specialCharacters )
        {
            if (specialCharacters == null)
                throw new ArgumentNullException(nameof(specialCharacters));

            var newCharacters = _specialCharacters.RemoveRange(specialCharacters);
            BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
            _specialCharacters = newCharacters;
        }

        protected void RemoveSpecialCharacter ( char specialCharacter )
        {
            var newCharacters = _specialCharacters.Remove(specialCharacter);
            BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
            _specialCharacters = newCharacters;
        }

        protected void ClearSpecialCharacters ()
        {
            var newCharacters = ImmutableDictionary<char, byte>.Empty;
            BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
            _specialCharacters = newCharacters;
        }

        protected void SetSpecialCharacters ( IEnumerable<char> removeCharacters, IEnumerable<KeyValuePair<char, byte>> specialCharacters )
        {
            // assumes that we can enumerate the enumerable multiple times..
            ImmutableDictionary<char, byte> newCharacters = (removeCharacters != null) ?
                _specialCharacters.RemoveRange(removeCharacters) :
                _specialCharacters;

            // throws ArgumentException if specialCharacters conflicts with an existing value..
            newCharacters = newCharacters.AddRange(specialCharacters);
            BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
            _specialCharacters = newCharacters;
        }

        protected void SetSpecialCharacter ( char specialCharacter, byte charValue )
        {
            // throws ArgumentException if specialCharacters conflicts with an existing value..
            SetSpecialCharacters(new[] { specialCharacter },
                                 new[] { new KeyValuePair<char, byte>(specialCharacter, charValue) });
        }

        protected void SetSpecialCharacter ( char? originalCharacter, char? newCharacter, byte charValue )
        {
            if ((originalCharacter == null) && (newCharacter == null))
                return;

            ImmutableDictionary<char, byte> newCharacters = _specialCharacters;
            if (originalCharacter.HasValue)
                newCharacters = _specialCharacters.Remove(originalCharacter.Value);

            if (newCharacter.HasValue)
                newCharacters = _specialCharacters.Add(newCharacter.Value, charValue);

            BuildDecodingTable(GetCurrentAlphabetCharacters(), GetCurrentIgnoredCharacters(), newCharacters);
            _specialCharacters = newCharacters;
        }

        protected ImmutableArray<string> GetCurrentAlphabetCharacters () => _alphabetCharacters;
        protected string GetCurrentIgnoredCharacters () => _ignoredCharacters;
        protected ImmutableDictionary<char, byte> GetCurrentSpecialCharacters () => _specialCharacters;

        protected void BuildDecodingAffixTable ( ImmutableArray<string> prefixes, ImmutableArray<string> postfixes )
        {
            if ((prefixes.Length == 0) && (postfixes.Length == 0))
            {
                _decodingAffixTable = EmptyAffixes;
                _minimumInputBuffer = 1;
            }
            else
            {
                int maximumAffixLength = 0;

                var newTable = ImmutableArray.CreateBuilder<byte>(128);
                newTable.Count = newTable.Capacity;
                for (int iIndex = 0; iIndex < newTable.Capacity; iIndex++)
                    newTable[iIndex] = 0;

                for (int iIndex = 0; iIndex < prefixes.Length; iIndex++)
                {
                    var affixString = prefixes[iIndex];
                    if (affixString.Length > maximumAffixLength) maximumAffixLength = affixString.Length;
                    newTable[affixString[0]] = 0x01;
                }

                for (int iIndex = 0; iIndex < postfixes.Length; iIndex++)
                {
                    var affixString = postfixes[iIndex];
                    if (affixString.Length > maximumAffixLength) maximumAffixLength = affixString.Length;
                    newTable[affixString[0]] = 0x02;
                }

                _decodingAffixTable = newTable.MoveToImmutable();
                _minimumInputBuffer = maximumAffixLength + 1;
            }
        }

        protected void BuildDecodingTable ( ImmutableArray<string> alphabetCharacters,
                                            string ignoredCharacters,
                                            ImmutableDictionary<char, byte> specialCharacters )
        {
            var newTable = ImmutableArray.CreateBuilder<byte>(128);
            newTable.Count = newTable.Capacity;
            for (int iIndex = 0; iIndex < newTable.Capacity; iIndex++)
                newTable[iIndex] = CharSpecialInvalid;

            // fill in all of the alphabet characters...
            for (int iAlphabet = 0; iAlphabet < alphabetCharacters.Length; iAlphabet++)
            {
                string alphabetChars = alphabetCharacters[iAlphabet];
                for (int iIndex = 0; iIndex < alphabetChars.Length; iIndex++)
                {
                    char alphabetChar = alphabetChars[iIndex];
                    if ((alphabetChar >= newTable.Capacity) ||
                        ((newTable[alphabetChar] != CharSpecialInvalid) &&
                         (newTable[alphabetChar] != iIndex)))
                        throw new ArgumentException("TODO: character conflict");

                    newTable[alphabetChar] = (byte)iIndex;
                }
            }

            // add special characters..
            foreach (var specialPair in specialCharacters)
            {
                char specialChar = specialPair.Key;
                if ((specialChar >= newTable.Capacity) ||
                    ((newTable[specialChar] != CharSpecialInvalid) &&
                     (newTable[specialChar] != specialPair.Value)))
                    throw new ArgumentException("TODO: character conflict");

                newTable[specialChar] = specialPair.Value;
            }

            // add ignorable characters, 
            for (int iIndex = 0; iIndex < ignoredCharacters.Length; iIndex++)
            {
                char ignoredChar = ignoredCharacters[iIndex];
                if ((ignoredChar >= newTable.Capacity) ||
                    ((newTable[ignoredChar] != CharSpecialInvalid) &&
                     (newTable[ignoredChar] != CharSpecialIgnored)))
                    throw new ArgumentException("TODO: character conflict");

                newTable[ignoredChar] = CharSpecialIgnored;
            }

            // store the new decoding table..
            _decodingTable = newTable.MoveToImmutable();
        }

        protected void CheckWritable ()
        {
            if (_isProtected)
                throw new InvalidOperationException(Resources.InstanceIsReadOnly);
        }

        internal int GetMinimumInputBuffer ()
        {
            return _minimumInputBuffer;
        }

        private string[] GetAlphabets ()
        {
            string[] alphabets = new string[_alphabetCharacters.Length];
            _alphabetCharacters.CopyTo(alphabets);
            return alphabets;
        }

        private string[] GetDecodingPrefixesCopy ()
        {
            string[] prefixes = new string[_decodingPrefixes.Length];
            _decodingPrefixes.CopyTo(prefixes);
            return prefixes;
        }

        private string[] GetDecodingPostfixesCopy ()
        {
            string[] postfixes = new string[_decodingPostfixes.Length];
            _decodingPostfixes.CopyTo(postfixes);
            return postfixes;
        }

        private void ValidateAlphabet ( string[] alphabet, string parameterName )
        {
            if (alphabet == null)
                throw new ArgumentNullException(parameterName);

            if (alphabet.Length == 0)
                throw new ArgumentException(Resources.AlphabetMustHaveAtleastOneElement, parameterName);

            for (int iIndex = 0; iIndex < alphabet.Length; iIndex++)
                if ((alphabet[iIndex] == null) || ((_alphabetLength != -1) && (alphabet[iIndex].Length != _alphabetLength)))
                    throw new ArgumentException(String.Format(Resources.AlphabetMustExactLength, _alphabetLength), parameterName);

            // TODO: verify each alphabet contains chars 127 or less

        }

        private void ValidateAffixes ( string[] affixes, string parameterName )
        {
            if (affixes == null)
                throw new ArgumentNullException(parameterName);

            // empty list is fine..
            if (affixes.Length == 0)
                return;

            // limited to 31 because we use a uint to track matches 
            if (affixes.Length > 31)
                throw new ArgumentException("TODO: message -- cannot be more than 31 affixes", parameterName);

            for (int iIndex = 0; iIndex < affixes.Length; iIndex++)
                if (String.IsNullOrEmpty(affixes[iIndex]))
                    throw new ArgumentException("TODO: message -- element cannot be null or empty.", parameterName);

            // TODO: make sure each string is chars less than 128...

            // all entries must be unique..
            for (int iIndex = 0; iIndex < affixes.Length - 1; iIndex++)
            {
                string currentEntry = affixes[iIndex];
                for (int jIndex = iIndex + 1; jIndex < affixes.Length; jIndex++)
                    if (String.Equals(currentEntry, affixes[jIndex], StringComparison.Ordinal))
                        throw new ArgumentException("TODO: message -- duplicate element.", parameterName);
            }
        }

        internal bool IsReadOnly
        {
            get => _isProtected;
            set => _isProtected = value;
        }

        internal int EncodingAffixLength
        {
            get
            {
                return _encodingPrefix.Length + _encodingPostfix.Length;
            }
        }

        protected string[] AlphabetCharacters
        {
            get => GetAlphabets();
            set
            {
                CheckWritable();
                ValidateAlphabet(value, nameof(value));

                try
                {
                    var newAlphabet = value.ToImmutableArray<string>();
                    BuildDecodingTable(newAlphabet, GetCurrentIgnoredCharacters(), GetCurrentSpecialCharacters());
                    _alphabetCharacters = value.ToImmutableArray<string>();
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("TODO: charcater conflict", nameof(value), ex);
                }
            }
        }

        protected string[] DecodingPrefixes
        {
            get => GetDecodingPrefixesCopy();
            set
            {
                CheckWritable();
                ValidateAffixes(value, nameof(value));

                // order the affixes from shortest to longest..
                var affixList = value.OrderBy(v => v.Length).ToImmutableArray();
                BuildDecodingAffixTable(affixList, GetDecodingPostfixes());
                SetFlag(FlagHasPrefixes, affixList.Length != 0);
                _decodingPrefixes = affixList;
            }
        }

        protected string[] DecodingPostfixes
        {
            get => GetDecodingPostfixesCopy();
            set
            {
                CheckWritable();
                ValidateAffixes(value, nameof(value));

                // order the affixes from shortest to longest..
                var affixList = value.OrderBy(v => v.Length).ToImmutableArray();
                BuildDecodingAffixTable(GetDecodingPrefixes(), affixList);
                SetFlag(FlagHasPostfixes, affixList.Length != 0);
                _decodingPostfixes = affixList;
            }
        }

        protected string DecodingIgnoredCharacters
        {
            get => _ignoredCharacters;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();

                try
                {
                    BuildDecodingTable(GetCurrentAlphabetCharacters(), value, GetCurrentSpecialCharacters());
                    _ignoredCharacters = value;
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("TODO: charcater conflict", nameof(value), ex);
                }
            }
        }

        protected char? PaddingCharacter
        {
            get => _paddingCharacter;
            set
            {
                CheckWritable();

                SetSpecialCharacter(_paddingCharacter, value, CharSpecialPadding);
                _paddingCharacter = value;
                SetFlag(FlagHasPaddingCharacter, _paddingCharacter.HasValue);
            }
        }

        protected string EncodingPrefix
        {
            get => _encodingPrefix;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                CheckWritable();
                _encodingPrefix = value;
            }
        }

        protected string EncodingPostfix
        {
            get => _encodingPostfix;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                CheckWritable();
                _encodingPostfix = value;
            }
        }

        protected string EncodingLineSeparator
        {
            get => _lineSeparator;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                CheckWritable();
                _lineSeparator = value;
            }
        }

        protected int EncodingMaximumLineLength
        {
            get => _maximumLineLength;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckWritable();
                _maximumLineLength = value;
            }
        }

        protected bool EncodingIncludeTerminatingLineSeparator
        {
            get => GetFlag(FlagIncludeTerminatingLineSeparator);
            set => SetFlag(FlagIncludeTerminatingLineSeparator, value);
        }

        protected bool DecodingIgnoreInvalidCharacters
        {
            get => GetFlag(FlagIgnoreInvalidCharacters);
            set => SetFlag(FlagIgnoreInvalidCharacters, value);
        }

        protected bool DecodingIgnoreInvalidPadding
        {
            get => GetFlag(FlagIgnoreInvalidPadding);
            set => SetFlag(FlagIgnoreInvalidPadding, value);
        }

        protected bool DecodingRequirePadding
        {
            get => GetFlag(FlagRequirePadding);
            set => SetFlag(FlagRequirePadding, value);
        }

        protected bool DecodingRequireTrailingZeroBits
        {
            get => GetFlag(FlagRequireTrailingZeroBits);
            set => SetFlag(FlagRequireTrailingZeroBits, value);
        }

        protected bool DecodingIgnoreInvalidFinalQuantum
        {
            get => GetFlag(FlagIgnoreInvalidFinalQuantum);
            set => SetFlag(FlagIgnoreInvalidFinalQuantum, value);
        }

        protected bool DecodingPrefixRequired
        {
            get => GetFlag(FlagRequirePrefix);
            set => SetFlag(FlagRequirePrefix, value);
        }

        protected bool DecodingPostfixRequired
        {
            get => GetFlag(FlagRequirePostfix);
            set => SetFlag(FlagRequirePostfix, value);
        }
    }
}
