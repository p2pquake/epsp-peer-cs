using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Map.Controller;
using System.Collections.Generic;
using Map.Model;

namespace Map
{
    /// <summary>
    /// 単体実行サンプル
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Console.WriteLine(UserquakeAreas.Instance.ContainsKey("250"));
            Console.WriteLine(UserquakeAreas.Instance.Get("250"));

            Console.WriteLine(Stations.Instance.GetArea("東京都２３区"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区", "東京都"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区西新宿"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区西新宿", "東京都"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区存在しない地名", "東京都"));

            Console.WriteLine($"--- {sw.ElapsedMilliseconds} ms");

            var drawer = new MapDrawer
            {
                Trim = true,
                MapType = Model.MapType.JAPAN_1024,
                Hypocenter = new Model.GeoCoordinate(36.1, 140),
                ObservationPoints = new List<ObservationPoint>
                {
                    new ObservationPoint("", "滋賀県北部", 10),
                    new ObservationPoint("", "滋賀県南部", 10),
                    new ObservationPoint("", "宮城県北部", 50),
                    new ObservationPoint("", "宮城県北部", 50),
                    new ObservationPoint("", "宮城県中部", 50),

                    new ObservationPoint("", "宮城県南部", 45),
                    new ObservationPoint("", "青森県三八上北", 45),
                    new ObservationPoint("", "岩手県沿岸南部", 45),
                    new ObservationPoint("", "岩手県内陸南部", 45),
                    new ObservationPoint("", "福島県中通り", 45),
                    new ObservationPoint("", "福島県浜通り", 45),

                    new ObservationPoint("", "岩手県沿岸北部", 40),
                    new ObservationPoint("", "岩手県内陸北部", 40),
                    new ObservationPoint("", "秋田県内陸南部", 40),
                    new ObservationPoint("", "山形県村山", 40),
                    new ObservationPoint("", "茨城県北部", 40),
                    new ObservationPoint("", "栃木県南部", 40),

                    new ObservationPoint("", "青森県津軽北部", 30),
                    new ObservationPoint("", "青森県下北", 30),
                    new ObservationPoint("", "福島県会津", 30),
                    new ObservationPoint("", "秋田県沿岸北部", 30),
                    new ObservationPoint("", "秋田県沿岸南部", 30),
                    new ObservationPoint("", "秋田県内陸北部", 30),
                    new ObservationPoint("", "山形県庄内", 30),
                    new ObservationPoint("", "山形県最上", 30),
                    new ObservationPoint("", "山形県置賜", 30),
                    new ObservationPoint("", "茨城県南部", 30),
                    new ObservationPoint("", "栃木県北部", 30),
                    new ObservationPoint("", "石狩地方北部", 30),
                    new ObservationPoint("", "渡島地方東部", 30),
                    new ObservationPoint("", "日高地方中部", 30),
                    new ObservationPoint("", "十勝地方中部", 30),
                    new ObservationPoint("", "十勝地方南部", 30),
                    new ObservationPoint("", "釧路地方中南部", 30),
                    new ObservationPoint("", "群馬県南部", 30),
                    new ObservationPoint("", "埼玉県北部", 30),
                    new ObservationPoint("", "埼玉県南部", 30),
                    new ObservationPoint("", "千葉県北東部", 30),
                    new ObservationPoint("", "千葉県北西部", 30),
                    new ObservationPoint("", "東京都２３区", 30),
                    new ObservationPoint("", "神奈川県東部", 30),
                    new ObservationPoint("", "新潟県中越", 30),
                    new ObservationPoint("", "新潟県下越", 30),
                    new ObservationPoint("", "山梨県東部・富士五湖", 30),
                    new ObservationPoint("千葉県", "大網白里市大網", 30),
                    new ObservationPoint("千葉県", "東金市東新宿", 20),
                    new ObservationPoint("茨城県", "茨城鹿嶋市鉢形", 10),
                    new ObservationPoint("神奈川県", "横浜鶴見区末広町", 10),
                    new ObservationPoint("東京都", "東京品川区平塚", 10),
                    new ObservationPoint("鹿児島県", "奄美市名瀬港町", 10),
                    new ObservationPoint("鹿児島県", "西之表市西之表", 10),
                    new ObservationPoint("沖縄県", "今帰仁村仲宗根", 10),
                }
            };

            Console.WriteLine($"--- init {sw.ElapsedMilliseconds} ms");

            using var png = drawer.DrawAsPng();
            
            Console.WriteLine($"--- draw {sw.ElapsedMilliseconds} ms");

