using CombatAI.API.Core;
using InventorySystem;
using InventorySystem.Items;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatAI.API.Module
{
    public class Attacker : Core.BaseCombatAIModule
    {
        private IFpcRole fpc;

        public Attacker(BaseCombatAI owner) : base(owner)
        {
            fpc = owner.Npc.RoleManager.CurrentRole as IFpcRole;
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


        public override void Destroy()
        {
            
        }
    }
}