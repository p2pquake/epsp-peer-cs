using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using System;
using System.IO;

namespace MapSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var fc = new FontCollection();
            var ff = fc.Install(new MemoryStream(Map.FontResource.RobotoMono_Regular));
            var ffb = fc.Install(new MemoryStream(Map.FontResource.RobotoMono_Bold));
            var fb = ffb.CreateFont(16, FontStyle.Bold);
            var f = ff.CreateFont(16);

            using var image = Image.Load(new MemoryStream(Map.ImageResource.japan_gsi_1024));
            using var description = Image.Load(new MemoryStream(Map.ImageResource.description));

            var rect = new Rectangle(450, 350, 180, 60);
            image.Mutate(x => x.Fill(Color.WhiteSmoke, rect));

            image.Mutate(x => x.DrawText("Test 0123456789", f, Color.Black, new PointF(450, 350)));
            image.Mutate(x => x.DrawText("Test 0123456789", fb, Color.Black, new PointF(450, 370)));

            image.Mutate(x => x.Crop(new Rectangle(440, 340, 400, 400)));

            image.Mutate(x => x.DrawImage(description, new Point(0, image.Height - description.Height), 1));

            image.Save(@"..\..\..\japan_processed.png");
        }
    }
}
