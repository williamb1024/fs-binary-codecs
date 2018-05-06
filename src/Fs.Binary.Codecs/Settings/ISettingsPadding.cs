using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsPadding
    {
        char? PaddingCharacter { get; set; }
        bool EncodingNoPadding { get; set; }
        bool DecodingIgnoreInvalidPadding { get; set; }
        bool DecodingRequirePadding { get; set; }
    }
}
