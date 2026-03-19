using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using System;
using System.Globalization;

namespace AvaloniaClient.Utils;

public class BitmapAssetConverter : IValueConverter
{
    public static readonly BitmapAssetConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && path.StartsWith("avares://"))
        {
            return new Bitmap(AssetLoader.Open(new Uri(path)));
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
