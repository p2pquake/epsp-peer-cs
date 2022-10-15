using Map.Model;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;

namespace CLI.Command
{
    public class MapCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "map",
                "地図を生成します"
                )
            {
                EEWSubCommand.Build(),
                QuakeSubCommand.Build(),
                TsunamiSubCommand.Build(),
                UserquakeSubCommand.Build(),
            };

            return command;
        }
    }
}
