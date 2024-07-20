using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Goals;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using MapGeneration.Distributors;
using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room;

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
            mind.AddBelief(new DoorObstacle(perception.GetSense<DoorsWithinSightSense>(), botPlayer.Navigator));
            mind.AddBelief(new GlassObstacle(perception.GetSense<GlassSightSense>(), botPlayer.Navigator));


            mind.AddBelief(new RoomEnterLocation(perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new ZoneWithin(perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new ZoneEnterLocation(FacilityZone.LightContainment, FacilityZone.HeavyContainment, perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new ZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.LightContainment, perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new ZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.Entrance, perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new ZoneEnterLocation(FacilityZone.Entrance, FacilityZone.HeavyContainment, perception.GetSense<RoomSightSense>()));

            mind.AddAction(new GoToZoneEnterLocation(FacilityZone.LightContainment, FacilityZone.HeavyContainment, botPlayer));
            mind.AddAction(new GoToZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.LightContainment, botPlayer));
            mind.AddAction(new GoToZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.Entrance, botPlayer));
            mind.AddAction(new GoToZoneEnterLocation(FacilityZone.Entrance, FacilityZone.HeavyContainment, botPlayer));
            mind.AddAction(new GoToSearchRoomForZoneEnterLocation(FacilityZone.LightContainment, FacilityZone.HeavyContainment, botPlayer));
            mind.AddAction(new GoToSearchRoomForZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.LightContainment, botPlayer));
            mind.AddAction(new GoToSearchRoomForZoneEnterLocation(FacilityZone.HeavyContainment, FacilityZone.Entrance, botPlayer));
            mind.AddAction(new GoToSearchRoomForZoneEnterLocation(FacilityZone.Entrance, FacilityZone.HeavyContainment, botPlayer));


            mind.AddBelief(new Scp914Location(perception.GetSense<RoomSightSense>()));
            mind.AddBelief(new Scp914Controls(perception.GetSense<InteractablesWithinSightSense>()));
            mind.AddBelief(new Scp914RunningOnSetting(perception.GetSense<RoomSightSense>()));

            mind.AddAction(new GoToSearchRoomForScp914(botPlayer));
            mind.AddAction(new GoToStartScp914OnSetting(Scp914.Scp914KnobSetting.Fine, botPlayer));
            mind.AddAction(new GoToStartScp914OnSetting(Scp914.Scp914KnobSetting.OneToOne, botPlayer));


            mind.AddBelief(new LockerSpawnsLocation(StructureType.StandardLocker, perception.GetSense<RoomSightSense>()));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardJanitor, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardJanitor, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardJanitor, botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardZoneManager, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardZoneManager, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardZoneManager, botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardScientist, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardScientist, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardScientist, botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardResearchCoordinator, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardResearchCoordinator, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardResearchCoordinator, botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardFacilityManager, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardFacilityManager, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardFacilityManager, botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(ItemType.KeycardO5, perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<ItemOfType>(ItemType.KeycardO5, botPlayer));


            var outputKeycardCriterias = new IItemBeliefCriteria[]
            {
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
                new KeycardWithPermissions(KeycardPermissions.ExitGates),
                new KeycardWithPermissions(KeycardPermissions.Intercom),
                new KeycardWithPermissions(KeycardPermissions.AlphaWarhead),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelThree),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelTwo),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelThree),
                new KeycardWithPermissions(PermissionsCheckpointContainmentLevelOneTwo),
            };
            foreach (var outputCriteria in outputKeycardCriterias)
            {
                mind.AddBelief(new ItemsInOutakeChamber(outputCriteria, perception.GetSense<ItemsWithinSightSense>()));
            }


            var outputKeycardFacilityManagerCriterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardFacilityManager),
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
                new KeycardWithPermissions(KeycardPermissions.ExitGates),
                new KeycardWithPermissions(KeycardPermissions.Intercom),
                new KeycardWithPermissions(KeycardPermissions.AlphaWarhead),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelThree),
                new KeycardWithPermissions(PermissionsCheckpointContainmentLevelOneTwo),
            };

            var outputKeycardResearchSupervisorCriterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardResearchCoordinator),
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
                new KeycardWithPermissions(PermissionsCheckpointContainmentLevelOneTwo),
            };

            var outputKeycardScientistCriterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardScientist),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
            };

            var outputKeycardZoneManagerCriterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardZoneManager),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
            };

            var outputKeycardO5Criterias = new IItemBeliefCriteria[]
            {
                new ItemOfType(ItemType.KeycardO5),
                new KeycardWithPermissions(KeycardPermissions.Checkpoints),
                new KeycardWithPermissions(KeycardPermissions.ExitGates),
                new KeycardWithPermissions(KeycardPermissions.Intercom),
                new KeycardWithPermissions(KeycardPermissions.AlphaWarhead),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelTwo),
                new KeycardWithPermissions(KeycardPermissions.ContainmentLevelThree),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelOne),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelTwo),
                new KeycardWithPermissions(KeycardPermissions.ArmoryLevelThree),
                new KeycardWithPermissions(PermissionsCheckpointContainmentLevelOneTwo),
            };

            #region KeycardScientist in intake chamber
            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardScientist)));
            mind.AddAction(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardScientist), botPlayer));
            #endregion

            #region KeycardJanitor in intake chamber
            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardJanitor)));
            mind.AddAction(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardJanitor), botPlayer));
            #endregion

            #region KeycardScientist to KeycardResearchCoordinator on Fine
            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardResearchCoordinator), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardScientist, outputKeycardResearchSupervisorCriterias, Scp914.Scp914KnobSetting.Fine));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardResearchCoordinator), botPlayer));
            #endregion

            #region KeycardZoneManager to KeycardFacilityManager on Fine
            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardZoneManager)));
            mind.AddAction(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardZoneManager), botPlayer));

            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardFacilityManager), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardZoneManager, outputKeycardFacilityManagerCriterias, Scp914.Scp914KnobSetting.Fine));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardFacilityManager), botPlayer));
            #endregion

            #region KeycardResearchCoordinator to KeycardFacilityManager on Fine
            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardResearchCoordinator)));
            mind.AddAction(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardResearchCoordinator), botPlayer));

            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardFacilityManager), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardResearchCoordinator, outputKeycardFacilityManagerCriterias, Scp914.Scp914KnobSetting.Fine));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardFacilityManager), botPlayer));
            #endregion

            #region KeycardScientist to KeycardZoneManager on 1:1
            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardZoneManager), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardScientist, outputKeycardZoneManagerCriterias, Scp914.Scp914KnobSetting.OneToOne));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardZoneManager), botPlayer));
            #endregion

            #region KeycardJanitor to KeycardScientist on Fine
            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardZoneManager), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardJanitor, outputKeycardZoneManagerCriterias, Scp914.Scp914KnobSetting.OneToOne));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardZoneManager), botPlayer));
            #endregion

            //#region KeycardJanitor to KeycardZoneManager on 1:1
            //mind.AddBelief(new ItemInOutakeChamber(new ItemOfType(ItemType.KeycardScientist), perception.GetSense<ItemsWithinSightSense>()));
            //mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardJanitor, outputKeycardScientistCriterias, Scp914.Scp914KnobSetting.Fine));
            //mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardScientist), botPlayer));
            //#endregion

            #region KeycardFacilityManager to KeycardO5 on Fine
            mind.AddBelief(new ItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardFacilityManager)));
            mind.AddAction(new GoToDropItemInIntakeChamber<ItemOfType>(new(ItemType.KeycardFacilityManager), botPlayer));

            mind.AddBelief(new ItemsInOutakeChamber(new ItemOfType(ItemType.KeycardO5), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddAction(new WaitForItemUpgrading(ItemType.KeycardFacilityManager, outputKeycardO5Criterias, Scp914.Scp914KnobSetting.Fine));
            mind.AddAction(new GoToItemInOutakeChamber<ItemOfType>(new(ItemType.KeycardO5), botPlayer));
            #endregion


            #region KeycardScientist searching
            mind.AddBelief(new ItemSpawnsLocation<ItemOfType>(ItemType.KeycardScientist, new[] { ItemType.KeycardScientist }, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<ItemOfType>(ItemType.KeycardScientist, new[] { ItemType.KeycardScientist }, perception.GetSense<LockersWithinSightSense>()));

            mind.AddAction(new GoToItemSpawnLocation<ItemOfType>(ItemType.KeycardScientist, botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<ItemOfType>(ItemType.KeycardScientist, StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<ItemOfType>(ItemType.KeycardScientist, botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<ItemOfType>(ItemType.KeycardScientist, idx, botPlayer));
            #endregion


            #region KeycardZoneManager searching
            mind.AddBelief(new ItemSpawnsLocation<ItemOfType>(ItemType.KeycardZoneManager, new[] { ItemType.KeycardZoneManager }, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<ItemOfType>(ItemType.KeycardZoneManager, new[] { ItemType.KeycardZoneManager }, perception.GetSense<LockersWithinSightSense>()));

            mind.AddAction(new GoToItemSpawnLocation<ItemOfType>(ItemType.KeycardZoneManager, botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<ItemOfType>(ItemType.KeycardZoneManager, StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<ItemOfType>(ItemType.KeycardZoneManager, botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<ItemOfType>(ItemType.KeycardZoneManager, idx, botPlayer));
            #endregion

            #region KeycardJanitor searching
            mind.AddBelief(new ItemSpawnsLocation<ItemOfType>(ItemType.KeycardJanitor, new[] { ItemType.KeycardJanitor }, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<ItemOfType>(ItemType.KeycardJanitor, new[] { ItemType.KeycardJanitor }, perception.GetSense<LockersWithinSightSense>()));

            mind.AddAction(new GoToItemSpawnLocation<ItemOfType>(ItemType.KeycardJanitor, botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<ItemOfType>(ItemType.KeycardJanitor, StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<ItemOfType>(ItemType.KeycardJanitor, botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<ItemOfType>(ItemType.KeycardJanitor, idx, botPlayer));
            #endregion


            mind.AddBelief(new ItemSightedLocations<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), botPlayer));

            var containmentLevelOneSpawnItemTypes = new ItemType[]
            {
                ItemType.KeycardJanitor, ItemType.KeycardZoneManager, ItemType.KeycardScientist, ItemType.KeycardResearchCoordinator,
                ItemType.KeycardGuard, ItemType.KeycardMTFPrivate, ItemType.KeycardMTFOperative, ItemType.KeycardMTFCaptain, ItemType.KeycardChaosInsurgency,
                ItemType.KeycardContainmentEngineer, ItemType.KeycardFacilityManager, ItemType.KeycardO5
            };
            mind.AddBelief(new ItemSpawnsLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), containmentLevelOneSpawnItemTypes, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), containmentLevelOneSpawnItemTypes, perception.GetSense<LockersWithinSightSense>()));
            
            mind.AddAction(new GoToItemSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), idx, botPlayer));


            mind.AddBelief(new ItemSightedLocations<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), botPlayer));

            var containmentLevelTwoSpawnItemTypes = new ItemType[]
            {
                ItemType.KeycardScientist, ItemType.KeycardResearchCoordinator,
                ItemType.KeycardMTFPrivate, ItemType.KeycardMTFOperative, ItemType.KeycardMTFCaptain, ItemType.KeycardChaosInsurgency,
                ItemType.KeycardContainmentEngineer, ItemType.KeycardFacilityManager, ItemType.KeycardO5
            };
            mind.AddBelief(new ItemSpawnsLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), containmentLevelTwoSpawnItemTypes, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), containmentLevelTwoSpawnItemTypes, perception.GetSense<LockersWithinSightSense>()));

            mind.AddAction(new GoToItemSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelTwo), idx, botPlayer));


            mind.AddBelief(new ItemSightedLocations<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), perception.GetSense<ItemsInInventorySense>()));
            mind.AddAction(new GoToPickupItem<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), botPlayer));

            var checkpointsSpawnItemTypes = new ItemType[]
            {
                ItemType.KeycardZoneManager, ItemType.KeycardResearchCoordinator,
                ItemType.KeycardGuard, ItemType.KeycardMTFPrivate, ItemType.KeycardMTFOperative, ItemType.KeycardMTFCaptain, ItemType.KeycardChaosInsurgency,
                ItemType.KeycardContainmentEngineer, ItemType.KeycardFacilityManager, ItemType.KeycardO5
            };
            mind.AddBelief(new ItemSpawnsLocation<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), checkpointsSpawnItemTypes, perception.GetSense<RoomSightSense>(), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemSpawnsInSightedLocker<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), checkpointsSpawnItemTypes, perception.GetSense<LockersWithinSightSense>()));

            mind.AddAction(new GoToItemSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), botPlayer));
            mind.AddAction(new GoToLockerSpawnLocation<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), StructureType.StandardLocker, botPlayer));
            mind.AddAction(new GoToItemSpawnInLocker<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), botPlayer));
            mind.AddActions(idx => new GoToSearchRoom<KeycardWithPermissions>(new(KeycardPermissions.Checkpoints), idx, botPlayer));


            mind.AddAction(new OpenNonKeycardDoorObstacle(botPlayer));
            mind.AddAction(new OpenKeycardDoorObstacle(KeycardPermissions.ContainmentLevelOne, botPlayer));
            mind.AddAction(new OpenKeycardDoorObstacle(KeycardPermissions.ContainmentLevelTwo, botPlayer));
            mind.AddAction(new OpenKeycardDoorObstacle(KeycardPermissions.Checkpoints, botPlayer));
            
            mind.AddAction(new WaitForChamberDoorOpening(botPlayer));


            mind.AddBelief(new ItemSightedLocations<ItemOfType>(new(ItemType.Medkit), perception.GetSense<ItemsWithinSightSense>()));

            //mind.AddGoal(new GetResearchSupervisorKeycard());
            mind.AddGoal(new EscapeTheFacility());
            mind.AddGoal(new GetO5Keycard());
        }
    }
}
