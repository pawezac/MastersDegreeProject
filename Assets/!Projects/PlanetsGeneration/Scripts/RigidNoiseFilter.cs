using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidNoiseFilter : INoiseFilter
{
    Noise noise = new Noise();
    NoiseSettings.RigidNoiseSettings settings;

    public RigidNoiseFilter(NoiseSettings noiseSettings)
    {
        this.settings = noiseSettings.rigidNoiseSettings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.BaseRoughness;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < settings.numOfNoiseLayers; i++)
        {
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.Centre));
            v *= v;
            v *= weight;

            weight = Mathf.Clamp01(v * settings.weightMultiplier); // its because we want to get more rigid detail on the higher number of layers

            noiseValue += v * amplitude;
            frequency *= settings.Roughness; //when Roughness is more than one frequency will increase with each layer
            amplitude *= settings.Persistence; //when Persistence is more than one amplitude will decrease with each layer
        }

        noiseValue = noiseValue - settings.minVal;
        return noiseValue * settings.Strength;
    }
}
