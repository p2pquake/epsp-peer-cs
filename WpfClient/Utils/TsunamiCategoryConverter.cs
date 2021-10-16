using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Utils
{
    public static class TsunamiCategoryConverter
    {
        public static string String(TsunamiCategory category) => category switch
        {
            TsunamiCategory.MajorWarning => "大津波警報",
            TsunamiCategory.Warning => "津波警報",
            TsunamiCategory.Advisory => "津波注意報",
            _ => "津波予報"
        };
    }
}
