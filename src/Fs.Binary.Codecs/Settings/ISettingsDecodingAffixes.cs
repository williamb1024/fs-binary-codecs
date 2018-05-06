using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Binary.Codecs.Settings
{
    internal interface ISettingsDecodingAffixes
    {
        string[] DecodingPrefixes { get; set; }
        string[] DecodingPostfixes { get; set; }
        bool DecodingPrefixRequired { get; set; }
        bool DecodingPostfixRequired { get; set; }
        int DecodingMinimumInputBuffer { get; }
    }
}
