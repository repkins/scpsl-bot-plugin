using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door.Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindFactory
    {
        private const KeycardPermissions KeycardO5Permissions = KeycardPermissions.Checkpoints | KeycardPermissions.ExitGates | 
                                                                KeycardPermissions.Intercom | KeycardPermissions.AlphaWarhead | 
                                                                KeycardPermissions.ContainmentLevelOne | KeycardPermissions.ContainmentLevelTwo | KeycardPermissions.ContainmentLevelThree | 
                                                                KeycardPermissions.ArmoryLevelOne | KeycardPermissions.ArmoryLevelTwo | KeycardPermissions.ArmoryLevelThree;

        public static void BuildMind(FpcMind mind, FpcBotPlayer botPlayer, FpcBotPerception perception)
        {
            mind.AddBelief(new LastKnownItemLocation<KeycardItem>());
            mind.AddBelief(new LastKnownItemLocation<Medkit>());
            mind.AddBelief(new LastKnownItemLocation<Firearm>());


            mind.AddBelief(new KeycardWithinSight(KeycardO5Permissions, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new KeycardWithinPickupDistance(KeycardO5Permissions, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new KeycardInInventory(KeycardO5Permissions, perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new FindKeycard(KeycardO5Permissions, botPlayer));
            mind.AddActivity(new GoToPickupKeycard(KeycardO5Permissions, botPlayer));
            mind.AddActivity(new PickupKeycard(KeycardO5Permissions, botPlayer));


            mind.AddBelief(new KeycardWithinSight(KeycardPermissions.ContainmentLevelOne, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new KeycardWithinPickupDistance(KeycardPermissions.ContainmentLevelOne, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new KeycardInInventory(KeycardPermissions.ContainmentLevelOne, perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new FindKeycard(KeycardPermissions.ContainmentLevelOne, botPlayer));
            mind.AddActivity(new GoToPickupKeycard(KeycardPermissions.ContainmentLevelOne, botPlayer));
            mind.AddActivity(new PickupKeycard(KeycardPermissions.ContainmentLevelOne, botPlayer));


            mind.AddBelief(new ItemWithinSight(ItemType.Medkit, perception.GetSense<ItemsWithinSightSense>()));


            mind.AddBelief(new DoorWithinSight<PryableDoor>());
            mind.AddBelief(new ClosedScp914ChamberDoorWithinSight());

            //mind.AddActivity(new Explore(botPlayer));

            mind.AddDesire(new GetKeycardContainmentOne());
            mind.AddDesire(new GetO5Keycard(KeycardO5Permissions));
        }
    }
}
