﻿using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal interface IFpcBotAction
    {
        void OnEnter();

        void UpdatePlayer(IFpcRole fpcRole);
    }
}
