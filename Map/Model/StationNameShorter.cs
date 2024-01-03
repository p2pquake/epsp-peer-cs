using System.Text.RegularExpressions;

namespace Map.Model
{
    public class StationNameShorter
    {
        public static readonly Regex ShortenPattern = new(@"^((?:余市町|田村市|玉村町|東村山市|武蔵村山市|羽村市|十日町市|上市町|大町市|名古屋中村区|大阪堺市.+?区|下市町|大村市|野々市市|四日市市|廿日市市|大町町|.+?[市区町村]))", RegexOptions.Compiled);
    }
}
