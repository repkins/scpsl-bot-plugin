using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemLocations<C> : Location
    {
        public C Criteria { get; }
        public ItemLocations(C criteria)
        {
            this.Criteria = criteria;
        }
    }
}
