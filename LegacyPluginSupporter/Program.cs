namespace LegacyPluginSupporter
{
    class Program
    {
        static int Main(string[] args)
        {
            var manager = new Manager("プラグイン接続テストツール");
            manager.Listen();
            while (true)
            {
                Console.ReadLine();
                foreach (var plugin in manager.PluginList)
                {
                    Console.WriteLine($"プラグイン: {plugin.PluginName} ({plugin.GetHashCode()})");
                    manager.RequestOption(plugin);
                }
                Console.WriteLine("----");
            }
        }
    }
}
