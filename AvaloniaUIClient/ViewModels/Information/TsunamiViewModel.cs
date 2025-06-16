using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Peer;
using Map.Controller;

namespace AvaloniaUIClient.ViewModels.Information
{
    public partial class TsunamiViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Bitmap? mapImage;

        [ObservableProperty]
        private Bitmap? emptyMapImage;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private string noteImagePath = string.Empty;

        public EPSPTsunamiEventArgs? EventArgs { get; private set; }
        
        [ObservableProperty]
        private double frameWidth = 800;
        
        [ObservableProperty]
        private double frameHeight = 600;

        public string Time => EventArgs?.ReceivedAt.ToString("dd日HH時mm分") ?? string.Empty;
        public string DetailTime => EventArgs?.ReceivedAt.ToString("dd日HH時mm分ss秒") ?? string.Empty;

        public string Caption
        {
            get
            {
                if (EventArgs?.IsCancelled == true)
                {
                    return "津波予報 解除";
                }
                return TsunamiCategoryToString(MaxTsunamiCategory());
            }
        }

        public bool IsTestVisible => EventArgs?.RegionList?.Any(region => region.Region == "テスト予報区") ?? false;

        public ObservableCollection<TsunamiDetailItem> DetailItems { get; } = new();

        public void Initialize(EPSPTsunamiEventArgs eventArgs)
        {
            EventArgs = eventArgs;
            System.Diagnostics.Debug.WriteLine($"TsunamiViewModel初期化:");
            System.Diagnostics.Debug.WriteLine($"  IsCancelled: {eventArgs.IsCancelled}");
            System.Diagnostics.Debug.WriteLine($"  ReceivedAt: {eventArgs.ReceivedAt}");
            System.Diagnostics.Debug.WriteLine($"  RegionList count: {eventArgs.RegionList?.Count ?? 0}");
            
            if (eventArgs.RegionList != null)
            {
                foreach (var region in eventArgs.RegionList)
                {
                    System.Diagnostics.Debug.WriteLine($"    Region: {region.Region}, Category: {region.Category}, IsImmediately: {region.IsImmediately}");
                }
            }
            
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(DetailTime));
            OnPropertyChanged(nameof(Caption));
            OnPropertyChanged(nameof(IsTestVisible));
            
