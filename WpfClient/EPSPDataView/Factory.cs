using Client.App.Userquake;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.EPSPDataView
{
    public static class Factory
    {
        public static object WrapEventArgs(EventArgs eventArgs, IFrameModel frameModel)
        {
            if (eventArgs is EPSPQuakeEventArgs quake)
            {
                return new EPSPQuakeView() { EventArgs = quake, FrameModel = frameModel };
            }

            if (eventArgs is EPSPTsunamiEventArgs tsunami)
            {
                return new EPSPTsunamiView() { EventArgs = tsunami };
            }

            if (eventArgs is EPSPEEWTestEventArgs eew)
            {
                return new EPSPEEWTestView() { EventArgs = eew };
            }

            if (eventArgs is UserquakeEvaluateEventArgs userquake)
            {
                return new EPSPUserquakeView() { EventArgs = userquake, FrameModel = frameModel };
            }

            return null;
        }
    }
}
