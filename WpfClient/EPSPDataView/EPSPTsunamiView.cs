using Client.Peer;

using Map.Controller;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WpfClient.Utils;

namespace WpfClient.EPSPDataView
{
    public class EPSPTsunamiView
    {
        public EPSPTsunamiEventArgs EventArgs { get; init; }
        public IFrameModel FrameModel { get; init; }

        public string Source
        {
            get
            {
                if (EventArgs.IsCancelled)
                {
                    return "/Resources/Icons/tsunami_gray.png";
                }

                return ThemeManager.Current.ActualApplicationTheme switch
                {
                    ApplicationTheme.Light => "/Resources/Icons/tsunami_red.png",
                    ApplicationTheme.Dark => "/Resources/Icons/tsunami_yellow.png",
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public string Time => EventArgs?.ReceivedAt.ToString("dd日HH時mm分");
        public string DetailTime => EventArgs?.ReceivedAt.ToString("dd日HH時mm分ss秒");

        public string Caption
        {
            get
            {
                if (EventArgs.IsCancelled)
                {
                    return "津波予報 解除";
                }

                return TsunamiCategoryConverter.String(MaxTsunamiCategory());
            }
        }

        private TsunamiCategory MaxTsunamiCategory()
        {
            if (EventArgs.IsCancelled) { return TsunamiCategory.Unknown; }

            var categories = EventArgs.RegionList.Select(e => e.Category).Distinct();
            if (categories.Contains(TsunamiCategory.MajorWarning)) { return TsunamiCategory.MajorWarning; }
            if (categories.Contains(TsunamiCategory.Warning)) { return TsunamiCategory.Warning; }
            if (categories.Contains(TsunamiCategory.Advisory)) { return TsunamiCategory.Advisory; }
            return TsunamiCategory.Unknown;
        }

        private Map.Model.TsunamiCategory GetCategory(TsunamiCategory category)
        {
            return category switch
            {
                TsunamiCategory.MajorWarning => Map.Model.TsunamiCategory.MajorWarning,
                TsunamiCategory.Warning => Map.Model.TsunamiCategory.Warning,
                TsunamiCategory.Advisory => Map.Model.TsunamiCategory.Advisory,
                _ => Map.Model.TsunamiCategory.Unknown,
            };
        }

        public string NoteFilename
        {
            get
            {
                if (EventArgs == null) { return null; }
                if (EventArgs.IsCancelled) { return null; }

                string category = MaxTsunamiCategory() switch
                {
                    TsunamiCategory.MajorWarning => "majorwarning",
                    TsunamiCategory.Warning => "warning",
                    TsunamiCategory.Advisory => "advisory",
                    _ => "majorwarning",
                };

                return $"/Resources/MapOverlays/tsunami_note_{category}.png";
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                if (EventArgs == null) { return null; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_1024,
                    Trim = !EventArgs.IsCancelled,
                    TsunamiPoints = EventArgs.RegionList.Select(e => new Map.Model.TsunamiPoint(e.Region, GetCategory(e.Category))).ToList(),
                    HideNote = true,
                    PreferedAspectRatio = FrameModel.FrameWidth / FrameModel.FrameHeight,
                };
                var png = mapDrawer.DrawAsPng();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = png;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public BitmapImage EmptyBitmapImage
        {
            get
            {
                if (EventArgs == null) { return null; }

                var mapDrawer = new MapDrawer()
                {
                    MapType = Map.Model.MapType.JAPAN_1024,
                    Trim = !EventArgs.IsCancelled,
                    TsunamiPoints = EventArgs.RegionList.Select(e => new Map.Model.TsunamiPoint(e.Region, GetCategory(e.Category))).ToList(),
                    HideNote = true,
                    HideDraw = true,
                    PreferedAspectRatio = FrameModel.FrameWidth / FrameModel.FrameHeight,
                };
                var png = mapDrawer.DrawAsPng();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = png;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public List<DetailItemView> DetailItemViewList
        {
            get
            {
                var list = new List<DetailItemView>();
                if (EventArgs is null)
                {
                    return list;
                }

                //list.Add(new DetailItemView($"津波予報 （受信日時： {DetailTime}）", TextStyles.Title));
                if (EventArgs.IsCancelled)
                {
                    list.Add(new DetailItemView("津波予報はすべて解除されました。", TextStyles.Name));
                    return list;
                }

                if (EventArgs.RegionList.Any(e => e.IsImmediately))
                {
                    list.Add(new DetailItemView("＊印の沿岸では、ただちに津波が来襲すると予想されます", TextStyles.Name));
                }

                var regionsByCategories = EventArgs.RegionList.OrderBy(e => e.Category).Reverse().GroupBy(e => e.Category);
                foreach (var regionsByCategory in regionsByCategories)
                {
                    list.Add(new DetailItemView("", regionsByCategory.Key switch {
                        TsunamiCategory.MajorWarning => TextStyles.MajorWarning,
                        TsunamiCategory.Warning => TextStyles.Warning,
                        TsunamiCategory.Advisory => TextStyles.Advisory,
                        _ => TextStyles.Name,
                    }));
                    list.Add(new DetailItemView(string.Join('、', regionsByCategory.Select(e => $"{(e.IsImmediately ? "＊" : "")}{e.Region}")), TextStyles.Name));
                }

                list.Add(new DetailItemView("", TextStyles.Name));
                list.Add(new DetailItemView("とるべき行動（気象庁リーフレット「津波防災」より）", TextStyles.Section));

                if (EventArgs.RegionList.Any(e => e.Category == TsunamiCategory.MajorWarning))
                {
                    list.Add(new DetailItemView($"{TsunamiCategoryConverter.String(TsunamiCategory.MajorWarning)} ・ {TsunamiCategoryConverter.String(TsunamiCategory.Warning)}", TextStyles.Section));
                    list.Add(new DetailItemView("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波は繰り返し襲ってくるので、大津波・津波警報が解除されるまで安全な場所から離れないでください。", TextStyles.Name));
                    list.Add(new DetailItemView("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TextStyles.Name));
                } else if (EventArgs.RegionList.Any(e => e.Category == TsunamiCategory.Warning))
                {
                    list.Add(new DetailItemView(TsunamiCategoryConverter.String(TsunamiCategory.Warning), TextStyles.Section));
                    list.Add(new DetailItemView("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波は繰り返し襲ってくるので、津波警報が解除されるまで安全な場所から離れないでください。", TextStyles.Name));
                    list.Add(new DetailItemView("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TextStyles.Name));
                }

                if (EventArgs.RegionList.Any(e => e.Category == TsunamiCategory.Advisory))
                {
                    list.Add(new DetailItemView(TsunamiCategoryConverter.String(TsunamiCategory.Advisory), TextStyles.Section));
                    list.Add(new DetailItemView("海の中にいる人は、ただちに海から上がって、海岸から離れてください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波注意報が解除されるまで海に入ったり海岸に近づいたりしないでください。", TextStyles.Name));
                }

                return list;
            }
        }
    }
}
