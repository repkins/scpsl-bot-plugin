using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcBotShootAction : IFpcBotAction
    {
        public FpcBotShootAction(FpcBotPlayer fpcBotPlayer)
        {
            _botPlayer = fpcBotPlayer;
        }

        public void OnEnter()
        { }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            // Update target.

            var fpcTransform = fpcRole.FpcModule.transform;
            var hub = fpcTransform.GetComponentInParent<ReferenceHub>();

            if (!_botPlayer.Perception.EnemiesWithinSight.Any())
            {
                return;
            }

            var closestTarget = _botPlayer.Perception.EnemiesWithinSight
                .Select(o => new { hub, distSqr = Vector3.SqrMagnitude(o.transform.position - fpcTransform.position) })
                .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                .hub;

            _targetToShoot = closestTarget.transform.position;

            // Update aim to target.

            _botPlayer.DesiredMoveDirection = Vector3.zero;

            var directionToTarget = Vector3.Normalize(_targetToShoot - fpcTransform.position);

            var angleDiff = Vector3.SignedAngle(directionToTarget, fpcTransform.forward, Vector3.down);
            _botPlayer.DesiredLook = new Vector3(0, angleDiff);

            // Update shooting.

            var equippedFirearm = hub.inventory.CurInstance as Firearm;
            if (equippedFirearm == null)
            {
                var firearm = hub.inventory.UserInventory.Items.Values.FirstOrDefault(i => i is Firearm) as Firearm;
                if (firearm == null)
                {
                    Log.Warning($"Can't find firearm in the inventory. ({hub})");
                    return;
                }

                hub.inventory.ServerSelectItem(firearm.ItemSerial);

                equippedFirearm = hub.inventory.CurInstance as Firearm;
            }

            if (equippedFirearm.ActionModule.ServerAuthorizeShot())
            {
                equippedFirearm.HitregModule.ClientCalculateHit(out var shotMessage);
                equippedFirearm.HitregModule.ServerProcessShot(shotMessage);
                equippedFirearm.OnWeaponShot();
            }

            if (equippedFirearm.Status.Ammo <= 0)
            {
                var requestMessage = new RequestMessage(equippedFirearm.ItemSerial, RequestType.Reload);
                _botPlayer.BotHub.ConnectionToServer.Send(requestMessage);
            }
        }

        private FpcBotPlayer _botPlayer;

        private Vector3 _targetToShoot;
    }
}
