﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViewPresenter.MessageDispatcher
{
    public interface IController
    {
        int ID { get; }
        bool HandleRequest(Telegram telegram);
    }
}
