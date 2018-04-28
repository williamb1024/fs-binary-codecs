using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Binary.Codecs.Common;

namespace Fs.Binary.Codecs.QuotedPrintable
{
    public class QuotedPrintableSettings: AlphabetBuilder
    {
        private static readonly string DefaultAlphabet = "!\"#$%&'()*+,-./0123456789:;<>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public QuotedPrintableSettings ()
            : base(DefaultAlphabet)
        {
        }

        public QuotedPrintableSettings ( QuotedPrintableSettings inheritedSettings, bool isProtected )
            : base(inheritedSettings, isProtected)
        {
        }

        public QuotedPrintableSettings ToReadOnly () => new QuotedPrintableSettings(this, true);
    }
}
