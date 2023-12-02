namespace LegacyPluginSupporter
{
    class Program
    {
        static int Main(string[] args)
        {
            var manager = new Manager("プラグイン接続テストツール");
            manager.OnNameNotified += (s, e) => { Console.WriteLine($"Name notified from {((Plugin)s).PluginName}"); };
            manager.OnExitRequest += (s, e) => { Console.WriteLine($"Exit request from {((Plugin)s).PluginName}"); };
            manager.OnUserquakeRequest += (s, e) => { Console.WriteLine($"Userquake request from {((Plugin)s).PluginName}"); };

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
