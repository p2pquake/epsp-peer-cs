using Avalonia.Data.Converters;
using AvaloniaUIClient.Models;
using System;
using System.Globalization;

namespace AvaloniaUIClient.Converters
{
    public static class DetailItemViewTypeConverter
    {
        public static readonly FuncValueConverter<TextStyles, bool> IsTitle =
            new(style => style == TextStyles.Title);

        public static readonly FuncValueConverter<TextStyles, bool> IsPrefecture =
            new(style => style == TextStyles.Prefecture);

        public static readonly FuncValueConverter<TextStyles, bool> IsScale =
            new(style => style == TextStyles.Scale);

        public static readonly FuncValueConverter<TextStyles, bool> IsName =
            new(style => style == TextStyles.Name);
    }
}