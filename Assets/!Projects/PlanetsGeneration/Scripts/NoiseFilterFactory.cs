using System;

public class NoiseFilterFactory
{
    public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
    {
        if (settings.NoiseFilterType.Type == null) return null;
        return (INoiseFilter)Activator.CreateInstance(settings.NoiseFilterType, args: new object[1] { settings });
    }
}
