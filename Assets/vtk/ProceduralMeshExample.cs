using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshExample : MonoBehaviour
{
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh
        {
            name = "proceduralExampleMesh"
        };
        GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void GenerateMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        MeshJob<ProceduralSquare, ProceduralMeshMultiStream>.ScheduleParallel(
            mesh, meshData, default
        ).Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}
