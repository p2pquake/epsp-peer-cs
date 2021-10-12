using Client.Peer;

using ModernWpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfClient.EPSPDataView
{
    public class EPSPTsunamiView
    {
        public EPSPTsunamiEventArgs EventArgs { get; init; }

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

        public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");
        public string DetailTime => EventArgs.ReceivedAt.ToString("dd日HH時mm分ss秒");

        public string Caption
        {
            get
            {
                if (EventArgs.IsCancelled)
                {
                    return "津波予報 解除";
                }

                return TsunamiCategoryString(MaxTsunamiCategory());
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
        public List<DetailItemView> DetailItemViewList
        {
            get
            {
                var list = new List<DetailItemView>();
                if (EventArgs is null)
                {
                    return list;
                }

                list.Add(new DetailItemView($"津波予報 （受信日時： {DetailTime}）", TextStyles.Title));
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
                    list.Add(new DetailItemView($"{TsunamiCategoryString(TsunamiCategory.MajorWarning)} ・ {TsunamiCategoryString(TsunamiCategory.Warning)}", TextStyles.Section));
                    list.Add(new DetailItemView("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波は繰り返し襲ってくるので、大津波・津波警報が解除されるまで安全な場所から離れないでください。", TextStyles.Name));
                    list.Add(new DetailItemView("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TextStyles.Name));
                } else if (EventArgs.RegionList.Any(e => e.Category == TsunamiCategory.Warning))
                {
                    list.Add(new DetailItemView(TsunamiCategoryString(TsunamiCategory.Warning), TextStyles.Section));
                    list.Add(new DetailItemView("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波は繰り返し襲ってくるので、津波警報が解除されるまで安全な場所から離れないでください。", TextStyles.Name));
                    list.Add(new DetailItemView("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TextStyles.Name));
                }

                if (EventArgs.RegionList.Any(e => e.Category == TsunamiCategory.Advisory))
                {
                    list.Add(new DetailItemView(TsunamiCategoryString(TsunamiCategory.Advisory), TextStyles.Section));
                    list.Add(new DetailItemView("海の中にいる人は、ただちに海から上がって、海岸から離れてください。", TextStyles.Name));
                    list.Add(new DetailItemView("津波注意報が解除されるまで海に入ったり海岸に近づいたりしないでください。", TextStyles.Name));
                }

                return list;
            }
        }

        private static string TsunamiCategoryString(TsunamiCategory category) => category switch
        {
            TsunamiCategory.MajorWarning => "大津波警報",
            TsunamiCategory.Warning => "津波警報",
            TsunamiCategory.Advisory => "津波注意報",
            _ => "津波予報"
        };
    }
}
