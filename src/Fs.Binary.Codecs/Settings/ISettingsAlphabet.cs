using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsAlphabet
    {
        string[] Alphabets { get; set; }
        string DecodingIgnorableCharacters { get; set; }
        bool DecodingIgnoreInvalidCharacters { get; set; }
    }
}
