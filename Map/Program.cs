using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using System;
using System.IO;
using System.Diagnostics;
using Map.Controller;
using System.Collections.Generic;
using Map.Model;

namespace Map
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(UserquakeAreas.Instance.ContainsKey("250"));
            Console.WriteLine(UserquakeAreas.Instance.Get("250"));

            Console.WriteLine(Stations.Instance.GetArea("東京都２３区"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区", "東京都"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区西新宿"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区西新宿", "東京都"));
            Console.WriteLine(Stations.Instance.GetPoint("東京新宿区存在しない地名", "東京都"));

            var drawer = new MapDrawer
            {
                MapType = Model.MapType.JAPAN_1024,
                Hypocenter = new Model.GeoCoordinate(36.1, 140),
                ObservationPoints = new List<ObservationPoint>
                {
                    new ObservationPoint("千葉県", "大網白里市大網", 30),
                    new ObservationPoint("千葉県", "東金市東新宿", 20),
                    new ObservationPoint("茨城県", "茨城鹿嶋市鉢形", 10),
                    new ObservationPoint("神奈川県", "横浜鶴見区末広町", 10),
                    new ObservationPoint("東京都", "東京品川区平塚", 10),
                }
            };

            using var png = drawer.DrawAsPng();
            using var file = File.Open(@"..\..\..\output.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            png.CopyTo(file);
            file.Close();

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
