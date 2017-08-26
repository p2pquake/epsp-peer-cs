using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Client.General;

namespace Client.Client.State
{
    interface IFinishedState
    {
        ClientConst.OperationResult Result { get; }
        ClientConst.ErrorCode ErrorCode { get; }
    }
}
