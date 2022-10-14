using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class Environment : MonoBehaviour
{

    public BoxCollider agent_region;
    public float cell_size;

    [Header("Debug")]
    public bool environment_outlines;
    public bool environment_tiles;
    public Material mat;

    [Header("Compute")]
    public ComputeShader compute;

    [HideInInspector] 
    public int field_resolution;

    private Vector3 min_pt;
    private Vector3 max_pt;

    [HideInInspector]
    public int x_res;
    [HideInInspector]
    public int z_res;

    private float x_size;
    private float z_size;

    [HideInInspector]
    public Vector3 axis_offset;
    private Vector3 center;

   

    [HideInInspector]
    public List<Vector3> grid_pts = new List<Vector3>();
    public List<Vector3> gradient = new List<Vector3>();


    // compute buffers
    private ComputeBuffer chemical_values;
    private RenderTexture env_rt;

    private void Awake()
    {
        InitEnvironment();

    }

    // Start is called before the first frame update
    void Start()
    {
        chemical_values = new ComputeBuffer(field_resolution, 4);

        env_rt = new RenderTexture(x_res, z_res, 0, RenderTextureFormat.ARGB32);
        env_rt.enableRandomWrite = true;
        env_rt.filterMode = FilterMode.Bilinear;
        env_rt.Create();

        mat.SetTexture("_MainTex", env_rt);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        chemical_values?.Release();
        env_rt?.Release();
    }

    private void InitEnvironment()
    {
        min_pt = agent_region.bounds.min;
        max_pt = agent_region.bounds.max;
        center = agent_region.bounds.center;

        axis_offset = Vector3.zero - new Vector3(min_pt.x,0,min_pt.z);

        x_size = max_pt.x - min_pt.x;
        z_size = max_pt.z - min_pt.z;

        x_res = (int)Mathf.Ceil(x_size / cell_size);
        z_res = (int)Mathf.Ceil(z_size / cell_size);

        field_resolution = x_res * z_res;

        CreateGridPts();
        if (environment_tiles) GenerateTiles();
    }

    private void GenerateTiles()
    {
        var environment_plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        environment_plane.transform.position = center;
        environment_plane.transform.localScale = new Vector3(x_size, z_size, 0.1f);
        environment_plane.name = "environment";
        environment_plane.transform.eulerAngles = new Vector3(90, 0, 0);


        environment_plane.GetComponent<MeshRenderer>().sharedMaterial = mat;
        
    }

    private void CreateGridPts()
    {
        for(int z=0; z<z_res; z++)
        {
            for(int x=0; x<x_res; x++)
            {
                var point = new Vector3(x * cell_size, 0, z * cell_size) - axis_offset;
                grid_pts.Add(point);
                gradient.Add(Vector3.zero);
            }
        }
    }

    public void ColorizeEnvironment(NativeArray<float> chemicals)
    {

        chemical_values.SetData(chemicals);
        compute.SetTexture(0, "Environment", env_rt);
        compute.SetBuffer(0, "Colors", chemical_values);
        compute.SetInt("res", x_res);
        compute.Dispatch(0, Mathf.Max(1, x_res / 8), Mathf.Max(1, z_res / 8), 1);
    }

    public void CalculateGradient(NativeArray<float> chemicals)
    {
        //int counter = 0;
        for (int z = 1; z < z_res-1; z++)
        {
            for (int x = 1; x < x_res-1; x++)
            {
                var index = z * x_res + x;
                var left = chemicals[index - 1];
                var right = chemicals[index + 1];
                var bottom = chemicals[index - x_res];
                var top = chemicals[index + x_res];

                var gr = new Vector3(left - right, 0, bottom - top);
                
                gradient[index] = gr.normalized * cell_size * 0.5f;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!environment_outlines) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(x_size, 0, z_size));

        
        for(int i=0; i<field_resolution; i++)
        {
            var pt = grid_pts[i];
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireCube(pt, new Vector3(cell_size, 0, cell_size));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pt,  pt + gradient[i]);

        }
    }

  


}
