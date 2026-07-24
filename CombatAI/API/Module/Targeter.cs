using CombatAI.API.Core;
using CustomPlayerEffects;
using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CombatAI.API.Module
{
    public class Targeter : Core.BaseCombatAIModule
    {
        private bool hasInfRange => SearchRange >= 1000;
        public float SearchRange = 10000f;
        public float AttackRange = 50f;
        public bool HiddenDetection = false;
        public LayerMask RaycastLayerMask = LayerMasks.Bullet;
        public HashSet<RoleTypeId> HostileRoles = new HashSet<RoleTypeId> { RoleTypeId.NtfSpecialist };

        public Targeter(BaseCombatAI owner) : base(owner)
        {

        }

        public (Player target, bool canAttack) FindTarget()
        {
            if (Owner.Npc == null) return (null, false);

            Player target = null;
            bool canAttack = false;
            float bestDistance = float.MaxValue;

            foreach (Player player in Player.List)
            {
                if (!HostileRoles.Contains(player.Role.Type) || player.IsNPC) continue;
                if (!HiddenDetection && player.IsEffectActive<Invisible>()) continue;

                Vector3 direction = (player.Position - Owner.Npc.Position);
                float distance = direction.magnitude;

                if (!hasInfRange)
                {
                    if (distance == 0 || distance > SearchRange) continue;
                }

                bool canAttackThis = (distance <= AttackRange && !Physics.Raycast(Owner.Npc.Position, direction.normalized, out var _, distance, RaycastLayerMask));

                if (canAttack && !canAttackThis) continue;
                if (!canAttack && canAttackThis)
                {
                    target = player;
                    bestDistance = distance;
                    canAttack = true;
                    continue;
                }

                if (bestDistance > distance)
                {
                    target = player;
                    bestDistance = distance;
                }
            }

            return (target, canAttack);
        }

        public override void Destroy()
        {
            
        }
    }
}