            SetNoteImagePath();
            GenerateDetailItems();
            _ = GenerateMapImageAsync();
        }

        public void UpdateFrameSize(double width, double height)
        {
            if (Math.Abs(FrameWidth - width) > 1 || Math.Abs(FrameHeight - height) > 1)
            {
                FrameWidth = width;
                FrameHeight = height;
                System.Diagnostics.Debug.WriteLine($"津波フレームサイズ更新: {width}x{height}");
                
                // フレームサイズが変更されたら地図を再生成
                if (EventArgs != null)
                {
                    IsLoading = true;
                    _ = GenerateMapImageAsync();
                }
            }
        }

        private TsunamiCategory MaxTsunamiCategory()
        {
            if (EventArgs?.IsCancelled == true) { return TsunamiCategory.Unknown; }
            if (EventArgs?.RegionList == null) { return TsunamiCategory.Unknown; }

            var categories = EventArgs.RegionList.Select(e => e.Category).Distinct();
            if (categories.Contains(TsunamiCategory.MajorWarning)) { return TsunamiCategory.MajorWarning; }
            if (categories.Contains(TsunamiCategory.Warning)) { return TsunamiCategory.Warning; }
            if (categories.Contains(TsunamiCategory.Advisory)) { return TsunamiCategory.Advisory; }
            return TsunamiCategory.Unknown;
        }

        private void SetNoteImagePath()
        {
            if (EventArgs?.IsCancelled == true)
            {
                NoteImagePath = string.Empty;
                return;
            }

            string category = MaxTsunamiCategory() switch
            {
                TsunamiCategory.MajorWarning => "majorwarning",
                TsunamiCategory.Warning => "warning",
                TsunamiCategory.Advisory => "advisory",
                _ => "majorwarning",
            };

            NoteImagePath = $"avares://AvaloniaUIClient/Assets/tsunami_note_{category}.png";
        }

        private async Task GenerateMapImageAsync()
        {
            if (EventArgs == null) 
            {
                System.Diagnostics.Debug.WriteLine("津波地図生成: EventArgs is null");
                IsLoading = false;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"津波地図生成開始: RegionList count = {EventArgs.RegionList?.Count ?? 0}");

            try
            {
                await Task.Run(() =>
                {
                    var tsunamiPoints = EventArgs.RegionList?.Select(e => new Map.Model.TsunamiPoint(
                        e.Region, 
                        ConvertToMapTsunamiCategory(e.Category)
                    )).ToList() ?? new List<Map.Model.TsunamiPoint>();

                    System.Diagnostics.Debug.WriteLine($"津波ポイント数: {tsunamiPoints.Count}");
                    foreach (var point in tsunamiPoints)
                    {
                        System.Diagnostics.Debug.WriteLine($"  津波ポイント: {point.Name}, カテゴリ: {point.Category}");
                    }
                    
                    // TsunamiAreasの初期化状況を確認
                    try
                    {
                        var areas = Map.Model.TsunamiAreas.Instance;
                        System.Diagnostics.Debug.WriteLine($"TsunamiAreas読み込み状況: 初期化済み");
                        
                        // 実際に津波ポイントが地図に描画可能か確認
                        foreach (var point in tsunamiPoints)
                        {
                            var area = areas?.GetArea(point.Name);
                            System.Diagnostics.Debug.WriteLine($"  地域'{point.Name}': {(area != null ? "見つかった" : "見つからない")}");
                            if (area != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"    地域データあり");
                                // 座標データを詳細チェック
                                if (area.Coordinates != null && area.Coordinates.Any())
                                {
                                    System.Diagnostics.Debug.WriteLine($"    座標グループ数: {area.Coordinates.Count()}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"    警告: 座標データが空");
                                }
                            }
                        }
                        
                        // 利用可能な地域名リストを一部表示（デバッグ用）
                        try 
                        {
                            var availableAreas = areas?.GetType().GetProperty("Areas")?.GetValue(areas);
                            if (availableAreas != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"    利用可能地域数確認完了");
                            }
                        }
                        catch (Exception listEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"    地域リスト取得エラー: {listEx.Message}");
                        }
                    }
                    catch (Exception areaEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"TsunamiAreas読み込みエラー: {areaEx.Message}");
                    }

                    double aspectRatio = FrameWidth > 0 && FrameHeight > 0 ? FrameWidth / FrameHeight : 4.0 / 3.0;
                    System.Diagnostics.Debug.WriteLine($"使用するアスペクト比: {aspectRatio} (FrameSize: {FrameWidth}x{FrameHeight})");

                    // 背景地図の生成
                    var emptyMapDrawer = new MapDrawer()
                    {
                        MapType = Map.Model.MapType.JAPAN_1024,
                        Trim = !EventArgs.IsCancelled,
                        TsunamiPoints = new List<Map.Model.TsunamiPoint>(), // 背景地図は津波情報なし
                        HideNote = true,
                        HideDraw = true,
                        PreferedAspectRatio = aspectRatio,
                    };

                    System.Diagnostics.Debug.WriteLine("背景地図を生成中...");
                    using var emptyPngStream = emptyMapDrawer.DrawAsPng();
                    if (emptyPngStream != null && emptyPngStream.Length > 0)
                    {
                        EmptyMapImage = new Bitmap(emptyPngStream);
                        System.Diagnostics.Debug.WriteLine($"背景地図生成完了: {emptyPngStream.Length} bytes");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("背景地図生成失敗: ストリームが空");
                    }

                    // 津波情報付き地図の生成
                    var mapDrawer = new MapDrawer()
                    {
                        MapType = Map.Model.MapType.JAPAN_1024,
                        Trim = !EventArgs.IsCancelled,
                        TsunamiPoints = tsunamiPoints,
                        HideNote = true,
                        PreferedAspectRatio = aspectRatio,
                    };

                    System.Diagnostics.Debug.WriteLine("津波地図を生成中...");
                    try
                    {
                        using var pngStream = mapDrawer.DrawAsPng();
                        if (pngStream != null && pngStream.Length > 0)
                        {
                            MapImage = new Bitmap(pngStream);
                            System.Diagnostics.Debug.WriteLine($"津波地図生成完了: {pngStream.Length} bytes");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("津波地図生成失敗: ストリームが空");
                        }
                    }
                    catch (Exception mapEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"津波地図描画エラー: {mapEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"スタックトレース: {mapEx.StackTrace}");
                        
                        // フォールバック: 津波ポイントなしで地図を生成
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("フォールバック: 津波ポイントなしで地図生成を試行");
                            var fallbackMapDrawer = new MapDrawer()
                            {
                                MapType = Map.Model.MapType.JAPAN_1024,
                                Trim = false, // フォールバックでは全体表示
                                TsunamiPoints = new List<Map.Model.TsunamiPoint>(), // 空リスト
                                HideNote = true,
                                PreferedAspectRatio = aspectRatio,
                            };
                            
                            using var fallbackStream = fallbackMapDrawer.DrawAsPng();
                            if (fallbackStream != null && fallbackStream.Length > 0)
                            {
                                MapImage = new Bitmap(fallbackStream);
                                System.Diagnostics.Debug.WriteLine($"フォールバック地図生成成功: {fallbackStream.Length} bytes");
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"フォールバック地図生成も失敗: {fallbackEx.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"津波地図生成エラー: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"スタックトレース: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("津波地図生成終了");
            }
        }

        private Map.Model.TsunamiCategory ConvertToMapTsunamiCategory(TsunamiCategory category)
        {
            return category switch
            {
                TsunamiCategory.MajorWarning => Map.Model.TsunamiCategory.MajorWarning,
                TsunamiCategory.Warning => Map.Model.TsunamiCategory.Warning,
                TsunamiCategory.Advisory => Map.Model.TsunamiCategory.Advisory,
                _ => Map.Model.TsunamiCategory.Unknown,
            };
        }

        private void GenerateDetailItems()
        {
            DetailItems.Clear();
            
            if (EventArgs == null) return;

            if (EventArgs.IsCancelled)
            {
                DetailItems.Add(new TsunamiDetailItem("津波予報はすべて解除されました。", TsunamiDetailItemType.Name));
                return;
            }

            if (EventArgs.RegionList?.Any(e => e.IsImmediately) == true)
            {
                DetailItems.Add(new TsunamiDetailItem("＊印の沿岸では、ただちに津波が来襲すると予想されます", TsunamiDetailItemType.Name));
            }

            var regionsByCategories = EventArgs.RegionList?
                .OrderByDescending(e => e.Category)
                .GroupBy(e => e.Category);

            if (regionsByCategories != null)
            {
                foreach (var regionsByCategory in regionsByCategories)
                {
                    var itemType = regionsByCategory.Key switch
                    {
                        TsunamiCategory.MajorWarning => TsunamiDetailItemType.MajorWarning,
                        TsunamiCategory.Warning => TsunamiDetailItemType.Warning,
                        TsunamiCategory.Advisory => TsunamiDetailItemType.Advisory,
                        _ => TsunamiDetailItemType.Name,
                    };

                    DetailItems.Add(new TsunamiDetailItem("", itemType));
                    DetailItems.Add(new TsunamiDetailItem(
                        string.Join("\n", regionsByCategory.Select(e => $"{(e.IsImmediately ? "＊" : "")}{e.Region}")), 
                        TsunamiDetailItemType.Name
                    ));
                }
            }

            DetailItems.Add(new TsunamiDetailItem("", TsunamiDetailItemType.Name));
            DetailItems.Add(new TsunamiDetailItem("とるべき行動（気象庁リーフレット「津波防災」より）", TsunamiDetailItemType.Section));

            if (EventArgs.RegionList?.Any(e => e.Category == TsunamiCategory.MajorWarning) == true)
            {
                DetailItems.Add(new TsunamiDetailItem($"{TsunamiCategoryToString(TsunamiCategory.MajorWarning)} ・ {TsunamiCategoryToString(TsunamiCategory.Warning)}", TsunamiDetailItemType.Section));
                DetailItems.Add(new TsunamiDetailItem("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TsunamiDetailItemType.Name));
                DetailItems.Add(new TsunamiDetailItem("津波は繰り返し襲ってくるので、大津波・津波警報が解除されるまで安全な場所から離れないでください。", TsunamiDetailItemType.Name));
                DetailItems.Add(new TsunamiDetailItem("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TsunamiDetailItemType.Name));
            }
            else if (EventArgs.RegionList?.Any(e => e.Category == TsunamiCategory.Warning) == true)
            {
                DetailItems.Add(new TsunamiDetailItem(TsunamiCategoryToString(TsunamiCategory.Warning), TsunamiDetailItemType.Section));
                DetailItems.Add(new TsunamiDetailItem("沿岸部や川沿いにいる人は、ただちに高台や避難ビルなど安全な場所へ避難してください。", TsunamiDetailItemType.Name));
                DetailItems.Add(new TsunamiDetailItem("津波は繰り返し襲ってくるので、津波警報が解除されるまで安全な場所から離れないでください。", TsunamiDetailItemType.Name));
                DetailItems.Add(new TsunamiDetailItem("＜ここなら安心と思わず、より高い場所を目指して避難しましょう！＞", TsunamiDetailItemType.Name));
            }

            if (EventArgs.RegionList?.Any(e => e.Category == TsunamiCategory.Advisory) == true)
            {
                DetailItems.Add(new TsunamiDetailItem(TsunamiCategoryToString(TsunamiCategory.Advisory), TsunamiDetailItemType.Section));
                DetailItems.Add(new TsunamiDetailItem("海の中にいる人は、ただちに海から上がって、海岸から離れてください。", TsunamiDetailItemType.Name));
                DetailItems.Add(new TsunamiDetailItem("津波注意報が解除されるまで海に入ったり海岸に近づいたりしないでください。", TsunamiDetailItemType.Name));
            }
        }

        private static string TsunamiCategoryToString(TsunamiCategory category) => category switch
        {
            TsunamiCategory.MajorWarning => "大津波警報",
            TsunamiCategory.Warning => "津波警報",
            TsunamiCategory.Advisory => "津波注意報",
            _ => "津波予報"
        };
    }

    public class TsunamiDetailItem
    {
        public string Text { get; }
        public TsunamiDetailItemType Type { get; }

        public TsunamiDetailItem(string text, TsunamiDetailItemType type)
        {
            Text = text;
            Type = type;
        }
    }

    public enum TsunamiDetailItemType
    {
        Header,
        Name,
        Section,
        MajorWarning,
        Warning,
        Advisory
    }

    public static class TsunamiDetailItemTypeConverter
    {
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsHeader =
            new(type => type == TsunamiDetailItemType.Header);
        
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsName =
            new(type => type == TsunamiDetailItemType.Name);
        
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsSection =
            new(type => type == TsunamiDetailItemType.Section);
        
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsMajorWarning =
            new(type => type == TsunamiDetailItemType.MajorWarning);
        
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsWarning =
            new(type => type == TsunamiDetailItemType.Warning);
        
        public static readonly FuncValueConverter<TsunamiDetailItemType, bool> IsAdvisory =
            new(type => type == TsunamiDetailItemType.Advisory);
    }
}