using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGenerator
{
    const string elevationProperty = "elevationMinMax";
    const string textProperty = "planetTexture";

    ColourSettings colourSettings;
    Texture2D texture;
    INoiseFilter biomeNoiseFilter;
    
    const int textRes = 50;

    Vector4 elevationVector;

    public void UpdateSettings(ColourSettings colourSettings)
    {
        this.colourSettings = colourSettings;
        int biomeNums = colourSettings.biomeColourSettings.biomes.Length;
        if (texture == null || texture.height != biomeNums)
        {
            texture = new Texture2D(textRes, biomeNums);
        }
        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(colourSettings.biomeColourSettings.noise);
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        elevationVector.x = elevationMinMax.Min;
        elevationVector.y = elevationMinMax.Max;
        colourSettings.planetMaterial.SetVector(elevationProperty, elevationVector);
    }

    public void UpdateColours()
    {
        Color[] colours = new Color[texture.width * texture.height];
        int colourIndex = 0;
        foreach (var biome in colourSettings.biomeColourSettings.biomes)
        {
            for (int i = 0; i < textRes; i++)
            {
                Color gradientColour = biome.gradient.Evaluate(i / (textRes - 1f));
                Color tintColour = biome.tint;
                colours[colourIndex] = gradientColour * (1 - biome.tintPercent) + tintColour * biome.tintPercent;
                colourIndex++;
            }
        }

        texture.SetPixels(colours);
        texture.Apply();
        colourSettings.planetMaterial.SetTexture(textProperty, texture);
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
        heightPercent += biomeNoiseFilter.Evaluate(pointOnUnitSphere) * colourSettings.biomeColourSettings.noiseOffset * colourSettings.biomeColourSettings.noiseStrength;
        float biomeIdx = 0;
        int numBiomes = colourSettings.biomeColourSettings.biomes.Length;
        float blendRange = colourSettings.biomeColourSettings.blendAmount / 2f + 0.001f;
        for (int i = 0; i < numBiomes; i++)
        {
            float dst = heightPercent - colourSettings.biomeColourSettings.biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIdx *= (1 - weight);
            biomeIdx += i * weight;
        }
        return biomeIdx / Mathf.Max(1, (numBiomes - 1));
    }
}
