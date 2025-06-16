using System.Collections.Generic;
using Avalonia.Media;

namespace AvaloniaUIClient.Models
{
    public enum TextStyles
    {
        Title,
        Prefecture, 
        Scale,
        Name
    }

    public class DetailItemView
    {
        public string Text { get; init; } = "";
        public TextStyles TextStyle { get; init; }
        public string ScaleIconPath { get; init; } = "";
        public int ScaleInt { get; init; }

        public DetailItemView(string text, TextStyles textStyle, int scaleInt = -1)
        {
            Text = text;
            TextStyle = textStyle;
            ScaleInt = scaleInt;
            if (scaleInt > 0)
            {
                ScaleIconPath = GetScaleIconPath(scaleInt);
                // より詳細なデバッグ用：コンソールにログ出力
                System.Diagnostics.Debug.WriteLine($"DetailItemView: ScaleInt={scaleInt}, ScaleIconPath={ScaleIconPath}, IsEmpty={string.IsNullOrEmpty(ScaleIconPath)}");
                System.Console.WriteLine($"DetailItemView: ScaleInt={scaleInt}, ScaleIconPath={ScaleIconPath}, IsEmpty={string.IsNullOrEmpty(ScaleIconPath)}");
            }
        }

        private static string GetScaleIconPath(int scaleInt)
        {
            return scaleInt switch
            {
                10 => "avares://AvaloniaUIClient/Assets/Scales/scale_1.png",
                20 => "avares://AvaloniaUIClient/Assets/Scales/scale_2.png",
                30 => "avares://AvaloniaUIClient/Assets/Scales/scale_3.png",
                40 => "avares://AvaloniaUIClient/Assets/Scales/scale_4.png",
                45 => "avares://AvaloniaUIClient/Assets/Scales/scale_5l.png",
                46 => "avares://AvaloniaUIClient/Assets/Scales/scale_5l_estimated.png", // 5弱以上（推定）
                50 => "avares://AvaloniaUIClient/Assets/Scales/scale_5u.png",
                55 => "avares://AvaloniaUIClient/Assets/Scales/scale_6l.png",
                60 => "avares://AvaloniaUIClient/Assets/Scales/scale_6u.png",
                70 => "avares://AvaloniaUIClient/Assets/Scales/scale_7.png",
                _ => ""
            };
        }
    }

    public class UserquakeAreaDetail
    {
        public string AreaName { get; set; } = "";
        public int Count { get; set; }
        public double Confidence { get; set; }
        public string ConfidenceText { get; set; } = "";
        public IBrush brush { get; set; } = Brushes.Gray;
    }

    public class EEWDetail
    {
        public string Hypocenter { get; set; } = "";
        public string Areas { get; set; } = "";
        public string Caption { get; set; } = "";
    }
}