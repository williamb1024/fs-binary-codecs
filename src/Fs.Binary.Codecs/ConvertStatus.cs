using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs
{
    /// <summary>
    /// Indicates the result of a Convert operation.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public struct ConvertStatus : IEquatable<ConvertStatus>
    {
        /// <summary>
        /// A <see cref="ConvertStatus"/> that has not been initialized.
        /// </summary>
        public static readonly ConvertStatus Unitialized = new ConvertStatus(ResultUninitialized);

        /// <summary>
        /// Additional input is required to continue the conversion.
        /// </summary>
        public static readonly ConvertStatus InputRequired = new ConvertStatus(ResultInputRequired);

        /// <summary>
        /// Additional output space is required to continue the conversion.
        /// </summary>
        public static readonly ConvertStatus OutputRequired = new ConvertStatus(ResultOutputRequired);

        /// <summary>
        /// The conversion operation is complete.
        /// </summary>
        public static readonly ConvertStatus Complete = new ConvertStatus(ResultComplete);

        private static readonly string[] Names = new string[]
        {
            "Uninitialized",
            "InputRequired",
            "OutputRequired",
            "Complete"
        };

        private const int ResultUninitialized = 0;
        private const int ResultInputRequired = 1;
        private const int ResultOutputRequired = 2;
        private const int ResultComplete = 3;

        private readonly int _result;

        private ConvertStatus ( int result )
        {
            _result = result;
        }

        /// <summary>
        /// Compares this <see cref="ConvertStatus"/> against the supplied object.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="ConvertStatus"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals ( object obj )
        {
            if (obj is ConvertStatus)
                return Equals((ConvertStatus)obj);
            else
                return false;
        }

        /// <summary>
        /// Compares this <see cref="ConvertStatus"/> against the supplied value.
        /// </summary>
        /// <param name="other">The <see cref="ConvertStatus"/> to be compared.</param>
        /// <returns><c>true</c> if this instance is equal to <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool Equals ( ConvertStatus other )
        {
            return (other._result == _result);
        }

        /// <summary>
        /// Generates a value that identifies this instance.
        /// </summary>
        /// <returns>A value that identifies this instance.</returns>
        public override int GetHashCode ()
        {
            return _result;
        }

        /// <summary>
        /// Gets a string representation of the instance.
        /// </summary>
        /// <returns>A string representation of the instance.</returns>
        public override string ToString ()
        {
            return Names[_result];
        }

        /// <summary>
        /// Compares two <see cref="ConvertStatus"/> instances.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>true</c>, if the left and right operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator == ( in ConvertStatus left, in ConvertStatus right )
        {
            return (left._result == right._result);
        }

        /// <summary>
        /// Compares two <see cref="ConvertStatus"/> instances.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>false</c>, if the left and right operands are equal; otherwise, <c>true</c>.</returns>
        public static bool operator != ( in ConvertStatus left, in ConvertStatus right )
        {
            return (left._result != right._result);
        }

        /// <summary>
        /// Converts a <see cref="ConvertStatus"/> to a <c>bool</c> value.
        /// </summary>
        /// <param name="operand">The <see cref="ConvertStatus"/> to convert.</param>
        public static implicit operator bool ( in ConvertStatus operand )
        {
            return (operand._result == ResultComplete);
        }
    }
}
