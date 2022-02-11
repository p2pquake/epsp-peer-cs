using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var updates = UpdateClient.CheckUpdateAsync().GetAwaiter().GetResult();
            if (updates.Length <= 0)
            {
                return;
            }

            App app = new();
            app.Run();
        }
    }
}
