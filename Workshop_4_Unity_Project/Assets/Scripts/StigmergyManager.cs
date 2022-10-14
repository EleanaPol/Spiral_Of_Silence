using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class StigmergyManager : MonoBehaviour
{
    [Header("Referenced Elements")]
    public PopulationManager pop_manager;
    public Environment environment;

    [Header("Chemical Deposition Settings")]
    public float chemical_value;
    [Range(0.50000f,1)]
    public float chemical_decay;

    

    private int resolution;
    private int pop_count;
    private Vector3 offset;
    private float c_size;
    private int x_res;

    // native arrays
    private NativeArray<float> job_chemicals;

    

    // Start is called before the first frame update
    void Start()
    {
        resolution = environment.field_resolution;
        AllocateJobMemory();
    }

    // Update is called once per frame
    void Update()
    {
        if (pop_manager.agent_positions.Count <= 0) return;
        
        DepositChemical();
        DecayChemicals();
        environment.ColorizeEnvironment(job_chemicals);
        environment.CalculateGradient(job_chemicals);
    }

    private void OnDestroy()
    {
        ClearJobMemory();
        
    }

    #region c# code

    public Vector3 SampleChemical(Vector3 agent_pos)
    {
        Vector3 gradient = Vector3.zero;
        var p = agent_pos + offset;
        int2 coords = new int2((int)(agent_pos.x / c_size), (int)(agent_pos.z / c_size));
        if (coords.x >= environment.x_res || coords.x < 0 || coords.y >= environment.z_res || coords.y < 0) return gradient;
        int index = coords.y * x_res + coords.x;

        gradient = environment.gradient[index];

        return gradient;
    }

    private void DepositChemical()
    {
        var agent_positions = pop_manager.agent_positions;
        pop_count = agent_positions.Count;
        offset = environment.axis_offset;
        c_size = environment.cell_size;
        x_res = environment.x_res;

        for(int i=0; i<pop_count; i++)
        {
            var pos = agent_positions[i] + offset;
            int2 coords = new int2((int)(pos.x / c_size), (int)(pos.z / c_size));
            if (coords.x >= environment.x_res || coords.x < 0 || coords.y >= environment.z_res || coords.y < 0) continue;
            int index = coords.y * x_res + coords.x;

            job_chemicals[index] += chemical_value;
        }
    }

    private void DecayChemicals()
    {
        ScheduleChemicalDecay();
    }

    #endregion

    #region Jobs

    private void AllocateJobMemory()
    {
        job_chemicals = new NativeArray<float>(resolution, Allocator.Persistent);
    }

    private void ClearJobMemory()
    {
        if (job_chemicals.IsCreated) job_chemicals.Dispose();
    }

    private void ScheduleChemicalDecay()
    {
        var ChemicalDecay = new HandleChemicals
        {
            cells = job_chemicals,
            chemical_decay = chemical_decay
        };

        ChemicalDecay.Schedule(resolution, 128).Complete();
    }



    #endregion
}

[BurstCompile]
public struct HandleChemicals : IJobParallelFor
{
    public NativeArray<float> cells;
    public float chemical_decay;

    public void Execute(int index)
    {
        cells[index] *= chemical_decay;
    }
}

