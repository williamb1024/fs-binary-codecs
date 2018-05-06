using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsTruncateFinalQuantum
    {
        bool EncodingTruncateFinalQuantum { get; set; }
        bool DecodingRequireCompleteFinalQuantum { get; set; }
    }
}
