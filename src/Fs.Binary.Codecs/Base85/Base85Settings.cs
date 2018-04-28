using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.Base85
{
    public class Base85Settings : ExposedAlphabetBuilder
    {
        private static readonly string[] DefaultAlphabet = new string[]
        {
            "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstu"
        };

        public static readonly Base85Settings Default = new Base85Settings()
        {
            IsReadOnly = true
        };

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecialZ = CharSpecialDescendantReserved;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const byte CharSpecialY = CharSpecialDescendantReserved + 1;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagForceFullQuantums = FlagsDescendantReservedBase;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagHasZCharacter = FlagsDescendantReservedBase << 1;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagHasYCharacter = FlagsDescendantReservedBase << 2;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagIgnoreOverflow = FlagsDescendantReservedBase << 3;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const int FlagRequireFullQuantum = FlagsDescendantReservedBase << 4;

        private char? _zCharacter;
        private char? _yCharacter;

        public Base85Settings ()
            : base(85, DefaultAlphabet)
        {
        }

        public Base85Settings ( Base85Settings inheritedSettings, bool isProtected )
            : base(inheritedSettings, isProtected)
        {
            _zCharacter = inheritedSettings._zCharacter;
            _yCharacter = inheritedSettings._yCharacter;
        }

        internal void GetEncoderSettings ( out string alphabet,
                                           out int flags,
                                           out string lineSeparator,
                                           out int maximumLineLength,
                                           out string initialPrefix,
                                           out string finalPrefix,
                                           out char? charZ,
                                           out char? charY )
        {
            alphabet = GetAlphabet();
            flags = GetFlags();
            lineSeparator = GetLineSeparator();
            maximumLineLength = GetMaximumLineLength();
            initialPrefix = GetEncodingPrefix();
            finalPrefix = GetEncodingPostfix();
            charZ = _zCharacter;
            charY = _yCharacter;
        }

        internal void GetDecoderSettings ( out ImmutableArray<byte> decodingTable,
                                           out ImmutableArray<byte> decodingAffixTable,
                                           out ImmutableArray<string> decodingPrefixes,
                                           out ImmutableArray<string> decodingPostfixes,
                                           out int flags )
        {
            decodingTable = GetDecodingTable();
            decodingAffixTable = GetDecodingAffixTable();
            decodingPrefixes = GetDecodingPrefixes();
            decodingPostfixes = GetDecodingPostfixes();
            flags = GetFlags();
        }

        private void SetCharacter ( ref char? charField, int flagValue, char? value, byte charValue )
        {
            SetSpecialCharacter(charField, value, charValue);
            charField = value;
            SetFlag(flagValue, charField.HasValue);
        }

        public Base85Settings ToReadOnly () => new Base85Settings(this, true);

        public bool EncodingCompleteFinalQuantum
        {
            get => GetFlag(FlagForceFullQuantums);
            set => SetFlag(FlagForceFullQuantums, value);
        }

        public char? FourZerosAbbreviation
        {
            get => _zCharacter;
            set => SetCharacter(ref _zCharacter, FlagHasZCharacter, value, CharSpecialZ);
        }

        public char? FourSpacesAbbreviation
        {
            get => _yCharacter;
            set => SetCharacter(ref _yCharacter, FlagHasYCharacter, value, CharSpecialY);
        }

        public bool DecodingIgnoreOverflow
        {
            get => GetFlag(FlagIgnoreOverflow);
            set => SetFlag(FlagIgnoreOverflow, value);
        }

        public bool DecodingRequireCompleteFinalQuantum
        {
            get => GetFlag(FlagRequireFullQuantum);
            set => SetFlag(FlagRequireFullQuantum, value);
        }
    }
}
