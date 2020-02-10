using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NimiMobilePack;
using PathCreation;

[System.Serializable]
public class Chunk
{
    private float Steepness;
    private float Curvyness;
    private float SegmentSpacing;
    [SerializeField]
    private Vector3[] Segments;

    public float steepness => Steepness;
    public float curvyness => Curvyness;
    public float spacing => SegmentSpacing;
    public Vector3[] segments => Segments;

    public Chunk(Vector3 start, float curvyness, float steepness, float spacing)
    {       
        Steepness = steepness;
        Curvyness = curvyness;
        SegmentSpacing = spacing;
        Segments = new Vector3[2];

        Segments[0] = start;
        Segments[0].z += spacing;

        for (int i = 1; i < 2; i++)
        {
            float x = Random.Range(-1f, 1f) * Curvyness;
            float y = Random.Range(-1f, 1f) * Steepness;
            float z = Segments[i - 1].z + SegmentSpacing;

            Segments[i] = new Vector3(x, y, z);
        }
    }


    public override string ToString()
    {
        return segments.Length.ToString();
    }
}

[System.Serializable]
public class LevelData
{
    public List<Chunk> Chunks;
    public float TrafficDensity;

    public LevelData(List<Chunk> chunks, float trafficDensity)
    {
        Chunks = chunks;
        this.TrafficDensity = trafficDensity;
    }
}

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance { get; set; }
    public static VertexPath path => instance.Path;

    public bool FlattenSurface;
    [Header("Road settings")]
    public float BaseCurvyness;
    public float BaseSteepness;
    public float BaseSpacing;
    public float RoadWidth = .4f;
    public float RoadThickness = .15f;
    public float RoadTextureTiling = 1;
    public Material RoadMaterial;
    public Material RoadUndersideMaterial;

    [Header("Ground settings")]
    public float GroundWidth = .4f;
    public float GroundThickness = .15f;
    public bool GroundFollowsPathRotation;
    public float GroundTextureTiling = 1;
    public Material GroundMaterial;
    public Material GroundUndersideMaterial;

    [SerializeField, HideInInspector]
    private GameObject meshHolder;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;


    private VertexPath Path;
    private LevelData data;
    private List<Chunk> PathChunks;

    void Awake()
    {
        instance = this;

        List<Vector3> points = new List<Vector3>();

        points.Add(Vector3.zero);
        points.Add(Vector3.forward * 100f);

        //Generates the bezier curve and creates mesh around it.
        GenerateLevel(CombineSegments(GenerateChunks()));
    }

    private void Start()
    {
        Traffic.Create(Path, Random.Range(15,25));//Create moving cars along the path with random density
        LevelDetails.Create(Path, 60);
    }

    private List<Chunk> GenerateChunks()
    {
        List<Chunk> pathChunks = new List<Chunk>();

        Chunk startChunk = new Chunk(Vector3.zero, 0, 15, 40);
        pathChunks.Add(startChunk);

        for (int i = 1; i < 40; i++)
        {
            Chunk chunk = new Chunk(pathChunks[i - 1].segments[1], 0f, 20f, 80f);
            pathChunks.Add(chunk);
        }

        NSaveLoadSystem.Save(new LevelData(pathChunks, 10f));

        return pathChunks;
    }

    private void GenerateLevel(Vector3[] segments)
    {    
        BezierPath bp = new BezierPath(segments, false, PathSpace.xyz);
        bp.ControlPointMode = BezierPath.ControlMode.Automatic;
        bp.GlobalNormalsAngle = 90f;
        Path = new VertexPath(bp, transform, 0.6f, 0f);
        CreateRoad();
        CreateGround();       
    }

    private Vector3[] CombineSegments(List<Chunk> list)
    {
        List<Vector3> ret = new List<Vector3>();

        for(int i = 0; i < list.Count; i++)
        {
            ret.AddRange(list[i].segments);
        }


        return ret.ToArray();
    }

    private void CreateRoad()
    {
        GameObject meshHolder = PathMeshCreator.CreateMeshHolder("Road");
        meshHolder.tag = "Road";
        PathMeshCreator.AssignMaterials(meshHolder.GetComponent<MeshRenderer>(),RoadMaterial, RoadUndersideMaterial, RoadTextureTiling);
        PathMeshCreator.CreateRoadMesh(Path,meshHolder.GetComponent<MeshFilter>().sharedMesh, RoadWidth, RoadThickness, FlattenSurface, false);
        MeshCollider ms = meshHolder.AddComponent<MeshCollider>();
        ms.material = Resources.Load<PhysicMaterial>("ZeroFriction");
        meshHolder.layer = LayerMask.NameToLayer("Road");

    }

    private void CreateGround()
    {
        GameObject meshHolder = PathMeshCreator.CreateMeshHolder("Ground");
        meshHolder.transform.position += Vector3.down * 1f; //Offset from road
        PathMeshCreator.AssignMaterials(meshHolder.GetComponent<MeshRenderer>(),GroundMaterial, GroundUndersideMaterial, GroundTextureTiling);
        PathMeshCreator.CreateRoadMesh(Path, meshHolder.GetComponent<MeshFilter>().sharedMesh, GroundWidth, GroundThickness, FlattenSurface, GroundFollowsPathRotation);
    }

    

}
