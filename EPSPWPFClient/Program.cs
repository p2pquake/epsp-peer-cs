﻿using EPSPWPFClient.Mediator;
using Reactive.Bindings;
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
                    mediator.HistoryViewModel.SetScheduler(UIDispatcherScheduler.Default);
                    mediator.HistoryViewModel.HistoryControl = ((MainWindow)app.MainWindow).HistoryControl;
                    ((MainWindow)app.MainWindow).HistoryViewModel = mediator.HistoryViewModel;
                    mediator.Start();
                }
            };
            app.SessionEnding += (s, e) =>
            {
                mediator.Stop();
            };

            app.InitializeComponent();
            app.Run();
        }
    }
}
