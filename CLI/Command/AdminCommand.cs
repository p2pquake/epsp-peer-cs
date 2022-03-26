using CLI.Command.Admin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLI.Command
{
    public class AdminCommand
    {
        public static System.CommandLine.Command Build()
        {
            var command = new System.CommandLine.Command(
                "admin",
                "管理用コマンド"
                )
            {
                ProtocolTimeSubCommand.Build(),
            };

            return command;
        }
    }
}
