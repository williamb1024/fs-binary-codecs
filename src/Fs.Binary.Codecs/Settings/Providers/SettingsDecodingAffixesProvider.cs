using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings.Providers
{
    public sealed class SettingsDecodingAffixesProvider : SettingsBase.SettingsProvider, ISettingsDecodingAffixes
    {
        private const int AffixTableLength = 128;

        private static readonly ImmutableArray<byte> EmptyAffixes;

        private ImmutableArray<string> _decodingPrefixes = ImmutableArray<string>.Empty;
        private ImmutableArray<string> _decodingPostfixes = ImmutableArray<string>.Empty;
        private ImmutableArray<byte> _decodingTable = EmptyAffixes;
        private int _decodingMinimumInput = 1;

        static SettingsDecodingAffixesProvider ()
        {
            var arrayBuilder = ImmutableArray.CreateBuilder<byte>(AffixTableLength);
            arrayBuilder.Count = AffixTableLength;
            EmptyAffixes = arrayBuilder.MoveToImmutable();
        }

        public SettingsDecodingAffixesProvider ( SettingsBase settingsBase )
            : base(settingsBase)
        {
        }

        public override void InheritSettings ( SettingsBase.SettingsProvider inheritedSettings )
        {
            if (!(inheritedSettings is SettingsDecodingAffixesProvider))
                throw new InvalidOperationException("Cannot inherit from a different type.");

            var inheritSettings = (SettingsDecodingAffixesProvider)inheritedSettings;
            _decodingPrefixes = inheritSettings._decodingPrefixes;
            _decodingPostfixes = inheritSettings._decodingPostfixes;
            _decodingTable = inheritSettings._decodingTable;
            _decodingMinimumInput = inheritSettings._decodingMinimumInput;
        }

        public ImmutableArray<byte> GetDecodingTable ()
        {
            return _decodingTable;
        }

        public ImmutableArray<string> GetDecodingPrefixes ()
        {
            return _decodingPrefixes;
        }

        public ImmutableArray<string> GetDecodingPostfixes ()
        {
            return _decodingPostfixes;
        }

        private (ImmutableArray<byte> decodingTable, int minimumInput) BuildTable ( ImmutableArray<string> prefixes, ImmutableArray<string> postfixes )
        {
            if ((prefixes.Length == 0) && (postfixes.Length == 0))
                return (EmptyAffixes, 1);

            int maximumAffixLength = 0;

            var newTable = ImmutableArray.CreateBuilder<byte>(AffixTableLength);
            newTable.Count = newTable.Capacity;
            for (int iIndex = 0; iIndex < newTable.Capacity; iIndex++)
                newTable[iIndex] = SettingsCharacterTypes.AffixNone;

            for (int iIndex = 0; iIndex < prefixes.Length; iIndex++)
            {
                var affixString = prefixes[iIndex];
                if (affixString.Length > maximumAffixLength) maximumAffixLength = affixString.Length;
                newTable[affixString[0]] |= SettingsCharacterTypes.AffixStartPrefix;
            }

            for (int iIndex = 0; iIndex < postfixes.Length; iIndex++)
            {
                var affixString = postfixes[iIndex];
                if (affixString.Length > maximumAffixLength) maximumAffixLength = affixString.Length;
                newTable[affixString[0]] |= SettingsCharacterTypes.AffixStartPostfix;
            }

            return (newTable.MoveToImmutable(), maximumAffixLength + 1);
        }

        private void RebuildTable ( ImmutableArray<string> prefixes, ImmutableArray<string> postfixes )
        {
            (_decodingTable, _decodingMinimumInput) = BuildTable(prefixes, postfixes);
        }

        private void ValidateAffixes ( string[] affixes, string parameterName )
        {
            if (affixes == null)
                throw new ArgumentNullException(parameterName);

            if (affixes.Length == 0)
                return;

            if (affixes.Length > 31)
                throw new ArgumentException(Resources.AffixArrayLimitedTo31, parameterName);

            for (int iIndex = 0; iIndex < affixes.Length; iIndex++)
            {
                if (affixes[iIndex] == null)
                    throw new ArgumentException(Resources.AffixCannotBeNull, parameterName);

                if (affixes[iIndex].Length == 0)
                    throw new ArgumentException(Resources.AffixCannotBeEmpty, parameterName);
            }

            // all entries must be unique..
            for (int iIndex = 0; iIndex < affixes.Length - 1; iIndex++)
            {
                string currentEntry = affixes[iIndex];
                for (int jIndex = iIndex + 1; jIndex < affixes.Length; jIndex++)
                    if (String.Equals(currentEntry, affixes[jIndex], StringComparison.Ordinal))
                        throw new ArgumentException(Resources.AffixMustBeUnique, parameterName);
            }
        }

        public string[] DecodingPrefixes
        {
            get => _decodingPrefixes.ToArray();
            set
            {
                ValidateAffixes(value, nameof(value));
                CheckWritable();

                var immutableArray = value.ToImmutableArray();
                RebuildTable(immutableArray, _decodingPostfixes);
                _decodingPrefixes = immutableArray;
                SetFlag(SettingsFlags.FlagHasPrefixes, _decodingPrefixes.Length > 0);
            }
        }

        public string[] DecodingPostfixes
        {
            get => _decodingPostfixes.ToArray();
            set
            {
                ValidateAffixes(value, nameof(value));
                CheckWritable();

                var immutableArray = value.ToImmutableArray();
                RebuildTable(_decodingPrefixes, immutableArray);
                _decodingPostfixes = immutableArray;
                SetFlag(SettingsFlags.FlagHasPostfixes, _decodingPostfixes.Length > 0);
            }
        }

        public bool DecodingPrefixRequired
        {
            get => GetFlag(SettingsFlags.FlagRequirePrefix);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagRequirePrefix, value);
            }
        }

        public bool DecodingPostfixRequired
        {
            get => GetFlag(SettingsFlags.FlagRequirePostfix);
            set
            {
                CheckWritable();
                SetFlag(SettingsFlags.FlagRequirePostfix, value);
            }
        }

        public int DecodingMinimumInputBuffer
        {
            get => _decodingMinimumInput;
        }
    }
}
