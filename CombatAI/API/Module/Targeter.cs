using CombatAI.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatAI.API.Module
{
    public class Targeter : Core.BaseCombatAIModule
    {
        float Range = 50f;
        bool HiddenDetection = false;

        public Targeter(BaseCombatAI owner) : base(owner)
        {

        }

        public override void Destroy()
        {
            
        }
    }
}
