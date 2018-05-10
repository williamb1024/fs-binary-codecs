﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Fs.Binary.Codecs {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Fs.Binary.Codecs.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The affix array may not contain more than 31 elements..
        /// </summary>
        internal static string AffixArrayLimitedTo31 {
            get {
                return ResourceManager.GetString("AffixArrayLimitedTo31", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An affix string cannot be empty..
        /// </summary>
        internal static string AffixCannotBeEmpty {
            get {
                return ResourceManager.GetString("AffixCannotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An affix string cannot be null..
        /// </summary>
        internal static string AffixCannotBeNull {
            get {
                return ResourceManager.GetString("AffixCannotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The strings in the affix array must be unique..
        /// </summary>
        internal static string AffixMustBeUnique {
            get {
                return ResourceManager.GetString("AffixMustBeUnique", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The same character cannot have multiple meanings within an alphabet..
        /// </summary>
        internal static string AlphabetConflict {
            get {
                return ResourceManager.GetString("AlphabetConflict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The alphabet must be exactaly {0} characters in length..
        /// </summary>
        internal static string AlphabetMustExactLength {
            get {
                return ResourceManager.GetString("AlphabetMustExactLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The alphabet must contain at least one element in the array..
        /// </summary>
        internal static string AlphabetMustHaveAtleastOneElement {
            get {
                return ResourceManager.GetString("AlphabetMustHaveAtleastOneElement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ordinal value of all characters in the alphabet must be 127 or less..
        /// </summary>
        internal static string AlphabetOrdinalMustBeByte {
            get {
                return ResourceManager.GetString("AlphabetOrdinalMustBeByte", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The codec was unable to make progress with a full buffer. This is the result of an internal error..
        /// </summary>
        internal static string CodecMadeNoProgress {
            get {
                return ResourceManager.GetString("CodecMadeNoProgress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data contains one or more characters that are not allowed or an invalid combination of characters..
        /// </summary>
        internal static string DecoderGenericInvalidCharacter {
            get {
                return ResourceManager.GetString("DecoderGenericInvalidCharacter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data contains an invalid escape sequence..
        /// </summary>
        internal static string DecoderHexDigitOrLineBreakExpected {
            get {
                return ResourceManager.GetString("DecoderHexDigitOrLineBreakExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data contains a character that is not part of the encoding alphabet and is not considered an ignorable character..
        /// </summary>
        internal static string DecoderInvalidCharacter {
            get {
                return ResourceManager.GetString("DecoderInvalidCharacter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data contains a padding character where a padding character is not allowed..
        /// </summary>
        internal static string DecoderInvalidPadding {
            get {
                return ResourceManager.GetString("DecoderInvalidPadding", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The encoded value is larger than the maximum possible value..
        /// </summary>
        internal static string DecoderOverflow {
            get {
                return ResourceManager.GetString("DecoderOverflow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data does not end with the correct amount of padding..
        /// </summary>
        internal static string DecoderPaddingRequired {
            get {
                return ResourceManager.GetString("DecoderPaddingRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A postfix is required, but the encoded data does not match any of the defined postfixes..
        /// </summary>
        internal static string DecoderPostfixRequired {
            get {
                return ResourceManager.GetString("DecoderPostfixRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A prefix is required, but the encoded data does not match any of the defined prefixes..
        /// </summary>
        internal static string DecoderPrefixRequired {
            get {
                return ResourceManager.GetString("DecoderPrefixRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The last quantum of input appears to have been truncated..
        /// </summary>
        internal static string DecoderTruncatedQuantum {
            get {
                return ResourceManager.GetString("DecoderTruncatedQuantum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The unused bits in the final quantum of data are not zero. The data may have been truncated..
        /// </summary>
        internal static string DecoderUnusedBitNonZero {
            get {
                return ResourceManager.GetString("DecoderUnusedBitNonZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The instance is read-only..
        /// </summary>
        internal static string InstanceIsReadOnly {
            get {
                return ResourceManager.GetString("InstanceIsReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection..
        /// </summary>
        internal static string InvalidIndexCountLength {
            get {
                return ResourceManager.GetString("InvalidIndexCountLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Offset and length were out of bounds for the array or count is greater than the number of elements from offset to the end of the source collection..
        /// </summary>
        internal static string InvalidOffsetCountLength {
            get {
                return ResourceManager.GetString("InvalidOffsetCountLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The incoming data would exceed the maximum length of the internal buffer..
        /// </summary>
        internal static string WouldExceedBuffer {
            get {
                return ResourceManager.GetString("WouldExceedBuffer", resourceCulture);
            }
        }
    }
}
