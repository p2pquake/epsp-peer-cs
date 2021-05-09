using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using System;
using System.IO;
using System.Diagnostics;
using Map.Controller;
using System.Collections.Generic;

namespace Map
{
    class Program
    {
        static void Main(string[] args)
        {
            var drawer = new MapDrawer
            {
                MapType = Model.MapType.JAPAN_1024
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
