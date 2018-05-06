using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsSafeCharacters
    {
        string SafeCharacters { get; set; }
        bool DecodingIgnoreInvalidCharacters { get; set; }
        string ExcludedCharacters { get; set; }
        string WhitespaceCharacters { get; set; }
        string EscapeCharacters { get; set; }
        string NewLineCharacters { get; set; }
    }
}
