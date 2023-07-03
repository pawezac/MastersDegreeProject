using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralPyramidRenderer : MonoBehaviour
{
    [SerializeField] Mesh sourceMesh = default;
    [SerializeField] ComputeShader pyramidComputeShader = default;
    [SerializeField] ComputeShader triToVertComputeShader = default; //for adjustement in triangle count
    [SerializeField] Material material = default;
    [SerializeField] float height = 1f;
    [SerializeField] float animFrequency = 1f;

    //the structure to send to the compute shader
    //this layout kind asures that the data is lait out squentialy

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct SourceVertex
    {
        public Vector3 position;
        public Vector2 uv;
    }

    //a state variable to help keep track of weather compute buffers have been set up
    private bool initialized;
    //a compute buffer to hold vertex data of the source mesh
    private ComputeBuffer sourceVertBuffer;
    //a compute buffer to hold index data of the source mesh
    private ComputeBuffer sourceTriBuffer;
    //a compute buffer to hold vertex data of the generated mesh
    private ComputeBuffer drawBuffer;
    //a compute buffer to hold indirect draw arguments
    private ComputeBuffer argsBuffer;

    //the id of the kernel in the tri to vert count compute shader
    private int idPyramidKernel;
    private int idTriToVertKernel;
    private int dispatchSize;
    private Bounds localBounds;

    //th size of one entry into the various compute buffers
    const int SOURCE_VERT_STRIDE = sizeof(float) * (3 + 2);
    const int SOURCE_TRI_STRIDE = sizeof(int);
    const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 2) * 3);
    const int ARGS_STRIDE = sizeof(int) * 4;

    private void OnEnable()
    {
        if (initialized)
        {
            OnDisable();
        }
        initialized = true;

        //grab data from source mesh
        Vector3[] positions = sourceMesh.vertices;
        Vector2[] uvs = sourceMesh.uv;
        int[] tris = sourceMesh.triangles;

        //create the data to upload to the source vert buffer
        SourceVertex[] vertices = new SourceVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new SourceVertex()
            { position = positions[i], uv = uvs[i] };
        }
        int numTriangles = tris.Length / 3; //the number of triangles in the source mesh is the index array / 3

        //create compute buffers
        //the stride is the size , in bytes , each object in the buffer takes up
        sourceVertBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceVertBuffer.SetData(vertices);
        sourceTriBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceTriBuffer.SetData(tris);
        //we split each triangle into three new ones 
        drawBuffer = new ComputeBuffer(numTriangles * 3, DRAW_STRIDE, ComputeBufferType.Append);
        drawBuffer.SetCounterValue(0); //set count to zero just to be sure

        argsBuffer = new ComputeBuffer(1, ARGS_STRIDE, ComputeBufferType.IndirectArguments);
        //the data in the args buffer corresponds to : 
        // 0: vertex count per draw instance . we will only use one instance
        // 1: instance count : one
        // 2: start vertex location if using a graphics buffer
        // 3: start instance location if using a graphics buffer
        argsBuffer.SetData(new int[] { 0, 1, 0, 0 });

        //cache the kernel ids we will be dispatching
        idPyramidKernel = pyramidComputeShader.FindKernel("Main");
        idTriToVertKernel = triToVertComputeShader.FindKernel("Main");

        // set data on the shaders
        pyramidComputeShader.SetBuffer(idPyramidKernel, "_SourceVertices", sourceVertBuffer);
        pyramidComputeShader.SetBuffer(idPyramidKernel, "_SourceTriangles", sourceTriBuffer);
        pyramidComputeShader.SetBuffer(idPyramidKernel, "_DrawTriangles", drawBuffer);
        pyramidComputeShader.SetInt("_numSourceTriangles", numTriangles);

        triToVertComputeShader.SetBuffer(idTriToVertKernel, "_IndirectArgsBuffer", argsBuffer);

        material.SetBuffer("_DrawTriangles", drawBuffer);

        // calculate the number of threads to use . get the thread size from th kernel
        // then divide the number of triangles by that size
        pyramidComputeShader.GetKernelThreadGroupSizes(idPyramidKernel, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)numTriangles / threadGroupSize); // used ceil to make sure we will not ommit any triangles

        localBounds = sourceMesh.bounds;
        localBounds.Expand(height);
    }

    private void OnDisable()
    {
        if (initialized)
        {
            sourceVertBuffer.Release();
            sourceTriBuffer.Release();
            drawBuffer.Release();
            argsBuffer.Release();
        }
        initialized = false;
    }

    private void LateUpdate()
    {
        //clear th draw buffer of last frame's data
        drawBuffer.SetCounterValue(0);

        //update the shader with frame specific data
        pyramidComputeShader.SetMatrix("_localToWorld", transform.localToWorldMatrix);
        pyramidComputeShader.SetFloat("_pyramidHeight", height * Mathf.Sin(animFrequency * Time.timeSinceLevelLoad));

        //dispatch the pyramid shader . it will run on the gpu
        pyramidComputeShader.Dispatch(idPyramidKernel, dispatchSize, 1, 1);

        //copy the count (stack size) of the draw buffer to the args buffer,at the byte position zero
        // this sets vertex  count for our draw procedural indirect call
        ComputeBuffer.CopyCount(drawBuffer, argsBuffer, 0);

        //this the compute shader outputs triangles, but grpahics shader needs the number of vertices, 
        //we need to multiply the vertex count by three . we'll do this on the gpu with a compute shader
        // so we dont have to transfer data back to the cpu
        triToVertComputeShader.Dispatch(idTriToVertKernel, 1, 1, 1);

        //DrawProceduralIndirect queues a draw call up for out generated mesh
        // it will recive a shadow casting pass, like normal
        Graphics.DrawProceduralIndirect(material, TransformBounds(localBounds), MeshTopology.Triangles, argsBuffer, 0, null, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);
    }

    private Bounds TransformBounds(Bounds localBounds) // transform bounds to world space
    {
        var center = transform.TransformPoint(localBounds.center);

        // transform the local extents axes
        var extents = localBounds.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);

        //sume their absolute val to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds(center, extents);
    }
}
