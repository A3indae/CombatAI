using CombatAI.API.Core;
using Exiled.API.Features.Roles;
using InventorySystem;
using InventorySystem.Items;
using NetworkManagerUtils.Dummies;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatAI.API.Module
{
    public class Acter : Core.BaseCombatAIModule
    {

        private FpcRole fpc;

        public Acter(BaseCombatAI owner) : base(owner)
        {
            fpc = owner.Npc.Role as FpcRole;
        }

        public ItemBase TryEquipItem(ItemType itemType, bool returnIfNull)
        {
            if (Owner.Npc.Inventory.TryGetInventoryItem(itemType, out ItemBase item))
            {
                Owner.Npc.Inventory.ServerSelectItem(item.ItemSerial);
                return item;
            }

            if (returnIfNull) return null;

            item = Owner.Npc.Inventory.ServerAddItem(itemType, ItemAddReason.AdminCommand);
            Owner.Npc.Inventory.ServerSelectItem(item.ItemSerial);
            return item;
        }

        public void TrySprint()
        {
            fpc.MoveState = PlayerMovementState.Sprinting;
        }

        public void TryWalk()
        {
            fpc.MoveState = PlayerMovementState.Walking;
        }

        public void TrySneak()
        {
            fpc.MoveState = PlayerMovementState.Sneaking;
        }

        public void TryCrounch()
        {
            fpc.MoveState = PlayerMovementState.Crouching;
        }

        public void TryInvokeAction(string action)
        {
            foreach (var a in DummyActionCollector.ServerGetActions(Owner.Npc.ReferenceHub))
            {
                if (a.Name.EndsWith(action))
                {
                    a.Action();
                    break;
                }
            }
        }

        public void TryShoot(bool hold)
        {
            if (hold)
            {
                TryInvokeAction("Shoot->Hold");
            }
            else
            {
                TryInvokeAction("Shoot->Click");
            }
        }

        public void TryReload()
        {
            TryInvokeAction("Reload->Click");
        }

        public void TryReleaseShoot()
        {
            TryInvokeAction("Shoot->Release");
        }

        public void TryZoom()
        {
            TryInvokeAction("Zoom->Hold");
        }

        public void TryReleaseZoom()
        {
            TryInvokeAction("Zoom->Release");
        }

        public void TryUseItem()
        {
            TryInvokeAction("UsableItem->Start");
        }

        public void TryCancelItemAction()
        {
            TryInvokeAction("UsableItem->Cancel");
        }

        public override void Destroy()
        {
            
        }
    }
}