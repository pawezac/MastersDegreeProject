using System;
using TypeReferences;

public class NoiseFilterFactory
{
    public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
    {
        switch (settings.filterType)
        {
            case NoiseSettings.FilterType.SimpleFilter:
                return new SimpleNoiseFilter(settings);
            case NoiseSettings.FilterType.RigidFilter:
                return new RigidNoiseFilter(settings);
            default:
                throw new NotImplementedException();
        }
    }
}
