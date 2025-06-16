using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace AvaloniaUIClient.Converters
{
    public class StringToBitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    var uri = new Uri(path);
                    return new Bitmap(AssetLoader.Open(uri));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"StringToBitmapConverter failed to load: {path}, Error: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}