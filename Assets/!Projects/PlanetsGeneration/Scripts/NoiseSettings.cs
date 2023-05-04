using NaughtyAttributes;
using System;
using TypeReferences;
using UnityEngine;

[Serializable]
public class NoiseSettings 
{
    public enum FilterType { SimpleFilter, RigidFilter}
    public FilterType filterType;

    [ShowIf(nameof(filterType), FilterType.SimpleFilter), AllowNesting] public SimpleNoiseSettings simpleNoiseSettings = new SimpleNoiseSettings();
    [ShowIf(nameof(filterType), FilterType.RigidFilter), AllowNesting] public RigidNoiseSettings rigidNoiseSettings = new RigidNoiseSettings();

    [Serializable]
    public class SimpleNoiseSettings
    {
        public float Strength = 1;
        public float Roughness = 2;
        public float BaseRoughness = 1;
        public float Persistence = .5f;
        public float minVal;
        public Vector3 Centre;
        [Range(1, 8)] public int numOfNoiseLayers = 1;
    }

    [Serializable]
    public class RigidNoiseSettings : SimpleNoiseSettings
    {
        public float weightMultiplier = .8f;
    }
}