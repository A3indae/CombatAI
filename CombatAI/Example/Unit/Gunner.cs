
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

namespace CombatAI.Example.Unit
{
    public class Gunner : API.Core.BaseCombatAI
    {
        public override RoleTypeId RoleType => RoleTypeId.ClassD;

        public override string Name => "허접AI";

        public Gunner(Vector3 spawnPosition) : base(spawnPosition)
        {
            TryAddModule<API.Module.Chaser>();
            TryAddModule<API.Module.Acter>();
            TryAddModule<API.Module.Targeter>();
        }

        protected override void OnDead(DamageHandlerBase handler)
        {
            
        }
    }
}
