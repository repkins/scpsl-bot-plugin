using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.MapGeneration
{
    internal enum RoomZone
    {
        None = FacilityZone.None,
        LightContainment = FacilityZone.LightContainment,
        HeavyContainment = FacilityZone.HeavyContainment,
        Entrance = FacilityZone.Entrance,
        Surface = FacilityZone.Surface,
        Other = FacilityZone.Other
    }
}
