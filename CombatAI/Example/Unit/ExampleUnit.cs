
using CombatAI.API.Module;
using Exiled.API.Features;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using System.Collections.Generic;
using UnityEngine;

namespace CombatAI.Example.Unit
{
    public class ExampleUnit : API.Core.BaseCombatAI
    {
        public override RoleTypeId RoleType => RoleTypeId.ClassD;

        public override string Name => "허접AI";

        private CoroutineHandle unitLoop;

        private Chaser chaser;
        private Acter acter;
        private Targeter targeter;
        private Looker looker;

        private MagazineModule magModule;

        public ExampleUnit(Vector3 spawnPosition) : base(spawnPosition)
        {
            unitLoop = Timing.RunCoroutine(UnitCoroutine());
        }

        protected override void OnDead(DamageHandlerBase handler)
        {
            Timing.KillCoroutines(unitLoop);
            API.CombatAIHandler.RemoveById(Npc.Id);
        }

        public IEnumerator<float> UnitCoroutine()
        {
            while (!(Npc.RoleManager.CurrentRole is PlayerRoles.FirstPersonControl.IFpcRole))
                yield return Timing.WaitForOneFrame;

            chaser = TryAddModule<API.Module.Chaser>();
            acter = TryAddModule<API.Module.Acter>();
            targeter = TryAddModule<API.Module.Targeter>();
            looker = TryAddModule<API.Module.Looker>();

            ItemBase item = acter.TryEquipItem(ItemType.GunCOM18, false);
            Firearm firearm = item as Firearm;
            firearm.TryGetModule<MagazineModule>(out magModule);
            chaser.StartChase();
            looker.StartLook();

            while (true)
            {
                (Player player, bool attackable) = targeter.FindTarget();
                chaser.CurrentTarget = player;
                looker.CurrentTarget = player;

                if (player != null)
                {
                    if (Mathf.Abs(Vector3.Distance(player.Position, Npc.Position) - chaser.KitingRange) > chaser.KitingTolerance) acter.TrySprint();
                    else acter.TryWalk();

                    if (attackable)
                    {
                        acter.TryShoot(false);
                        magModule.ServerModifyAmmo(255);
                        yield return Timing.WaitForSeconds(0.2f);
                        continue;
                    }
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
