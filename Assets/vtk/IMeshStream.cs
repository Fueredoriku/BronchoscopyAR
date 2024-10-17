using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface IMeshStream 
{
    public void Setup(Mesh.MeshData data, int vertexCount, int indexCouunt);

    public void SetVertex(int index, Vertex data);

    public void SetTriangle(int index, int3 triangle);
}

public interface IMeshGenerator
{
    void Execute<S>(int i, S streams) where S : struct, IMeshStream;
}