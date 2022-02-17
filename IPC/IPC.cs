using System;

namespace IPC
{
    public static class Const
    {
        public const string Name = "p2pquake-ipc";
    }

    public record Message(Method Method);

    public enum Method
    {
        Show,
        Exit,
    }
}
