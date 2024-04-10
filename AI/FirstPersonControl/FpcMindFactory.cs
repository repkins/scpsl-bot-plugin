using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Activities;
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
            mind.AddBelief(new ItemSightedLocation<ItemOfType>(new(ItemType.KeycardO5), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<ItemOfType>(new(ItemType.KeycardO5), perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new OldFindItem<ItemOfType>(ItemType.KeycardO5, botPlayer));
            mind.AddActivity(new GoToPickupItem<ItemOfType>(ItemType.KeycardO5, botPlayer));


            mind.AddBelief(new ItemSightedLocation<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsWithinSightSense>()));
            mind.AddBelief(new ItemInInventory<KeycardWithPermissions>(new(KeycardPermissions.ContainmentLevelOne), perception.GetSense<ItemsInInventorySense>()));

            mind.AddActivity(new FindKeycard(KeycardPermissions.ContainmentLevelOne, botPlayer));
            mind.AddActivity(new GoToPickupKeycard(KeycardPermissions.ContainmentLevelOne, botPlayer));


            mind.AddBelief(new ItemSightedLocation<ItemOfType>(new(ItemType.Medkit), perception.GetSense<ItemsWithinSightSense>()));

            //mind.AddActivity(new Explore(botPlayer));

            mind.AddDesire(new GetKeycardContainmentOne());
            mind.AddDesire(new GetO5Keycard());
        }
    }
}
