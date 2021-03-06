﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Network.Messages
{
    /*
    An abstract message which could be client originated or server originated
    */
    abstract class AbstractMessage
    {
        public abstract override String ToString();
    }
}
