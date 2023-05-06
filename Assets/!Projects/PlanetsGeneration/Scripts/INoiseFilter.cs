using UnityEngine;

namespace PlanetsGeneration
{
    public interface INoiseFilter
    {
        float Evaluate(Vector3 point);
    }
}