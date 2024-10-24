using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class VTKParser : MonoBehaviour
{
    [SerializeField]
    private TextAsset binaryResource;
    private List<Vector3> vertices;
    public static int VertexCount = 0;
    private List<int> triangles;
    public static int TriangleCount = 0;
    private List<Vector3> normals;
    public static int NormalCount = 0;
    private List<float> scalars;
    private VTKSection currentSection;

    private enum VTKSection
    {
        None,
        Points,
        Connectivity,
        Polygons,
        Normals,
        Scalars
    }

    void Start()
    {
        //binaryResource = Resources.Load("bronch") as TextAsset;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        //scalars = new List<float>();

        //ParseVTKFile(binaryResource);
        ParseProcessedData();
    }

    void ParseProcessedData()
    {
        var binaryVertices = Resources.Load("vertices") as TextAsset;
        string[] vertLines = Regex.Split(binaryVertices.text, "\r\n|\r|\n");
        for (int i = 0; i < vertLines.Length; i++)
        {
            ParsePoints(vertLines[i]);
        }

        var binarytriangles = Resources.Load("triangles") as TextAsset;
        string[] triangleLines = Regex.Split(binarytriangles.text, "\r\n|\r|\n");
        for (int i = 0; i < triangleLines.Length; i++)
        {
            ParsePolygons(triangleLines[i]);
        }
        VertexCount = vertices.Count;
        NormalCount = 0;
        TriangleCount = triangles.Count;
        // Optionally create a mesh
        CreateMesh();
        //GenerateMesh();
    }
    void ParseVTKFile(TextAsset text)
    {
        string[] lines = Regex.Split(text.text, "\r\n|\r|\n");
        currentSection = VTKSection.None;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

                if (line.StartsWith("POINTS"))
                {
                    currentSection = VTKSection.Points;
                    continue;
                }
                if (line.StartsWith("CONNECTIVITY"))
                {
                    currentSection = VTKSection.Connectivity;
                    continue;
                }
                else if (line.StartsWith("POLYGONS"))
                {
                    currentSection = VTKSection.Polygons;
                    continue;
                }
                else if (line.StartsWith("NORMALS"))
                {
                    currentSection = VTKSection.Normals;
                    continue;
                }
                else if (line.StartsWith("SCALARS"))
                {
                    currentSection = VTKSection.Scalars;
                    continue;
                }

                switch (currentSection)
                {
                    case VTKSection.Points:
                        ParsePoints(line);
                        break;

                    case VTKSection.Polygons:
                        ParsePolygons(line);
                        break;

                    case VTKSection.Normals:
                        ParseNormals(line);
                        break;
                        // You can implement SCALARS parsing if needed
                }
        }
        VertexCount = vertices.Count;
        NormalCount = normals.Count;
        TriangleCount = triangles.Count;
        // Optionally create a mesh
        CreateMesh();
        //GenerateMesh();
        Debug.Log($"Found {VertexCount} vertices, {NormalCount} normals, and {TriangleCount/3} triangles!");
    }

    void ParsePoints(string line)
    {
        string[] pointValues = line.Split(new[] { ',',' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (pointValues.Length < 3)
            return;
        float x = float.Parse(pointValues[0], CultureInfo.InvariantCulture);
        float y = float.Parse(pointValues[1], CultureInfo.InvariantCulture);
        float z = float.Parse(pointValues[2], CultureInfo.InvariantCulture);
        vertices.Add(new Vector3(x, y, z));
    }

    void ParsePolygons(string line)
    {
        string[] polygonValues = line.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < polygonValues.Length; i++)
        {
            try
            {
                triangles.Add(int.Parse(polygonValues[i]));
            }
            catch 
            {
                Debug.Log("Found trash:" + polygonValues[i] + " in "+ line);
                return;
            }
        }
    }

    void ParseNormals(string line)
    {
        string[] normalValues = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < normalValues.Length; i += 3)
        {
            try
            {
                float x = float.Parse(normalValues[i], CultureInfo.InvariantCulture);
                float y = float.Parse(normalValues[i + 1], CultureInfo.InvariantCulture);
                float z = float.Parse(normalValues[i + 2], CultureInfo.InvariantCulture);
                normals.Add(new Vector3(x, y, z));
            }
            catch
            {
                Debug.Log("Found trash:" + normalValues[i]);
                return;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            name = "NaivelyGeneratedMesh"
        };

        if (normals.Count == vertices.Count)
        {
            mesh.normals = normals.ToArray();
        }
        else
        {
            mesh.RecalculateNormals();
        }

        mesh.RecalculateBounds();

        // Assign the mesh to a MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
        }

        Debug.Log($"Mesh created with {vertices.Count} vertices and {triangles.Count / 3} triangles.");
    }

    private void GenerateMesh()
    {
        Debug.Log("First index = " + triangles[0] + " second " + triangles[1]);
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            name = "NaivelyGeneratedMesh"
        };

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        MeshJob<ProceduralVTKMesh, ProceduralMeshMultiStream>.ScheduleParallel(
            mesh, meshData, default
        ).Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}

public struct ProceduralVTKMesh : IMeshGenerator
{
    public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
    public int VertexCount => VTKParser.VertexCount;
    public int IndexCount => VTKParser.TriangleCount;

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
