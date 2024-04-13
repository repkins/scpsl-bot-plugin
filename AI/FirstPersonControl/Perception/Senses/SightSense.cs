using PluginAPI.Core;
using System;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class SightSense
    {
        public SightSense(FpcBotPlayer botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        protected bool IsWithinSight<T>(Collider collider, T item) where T : MonoBehaviour
        {
            var playerHub = _fpcBotPlayer.BotHub.PlayerHub;
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            if (IsWithinFov(cameraTransform, collider.transform))
            {
                var relPosToItem = collider.bounds.center - cameraTransform.position;
                _numHits = Physics.RaycastNonAlloc(cameraTransform.position, relPosToItem, _hitsBuffer, relPosToItem.magnitude);

                if (_numHits == HitsBufferSize)
                {
                    Log.Warning($"{nameof(SightSense)} num of hits equal to buffer size, possible cuts.");
                }

                var hits = _hitsBuffer.Take(_numHits);
                if (hits.Any())
                {
                    hits = hits.OrderBy(h => h.distance);

                    //var hit = hits.First(h => (h.collider.gameObject.layer & LayerMask.GetMask("Hitbox")) <= 0);
                    var hit = hits.Select(h => new RaycastHit?(h))
                        .FirstOrDefault(h => h!.Value.collider.GetComponentInParent<ReferenceHub>() is not ReferenceHub otherHub || otherHub != playerHub);

                    if (hit.HasValue && hit.Value.collider.GetComponentInParent<T>() is T hitItem
                        && hitItem == item)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected static bool IsWithinFov(Transform transform, Transform targetTransform) => 
            IsWithinFov(transform.position, transform.forward, targetTransform.position);

        protected static bool IsWithinFov(Vector3 position, Vector3 forward, Vector3 targetPosition)
        {
            var facingDir = forward;
            var diff = Vector3.Normalize(targetPosition - position);

            if (Vector3.Dot(facingDir, diff) < 0)
            {
                return false;
            }

            if (Vector3.Angle(facingDir, diff) > 90)
            {
                return false;
            }

            return true;
        }

        private readonly FpcBotPlayer _fpcBotPlayer;

        private const int HitsBufferSize = 20;
        private RaycastHit[] _hitsBuffer = new RaycastHit[HitsBufferSize];
        private int _numHits;
    }
}
