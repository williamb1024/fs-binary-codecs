using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsDecodingIgnoreOverflow
    {
        bool DecodingIgnoreOverflow { get; set; }
    }
}
