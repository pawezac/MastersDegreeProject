using MarchingTerrainGeneration;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] Chunk chunk;


    public void Test_Method() => chunk.Test();
    public void Quit()
    {
        Application.Quit();
    }
}
