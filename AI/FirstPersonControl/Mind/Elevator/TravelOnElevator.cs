using Interactables.Interobjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Elevator
{
    internal class TravelOnElevator : IAction
    {
        private ElevatorObstacle elevatorObstacle;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            elevatorObstacle = fpcMind.ActionImpactsWithCondition<ElevatorObstacle>(this, b => !b.Has());
        }

        private readonly FpcBotPlayer botPlayer;

        private TravelOnElevator(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var elevatorChamber = elevatorObstacle.Elevator;
            if (!elevatorChamber.IsReady)
            {
                return;
            }

            var elevatorGroup = elevatorChamber.AssignedGroup;
            var targetLvl = (elevatorChamber.CurrentLevel + 1) % ElevatorDoor.AllElevatorDoors[elevatorGroup].Count;

            var elevatorSyncMsg = new ElevatorManager.ElevatorSyncMsg(elevatorGroup, targetLvl);
            botPlayer.BotHub.ConnectionToServer.Send(elevatorSyncMsg);
        }

        public void Reset()
        {
        }
    }
}
