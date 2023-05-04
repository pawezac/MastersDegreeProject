using System;
using UnityEngine;
using System.Linq;

public class ShapeGenerator
{
    ShapeSettings settings;
    INoiseFilter[] noiseFilters;
    public MinMax elevationMinMax;

    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            var noiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
            if (noiseFilter != null)
            {
                noiseFilters[i] = noiseFilter;
            }
        }
        elevationMinMax = new MinMax();
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointonUnitSphere)
    {
        float firstLayerValue = 0;

        float elevation = 0;

        if (noiseFilters.Length > 0 && noiseFilters[0] != null)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointonUnitSphere);
            if (settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (settings.noiseLayers[i] != null && settings.noiseLayers[i].enabled)
            {
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(pointonUnitSphere) * mask;
            }
        }
        elevation = settings.planetRadius * (1 + elevation);
        elevationMinMax.AddValue(elevation);
        return pointonUnitSphere * elevation;
    }
}
