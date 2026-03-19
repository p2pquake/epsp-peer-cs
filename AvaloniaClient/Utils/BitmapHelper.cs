using Avalonia.Media.Imaging;

using System.IO;

namespace AvaloniaClient.Utils;

public static class BitmapHelper
{
    public static Bitmap FromStream(Stream pngStream)
    {
        pngStream.Position = 0;
        return new Bitmap(pngStream);
    }

    public static Bitmap? FromBytes(byte[]? data)
    {
        if (data == null) return null;
        using var ms = new MemoryStream(data);
        return new Bitmap(ms);
    }
}
