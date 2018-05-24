using EPSPWPFClient.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            var mediator = new EPSPMediator();

            EPSPWPFClient.App app = new EPSPWPFClient.App();
            app.Activated += (s, e) => {
                if (app.MainWindow.DataContext == null)
                {
                    app.MainWindow.DataContext = mediator.StatusViewModel;
                    mediator.Start();
                }
            };
            app.InitializeComponent();
            app.Run();
        }
    }
}
