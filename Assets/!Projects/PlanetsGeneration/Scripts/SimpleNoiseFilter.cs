using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetsGeneration
{
    public class SimpleNoiseFilter : INoiseFilter
    {
        Noise noise = new Noise();
        NoiseSettings.SimpleNoiseSettings settings;

        public SimpleNoiseFilter(NoiseSettings settings)
        {
            this.settings = settings.simpleNoiseSettings;
        }

        public float Evaluate(Vector3 point)
        {
            // noise.Evaluate reutrns value between -1 and 1 and we want value 0 , 1 , thus the additional calculations
            // roughness is applied because the further appart the points are we are sampling the grater the difference between those values will be

            float noiseValue = 0;
            float frequency = settings.BaseRoughness;
            float amplitude = 1;

            for (int i = 0; i < settings.numOfNoiseLayers; i++)
            {
                float v = noise.Evaluate(point * frequency + settings.Centre);
                noiseValue += (v + 1) * .5f * amplitude;
                frequency *= settings.Roughness; //when Roughness is more than one frequency will increase with each layer
                amplitude *= settings.Persistence; //when Persistence is more than one amplitude will decrease with each layer
            }

            noiseValue = noiseValue - settings.minVal;
            return noiseValue * settings.Strength;
        }
    }
}