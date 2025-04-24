using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPathDrawWifiApp
{
    public class StatusRestData
    {
        public bool Moving { get; set; }
        public bool Dispensing { get; set; }
        public bool StopOnEmpty { get; set; }
        public bool IsEmpty { get; set; }
        public bool ManualMode { get; set; }
        public byte Direction { get; set; }
        public uint DistanceTraveled { get; set; }
    }

    public struct PathStep
    {
        public UInt16 DistanceMM { get; set; }
        public UInt16 Angle { get; set; }

        public PathStep(UInt16 distance, UInt16 angle)
        {
            DistanceMM = distance;
            Angle = angle;
        }

        public override string ToString()
        {
            return $"({DistanceMM}, {Angle})";
        }
    }

    public class DrawCommandRestData
    {
        public List<PathStep> DrivePath { get; set; }
    }

    public class ManualCommandRestData
    {
        public bool Moving { get; set; } = false;
        public bool Dispensing { get; set; } = true;
        public bool StopOnEmpty { get; set; } = true;
        public bool ManualMode { get; set; } = true;
        public byte Direction { get; set; } = 0;
    }
}
