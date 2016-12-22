using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.App.State
{
    /// <summary>
    /// 切断途中の状態
    /// </summary>
    class DisconnectingState : AbstractState
    {
        internal override bool CanConnect
        {
            get { return false; }
        }

        internal override bool CanDisconnect
        {
            get { return false; }
        }

        internal override bool CanMaintain
        {
            get { return false; }
        }
    }
}
