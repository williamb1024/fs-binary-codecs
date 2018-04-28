using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Binary.Codecs
{
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public struct ConvertStatus : IEquatable<ConvertStatus>
    {
        public static readonly ConvertStatus Unitialized = new ConvertStatus(ResultUninitialized);
        public static readonly ConvertStatus InputRequired = new ConvertStatus(ResultInputRequired);
        public static readonly ConvertStatus OutputRequired = new ConvertStatus(ResultOutputRequired);
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

        public override bool Equals ( object obj )
        {
            if (obj is ConvertStatus)
                return Equals((ConvertStatus)obj);
            else
                return false;
        }

        public bool Equals ( ConvertStatus other )
        {
            return (other._result == _result);
        }

        public override int GetHashCode ()
        {
            return _result;
        }

        public override string ToString ()
        {
            return Names[_result];
        }

        public static bool operator == ( in ConvertStatus left, in ConvertStatus right )
        {
            return (left._result == right._result);
        }

        public static bool operator != ( in ConvertStatus left, in ConvertStatus right )
        {
            return (left._result != right._result);
        }

        public static implicit operator bool ( in ConvertStatus operand )
        {
            return (operand._result == ResultComplete);
        }
    }
}
