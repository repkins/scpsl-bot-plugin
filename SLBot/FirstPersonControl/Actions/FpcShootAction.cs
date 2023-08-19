using InventorySystem.Items.Firearms;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcShootAction : IFpcBotAction
    {
        public FpcShootAction(FpcBotPlayer fpcBotPlayer)
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

            var numOfColliders = Physics.OverlapSphereNonAlloc(fpcTransform.position, 100f, _overlappingCollidersBuffer);
            if (numOfColliders <= 0)
            {
                return;
            }

            var closestTarget = _overlappingCollidersBuffer.Take(numOfColliders)
                .Select(c => c.GetComponentInParent<ReferenceHub>()).Where(o => o != null)
                .Where(o => o.GetFaction() != hub.GetFaction())
                .Select(o => new { o, d = Vector3.SqrMagnitude(o.transform.position - fpcTransform.position) })
                .Aggregate((a, c) => c.d < a.d ? c : a)
                .o;

            _targetToShoot = closestTarget.transform.position;

            // Update aim to target.

            var directionToTarget = Vector3.Normalize(_targetToShoot - fpcTransform.position);

            var angleDiff = Vector3.SignedAngle(_botPlayer.DesiredMoveDirection, fpcTransform.forward, Vector3.down);
            _botPlayer.DesiredLook = new Vector3(0, angleDiff);

            // Update shooting.

            var firearm = hub.inventory.CurInstance as Firearm;
            if (firearm == null)
            {
                Log.Warning($"Can't shoot without currently equipped firearm. ({hub})");
                return;
            }

            firearm.HitregModule.ClientCalculateHit(out var shotMessage);
            firearm.HitregModule.ServerProcessShot(shotMessage);
            firearm.OnWeaponShot();
        }

        private FpcBotPlayer _botPlayer;

        private Vector3 _targetToShoot;
        private Collider[] _overlappingCollidersBuffer = new Collider[OverlappingCollidersBufferSize];
        private const int OverlappingCollidersBufferSize = 10;
    }
}
