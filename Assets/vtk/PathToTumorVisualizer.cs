using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.VFX;

public class PathToTumorVisualizer : MonoBehaviour
{
    private List<Vector3> vertices;
    public static int VertexCount = 0;
    private List<int> triangles;
    public static int TriangleCount = 0;
    private List<Vector3> normals;
    public static int NormalCount = 0;
    private List<float> scalars;
    private VTKSection currentSection;
    [SerializeField]
    private VisualEffect pathParticles;
    private VFXTextureFormatter positionBuffer;
    [SerializeField]
    private float particleScale = 0.01f;
    [SerializeField]
    private Vector3 positionOffset = Vector3.zero;
    [SerializeField]
    private Vector3 rotationOffset = Vector3.zero;
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
    private void OnDisable()
    {
        positionBuffer?.Dispose();
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
        var sampledVertices = vertices.Where((item, index) => (index + 1) % 10 == 0).ToArray();
        positionBuffer = new VFXTextureFormatter(sampledVertices.Length);
        positionBuffer.setValues(sampledVertices);
        positionBuffer.ApplyChanges();
        pathParticles.SetGraphicsBuffer("PathVertices", positionBuffer.Buffer);
        pathParticles.SetFloat("Count", sampledVertices.Length);
        pathParticles.Play();
        pathParticles.transform.SetLocalPositionAndRotation(positionOffset, Quaternion.Euler(rotationOffset));
        //StartCoroutine(WaitAndAdjustOffset());
        // Optionally create a mesh
        //CreateMesh();
        //GenerateMesh();
        //vertices.ForEach(vertex => Instantiate(drawPrefab, vertex/100f, Quaternion.identity, transform));
    }

    private IEnumerator WaitAndAdjustOffset()
    {
        yield return new WaitForSeconds(2f);
        
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
        Debug.Log($"Found {VertexCount} vertices, {NormalCount} normals, and {TriangleCount / 3} triangles!");
    }

    void ParsePoints(string line)
    {
        string[] pointValues = line.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (pointValues.Length < 3)
            return;
        float x = float.Parse(pointValues[0], CultureInfo.InvariantCulture);
        float y = float.Parse(pointValues[1], CultureInfo.InvariantCulture);
        float z = float.Parse(pointValues[2], CultureInfo.InvariantCulture);
        vertices.Add( new Vector3(x * particleScale, y * particleScale, z * particleScale));
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
                Debug.Log("Found trash:" + polygonValues[i] + " in " + line);
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
