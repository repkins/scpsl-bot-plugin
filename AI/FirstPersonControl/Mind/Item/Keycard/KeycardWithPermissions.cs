using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard
{
    internal struct KeycardWithPermissions : IItemBeliefCriteria, IEquatable<KeycardWithPermissions>
    {
        public KeycardPermissions Permissions;
        public KeycardWithPermissions(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }

        public bool EvaluateItem(ItemPickupBase item)
        {
            return InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                && keycard.Permissions.HasFlag(Permissions);
        }

        public bool EvaluateItem(ItemBase item)
        {
            return item is KeycardItem keycard
                && keycard.Permissions.HasFlag(Permissions);
        }

        public bool CanOvercome(Collider hitCollider)
        {
            var door = hitCollider.GetComponentInParent<DoorVariant>();
            if (door is not null)
            {
                var doorPermissions = door.RequiredPermissions.RequiredPermissions;
                var doorKeycardPermissions = doorPermissions & ~KeycardPermissions.ScpOverride;
                if (doorKeycardPermissions == this.Permissions)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(IItemBeliefCriteria other)
        {
            return other is KeycardWithPermissions otherOf && this.Equals(otherOf);
        }

        public bool Equals(KeycardWithPermissions other)
        {
            return other.Permissions == this.Permissions;
        }

        public override string ToString()
        {
            return $"{Permissions}";
        }
    }
}
