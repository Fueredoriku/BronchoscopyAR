using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public interface IMeshStream 
{
    public void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCouunt);
    public void SetVertex(int index, Vertex data);
    public void SetTriangle(int index, int3 triangle);
}

public interface IMeshGenerator
{
    Bounds Bounds { get; }
    void Execute<S>(int i, S streams) where S : struct, IMeshStream;
    int VertexCount { get; }
    int IndexCount { get; }

    int JobLength { get; }
}


[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct MeshJob<G, S> : IJobFor
    where G : struct, IMeshGenerator
    where S : struct, IMeshStream
{
    G generator;
    [WriteOnly]
    S streams;

    public void Execute(int i) => generator.Execute(i, streams);

    public static JobHandle ScheduleParallel(
    Mesh mesh, Mesh.MeshData meshData, JobHandle dependency)
    {
        var job = new MeshJob<G, S>();
        job.streams.Setup(
            meshData,
            mesh.bounds = job.generator.Bounds,
            job.generator.VertexCount,
            job.generator.IndexCount
        );
        return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct TriangleUInt16
{
    public ushort a, b, c;

    public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16
    {
        a = (ushort)t.x,
        b = (ushort)t.y,
        c = (ushort)t.z
    };
}