            using var file = File.Open(@"..\..\..\output.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            png.CopyTo(file);
            file.Close();

            Console.WriteLine($"--- write {sw.ElapsedMilliseconds} ms");

            var r = new Random();
            var points = Map.PointResource.UserquakeAreas.Split('\n').Skip(1).Where((line) => line.Length > 0).Select((line) => line.Split(',')[0]).Where((areaCode) => areaCode.StartsWith("1")).Select((areaCode) => new UserquakePoint(areaCode, r.NextDouble() - 0.1)).ToList();
            var draw2 = new MapDrawer
            {
                MapType = Model.MapType.JAPAN_1024,
                UserquakePoints = points
            };
            using var png2 = draw2.DrawAsPng();
            using var file2 = File.Open(@"..\..\..\output2.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            png2.CopyTo(file2);
            file2.Close();

            Console.WriteLine($"--- write {sw.ElapsedMilliseconds} ms");

            var userquakeCsvs = new string[][]
            {
new string[]{  "2023/03/15 06:41:33.926","425" },
new string[]{  "2023/03/15 06:41:27.413","240" },
new string[]{  "2023/03/15 06:41:10.997","231" },
new string[]{  "2023/03/15 06:41:04.333","250" },
new string[]{  "2023/03/15 06:41:02.190","231" },
new string[]{  "2023/03/15 06:40:45.672","250" },
new string[]{  "2023/03/15 06:40:40.232","241" },
new string[]{  "2023/03/15 06:40:32.618","505" },
new string[]{  "2023/03/15 06:40:27.580","241" },
new string[]{  "2023/03/15 06:40:25.549","250" },
new string[]{  "2023/03/15 06:40:25.040","215" },
new string[]{  "2023/03/15 06:40:22.471","270" },
new string[]{  "2023/03/15 06:40:21.726","231" },
new string[]{  "2023/03/15 06:40:20.407","111" },
new string[]{  "2023/03/15 06:40:17.672","250" },
new string[]{  "2023/03/15 06:40:17.072","215" },
new string[]{  "2023/03/15 06:40:16.006","250" },
new string[]{  "2023/03/15 06:40:15.508","270" },
new string[]{  "2023/03/15 06:40:13.977","250" },
new string[]{  "2023/03/15 06:40:10.725","250" },
new string[]{  "2023/03/15 06:40:08.200","270" },
new string[]{  "2023/03/15 06:40:07.916","250" },
new string[]{  "2023/03/15 06:40:07.486","250" },
new string[]{  "2023/03/15 06:40:05.826","241" },
new string[]{  "2023/03/15 06:40:05.661","231" },
new string[]{  "2023/03/15 06:40:03.945","250" },
new string[]{  "2023/03/15 06:40:03.254","250" },
new string[]{  "2023/03/15 06:40:02.436","275" },
new string[]{  "2023/03/15 06:39:59.547","250" },
new string[]{  "2023/03/15 06:39:58.812","241" },
new string[]{  "2023/03/15 06:39:56.033","225" },
new string[]{  "2023/03/15 06:39:54.054","270" },
new string[]{  "2023/03/15 06:39:53.448","250" },
new string[]{  "2023/03/15 06:39:52.481","250" },
new string[]{  "2023/03/15 06:39:48.754","270" },
new string[]{  "2023/03/15 06:39:48.052","215" },
new string[]{  "2023/03/15 06:39:46.636","250" },
new string[]{  "2023/03/15 06:39:45.539","270" },
new string[]{  "2023/03/15 06:39:44.329","250" },
new string[]{  "2023/03/15 06:39:42.119","250" },
new string[]{  "2023/03/15 06:39:41.361","270" },
new string[]{  "2023/03/15 06:39:39.445","225" },
new string[]{  "2023/03/15 06:39:37.669","305" },
new string[]{  "2023/03/15 06:39:37.269","250" },
new string[]{  "2023/03/15 06:39:34.567","250" },
new string[]{  "2023/03/15 06:39:33.100","250" },
new string[]{  "2023/03/15 06:39:30.491","250" },
new string[]{  "2023/03/15 06:39:30.183","250" },
            };

            var userquakes = userquakeCsvs.Select(uq => new Userquake(DateTime.Parse(uq[0]), uq[1])).OrderBy(uq => uq.Timestamp).ToList();

            var draw3 = new MapDrawer
            {
                MapType = Model.MapType.JAPAN_1024,
                Userquakes = userquakes,
            };
            using var png3 = draw3.DrawAsPng();
            using var file3 = File.Open(@"..\..\..\output3.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            png3.CopyTo(file3);
            file3.Close();

            //var sw = new Stopwatch();
            //sw.Start();

            //var fc = new FontCollection();
            //var ff = fc.Install(new MemoryStream(Map.FontResource.RobotoMono_Regular));
            //var ffb = fc.Install(new MemoryStream(Map.FontResource.RobotoMono_Bold));
            //var fb = ffb.CreateFont(16, FontStyle.Bold);
            //var f = ff.CreateFont(16);

            //Console.WriteLine($"Font loaded: {sw.ElapsedMilliseconds} ms");

            //using var image = Image.Load(new MemoryStream(Map.ImageResource.japan_gsi_1024));
            //using var description = Image.Load(new MemoryStream(Map.ImageResource.description));

            //Console.WriteLine($"Image loaded: {sw.ElapsedMilliseconds} ms");

            //var rect = new Rectangle(450, 350, 180, 60);
            //image.Mutate(x => x.Fill(Color.WhiteSmoke, rect));

            //Console.WriteLine($"Image filled: {sw.ElapsedMilliseconds} ms");

            //image.Mutate(x => x.DrawText("Test 0123456789", f, Color.Black, new PointF(450, 350)));
            //image.Mutate(x => x.DrawText("Test 0123456789", fb, Color.Black, new PointF(450, 370)));

            //Console.WriteLine($"Image text drawed: {sw.ElapsedMilliseconds} ms");

            //image.Mutate(x => x.Crop(new Rectangle(440, 340, 400, 400)));
            //Console.WriteLine($"Image cropped: {sw.ElapsedMilliseconds} ms");

            //image.Mutate(x => x.DrawImage(description, new Point(0, image.Height - description.Height), 1));

            //Console.WriteLine($"Image mutated {sw.ElapsedMilliseconds} ms");

            //image.Save(@"..\..\..\japan_processed.png");
            //Console.WriteLine($"Image saved: {sw.ElapsedMilliseconds} ms");
        }
    }
}
