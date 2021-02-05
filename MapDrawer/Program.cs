using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Map.Map;
using Map.Util;
using System.IO;
using System.CommandLine;
using MapDrawer.Cmd;

namespace Map
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                 Quake.Generate(),
                 Userquake.Generate()
            };

            rootCommand.Invoke(args);
        }
    }
}
