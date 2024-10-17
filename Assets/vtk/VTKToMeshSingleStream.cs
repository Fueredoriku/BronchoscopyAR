using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


public struct SingleStream : IMeshStream
{
    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

    [StructLayout(LayoutKind.Sequential)]
    struct Stream0
    {
        public float3 position, normal;
        public float4 tangent;
        public float2 texCoord;
    }

    NativeArray<Stream0> stream0;
    NativeArray<int3> triangles;
    public void Setup(Mesh.MeshData meshData, int vertexCount, int indexCount)
    {
        var descriptor = new NativeArray<VertexAttributeDescriptor>(
            4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
        descriptor[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3
        );
        descriptor[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, dimension: 4
        );
        descriptor[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, dimension: 2
        );
        meshData.SetVertexBufferParams(vertexCount, descriptor);
        descriptor.Dispose();

        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));

        stream0 = meshData.GetVertexData<Stream0>();
        triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0
    {
        position = vertex.position,
        normal = vertex.normal,
        tangent = vertex.tangent,
        texCoord = vertex.texCoord
    };
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VTKToMeshSingleStream : MonoBehaviour
{
    //TODO: get these from the VTK file
    const int vertexAttributeCount = 4;
    private int vertexCount = 4;
    private int triangleIndexCount = 6;
    private void OnEnable()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3);
        vertexAttributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, dimension: 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, dimension: 2);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        half h0 = half(0f), h1 = half(1f);

        var vertex = new Vertex
        {
            normal = back(),
            tangent = half4(h1, h0, h0, half(-1f))
        };

        vertex.position = 0f;
        vertex.texCoord = h0;
        vertices[0] = vertex;

        vertex.position = right();
        vertex.texCoord = half2(h1, h0);
        vertices[1] = vertex;

        vertex.position = up();
        vertex.texCoord = half2(h0, h1);
        vertices[2] = vertex;

        vertex.position = float3(1f, 1f, 0f);
        vertex.texCoord = h1;
        vertices[3] = vertex;

        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;

        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            name = "VTKMesh",
            bounds = bounds
        };

        //mesh.vertices = new Vector3[]
        //mesh.triangles = new int[]
        //mesh.normals = new Vector3[];

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
