using InventorySystem.Items.Firearms;
using PluginAPI.Core;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class AttackTarget : IFpcAction
    {
        public ReferenceHub TargetToAttack { get; set; }

        public AttackTarget(FpcBotPlayer fpcBotPlayer)
        {
            _botPlayer = fpcBotPlayer;
            _lookAction = new FpcLookAction(fpcBotPlayer);
            _fireAction = new FpcFireAction(fpcBotPlayer);
            _equipAction = new FpcEquipAction(fpcBotPlayer);
            _reloadAction = new FpcReloadFirearmAction(fpcBotPlayer);
        }

        public void Reset()
        {
            _lookAction.Reset();
            _fireAction.Reset();
            _equipAction.Reset();
            _reloadAction.Reset();

            TargetToAttack = null;
        }

        public void UpdatePlayer()
        {
            var fpcTransform = _botPlayer.FpcRole.FpcModule.transform;
            var hub = fpcTransform.GetComponentInParent<ReferenceHub>();

            if (!TargetToAttack)
            {
                return;
            }

            // Update equipped firearm.

            var equippedFirearm = hub.inventory.CurInstance as Firearm;
            if (equippedFirearm == null)
            {
                var firearm = hub.inventory.UserInventory.Items.Values.FirstOrDefault(i => i is Firearm) as Firearm;
                if (firearm == null)
                {
                    Log.Warning($"Can't find firearm in the inventory. ({hub})");
                    return;
                }

                _equipAction.TargetItem = firearm;
                _equipAction.UpdatePlayer();

                equippedFirearm = hub.inventory.CurInstance as Firearm;
            }

            // Update aim to target.

            var directionToTarget = Vector3.Normalize(TargetToAttack.transform.position - fpcTransform.position);

            _lookAction.TargetLookDirection = directionToTarget;
            _lookAction.UpdatePlayer();

            // Fire.

            _fireAction.Firearm = equippedFirearm;
            _fireAction.UpdatePlayer();

            // Reload if necesarry.

            if (equippedFirearm.Status.Ammo <= 0)
            {
                _reloadAction.Firearm = equippedFirearm;
                _reloadAction.UpdatePlayer();
            }
        }

        private FpcBotPlayer _botPlayer;
        private FpcLookAction _lookAction;
        private FpcFireAction _fireAction;
        private FpcEquipAction _equipAction;
        private FpcReloadFirearmAction _reloadAction;
    }
}
