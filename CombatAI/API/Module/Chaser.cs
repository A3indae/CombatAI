using CombatAI.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatAI.API.Module
{
    public class Chaser : Core.BaseCombatAIModule
    {
        public Chaser(BaseCombatAI owner) : base(owner)
        {
            
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
