using CombatAI.API.Core;
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

        public Acter(BaseCombatAI owner) : base(owner)
        {
            
        }

        public void TryEquipItem(ItemType itemType, bool returnIfNull)
        {
            if (Owner.Npc.Inventory.TryGetInventoryItem(itemType, out ItemBase itemBase))
            {
                Owner.Npc.Inventory.ServerSelectItem(itemBase.ItemSerial);
            }
            else
            {
                if (!returnIfNull)
                {
                    ItemBase itemBase1 = Owner.Npc.Inventory.ServerAddItem(itemType, ItemAddReason.AdminCommand);
                    Owner.Npc.Inventory.ServerSelectItem(itemBase1.ItemSerial);
                }
            }
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