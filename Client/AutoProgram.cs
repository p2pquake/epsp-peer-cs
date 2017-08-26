using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using Client.App;
using Client.Common.General;

namespace Client
{
    class AutoProgram
    {
        static void Main(string[] args)
        {
            Logger.GetLog().Info("Client.EPSP030compatible " + Const.SOFTWARE_NAME + "/" + Const.SOFTWARE_VERSION);

#if !DEBUG
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(AutoProgram_UnhandledException);

            try
            {
#endif
                Logger.GetLog().Info("スタートします。");

                IOperatable operatableContext = new MediatorContext();
                operatableContext.Connect();

                while (true)
                {
                    Thread.Sleep(1000);
                }
#if !DEBUG
            }
            catch (Exception e)
            {
                Logger.GetLog().Fatal("実行中に例外が発生しました。", e);
                Environment.Exit(1);
                // throw new Exception("エラーにつき終了します。", e);
            }
#endif
        }

        static void AutoProgram_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.GetLog().Fatal("実行中に例外が発生しました。");
            Logger.GetLog().Fatal("実行中に例外が発生しました。", (Exception)e.ExceptionObject);
            Environment.Exit(1);
            // throw new Exception("エラーにつき終了します。", (Exception)e.ExceptionObject);
        }
    }
}
