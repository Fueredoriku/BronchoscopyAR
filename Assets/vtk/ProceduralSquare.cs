using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public struct ProceduralSquare : IMeshGenerator
{
    public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
    public int VertexCount => 4;

    public int IndexCount => 6;

    public int JobLength => 1;

    public void Execute<S>(int i, S streams) where S : struct, IMeshStream 
    {
        var vertex = new Vertex();
        vertex.normal.z = -1f;
        vertex.tangent.xw = float2(1f, -1f);

        streams.SetVertex(0, vertex);
        vertex.position = right();
        vertex.texCoord = float2(1f, 0f);
        streams.SetVertex(1, vertex);

        vertex.position = up();
        vertex.texCoord = float2(0f, 1f);
        streams.SetVertex(2, vertex);

        vertex.position = float3(1f, 1f, 0f);
        vertex.texCoord = 1f;
        streams.SetVertex(3, vertex);

        streams.SetTriangle(0, int3(0, 2, 1));
        streams.SetTriangle(1, int3(1, 2, 3));
    }
}
