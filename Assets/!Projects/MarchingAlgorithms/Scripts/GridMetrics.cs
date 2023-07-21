using UnityEngine.Rendering;

namespace MarchingTerrainGeneration
{
    public static class GridMetrics
    {
        public static int Scale = 32;

        public static int GroundLevel = Scale / 2;

        public const int NumThreads = 4;

        public static int[] LODs = {4,8, 16, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96, 104 };

        public static int LastLodLvl = LODs.Length - 1;

        public static int PointsPerChunk(int lod)
        {
            return LODs[lod];
        }

        public static int ThreadGroups(int lod)
        {
            return LODs[lod] / NumThreads;
        }
    }
}