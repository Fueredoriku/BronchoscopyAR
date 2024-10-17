using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VTKToMesh : MonoBehaviour
{
    //TODO: get these from the VTK file
    const int vertexAttributeCount = 4;
    private int vertexCount = 4;

    private void OnEnable()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3, stream: 1);
        vertexAttributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, dimension: 4, stream: 2);
        vertexAttributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, dimension: 2, stream: 3);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        var mesh = new Mesh
        {
            name = "VTKMesh"
        };

        //mesh.vertices = new Vector3[]
        //mesh.triangles = new int[]
        //mesh.normals = new Vector3[];

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
