using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetsGeneration
{
    public class MinMax
    {
        public MinMax()
        {
            Max = float.MinValue;
            Min = float.MaxValue;
        }

        public float Min { get; private set; }
        public float Max { get; private set; }

        public void AddValue(float value)
        {
            if (value > Max)
            {
                Max = value;
            }
            if (value < Min)
            {
                Min = value;
            }
        }
    }
}