using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class FallofGenerator
{
    public static float[,] GenerateFallofMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                map[i, j] = Evaluate(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)));
            }
        }
        return map;
    }

    static float a = 3;
    static float b = 2.2f;

    static float Evaluate(float val)
    {
        float tmp = Mathf.Pow(val, a);
        return tmp / (tmp + Mathf.Pow(b-b*val,a));
    }
}
