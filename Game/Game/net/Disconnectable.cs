using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.net
{
    public interface Disconnectable
    {
        void Disconnect();
        bool IsConnected();
    }
}
