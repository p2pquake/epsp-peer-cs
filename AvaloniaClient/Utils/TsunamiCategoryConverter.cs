using Client.Peer;

namespace AvaloniaClient.Utils;

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
