using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal class PerceptionComponent : MonoBehaviour
    {
        private readonly List<ISense> senses = new();

        public void Awake()
        {
            var referenceHub = GetComponentInParent<ReferenceHub>();
            var botHub = BotManager.Instance.BotPlayers[referenceHub];
            var fpcPerception = botHub.FpcPlayer.Perception;

            senses.Add(fpcPerception.GetSense<ItemsWithinSightSense>());
            senses.Add(fpcPerception.GetSense<DoorsWithinSightSense>());
            senses.Add(fpcPerception.GetSense<PlayersWithinSightSense>());
            senses.Add(fpcPerception.GetSense<ItemsInInventorySense>());
            senses.Add(fpcPerception.GetSense<GlassSightSense>());
            senses.Add(fpcPerception.GetSense<LockersWithinSightSense>());
            senses.Add(fpcPerception.GetSense<SpatialSense>());
            senses.Add(fpcPerception.GetSense<RoomSightSense>());
            senses.Add(fpcPerception.GetSense<InteractablesWithinSightSense>());
        }

        public void OnTriggerEnter(Collider other)
        {
            foreach (var sense in senses)
            {
                sense.ProcessEnter(other);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            foreach (var sense in senses)
            {
                sense.ProcessExit(other);
            }
        }
    }
}
