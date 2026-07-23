using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CombatAI.API
{
    public static class LayerMasks
    {
        public static LayerMask CharacterCollision { get; } = LayerMask.GetMask( "Default", "InvisibleCollider", "Door" );
        public static LayerMask Bullet { get; } = LayerMask.GetMask("Default", "InvisibleCollider", "Door");
    }
}
