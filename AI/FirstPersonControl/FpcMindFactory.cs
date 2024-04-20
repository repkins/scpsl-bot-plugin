using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindFactory
    {
        private const KeycardPermissions KeycardO5Permissions = KeycardPermissions.Checkpoints | KeycardPermissions.ExitGates | 
                                                                KeycardPermissions.Intercom | KeycardPermissions.AlphaWarhead | 
                                                                KeycardPermissions.ContainmentLevelOne | KeycardPermissions.ContainmentLevelTwo | KeycardPermissions.ContainmentLevelThree | 
                                                                KeycardPermissions.ArmoryLevelOne | KeycardPermissions.ArmoryLevelTwo | KeycardPermissions.ArmoryLevelThree;
        private const KeycardPermissions PermissionsCheckpointContainmentLevelOneTwo = KeycardPermissions.Checkpoints | KeycardPermissions.ContainmentLevelOne | KeycardPermissions.ContainmentLevelTwo;

        public static void BuildMind(FpcMind mind, FpcBotPlayer botPlayer, FpcBotPerception perception)
        {
            mind.AddBelief(new RoomEnterLocation(perception.GetSense<RoomSightSense>()));


            mind.AddBelief(new DoorObstacle(perception.GetSense<DoorsWithinSightSense>(), botPlayer.Navigator));


            mind.AddBelief(new Scp914Location(perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new Scp914Controls(perception.GetSense<InteractablesWithinSightSense>()));
            mind.AddBelief(new Scp914RunningOnSetting(perception.GetSense<RoomSightSense>()));

            mind.AddActivity(new GoToSearchRoomForScp914(botPlayer));
            mind.AddActivity(new GoToStartScp914OnSetting(Scp914.Scp914KnobSetting.Fine, botPlayer));


            //mind.AddBelief(new ItemSpawnLocation<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSightedLocation<ItemOfType>(ItemType.KeycardScientist, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardScientist, perception.GetSense<ItemsInInventorySense>()));

            //mind.AddActivity(new GoToItemSpawnLocation<ItemOfType>(ItemType.KeycardScientist, botPlayer));
            mind.AddActivity(new GoToSearchRoom<ItemOfType>(ItemType.KeycardScientist, botPlayer));
            mind.AddActivity(new GoToPickupItem<ItemOfType>(ItemType.KeycardScientist, botPlayer));


            mind.AddBelief(new ItemSightedLocation<ItemOfType>(ItemType.KeycardResearchCoordinator, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardResearchCoordinator, perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new GoToPickupItem<ItemOfType>(ItemType.KeycardResearchCoordinator, botPlayer));


            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardScientist)));
            mind.AddActivity(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardScientist), botPlayer));

            var outputKeycardCriterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardResearchCoordinator),
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
                new KeycardWithPermissions(PermissionsCheckpointContainmentLevelOneTwo),
            };
            foreach (var outputCriteria in outputKeycardCriterias)
            {
                mind.AddBelief(new ItemInOutakeChamber(outputCriteria, perception.GetSense<ItemsWithinSightSense>()));
            }            
            mind.AddActivity(new WaitForItemUpgrading(ItemType.KeycardScientist, outputKeycardCriterias, Scp914.Scp914KnobSetting.Fine));

            mind.AddActivity(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardResearchCoordinator), botPlayer));


            //mind.AddBelief(new ItemSpawnLocation<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSightedLocation<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new GoToSearchRoom<ItemOfType>(ItemType.KeycardO5, botPlayer));
            //mind.AddActivity(new GoToItemSpawnLocation<ItemOfType>(ItemType.KeycardO5, botPlayer));
            mind.AddActivity(new GoToPickupItem<ItemOfType>(ItemType.KeycardO5, botPlayer));


            //mind.AddBelief(new ItemSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSightedLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new GoToSearchRoom<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), botPlayer));
            mind.AddActivity(new GoToPickupItem<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), botPlayer));




            mind.AddActivity(new OpenNonKeycardDoorObstacle(botPlayer));
            mind.AddActivity(new OpenKeycardDoorObstacle(KeycardPermissions.ContainmentLevelOne, botPlayer));
            mind.AddActivity(new WaitForDoorOpening(botPlayer));


            mind.AddBelief(new ItemSightedLocation<ItemOfType>(new(ItemType.Medkit), perception.GetSense<ItemsWithinSightSense>()));

            mind.AddDesire(new GetResearchSupervisorKeycard());
            //mind.AddDesire(new GetO5Keycard());
        }
    }
}
