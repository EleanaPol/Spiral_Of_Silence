using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Agent Personality Settings")]
    public int identity;
    public int hidden_identity;
    public float probability_to_speak;

    public Vector3 a_position;
    public Vector3 a_prev_position;
    public Vector3 a_velocity;

    // a variable that will control the length of our moving vector
    public float randomness_strength;
    public float attraction_strength;
    public float separation_strength;
    public float cohesion_strength;
    public float alignment_strength;
    public float stigmergy_strength;

    public float separation_threshold;
    public float cohesion_threshold;
    public float alignment_threshold;

    public GameObject attractor;

    private Vector3 moving_vector;
    private Vector3 attraction_vector;
    private Vector3 separation_vector;
    private Vector3 cohesion_vector;
    private Vector3 align_vector;
    private Vector3 gradient;

    public StigmergyManager stigmergy;

    //private Vector3 gradientVector;

    //public StigmergyManager stigmergy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void CalculateAgentForces(List<Vector3> positions, List<Vector3> velocities, List<int> identities)
    {
        // this method calculates all the seperate vectors that are going to be applied on the agent's current position
        moving_vector = RandomWalk();
        attraction_vector = AttractToPoint();
        separation_vector = Separate(positions);
        cohesion_vector = Cohesion(positions);
        align_vector = Align(positions, velocities);
        gradient = stigmergy.SampleChemical(transform.position);
    }

    public void MoveAgent()
    {
        // this method applies all the calculated forces to the current agent's position to make it move
        transform.position = transform.position
            + moving_vector * randomness_strength * Time.deltaTime
            + attraction_vector * attraction_strength * Time.deltaTime
            + separation_vector * separation_strength * Time.deltaTime
            + cohesion_vector * cohesion_strength * Time.deltaTime
            + align_vector * alignment_strength * Time.deltaTime
            + gradient * stigmergy_strength * Time.deltaTime;
            

        a_position = transform.position;
        UpdateAgentVelocity();
    }

    public void UpdateAgentVelocity()
    {
        a_velocity = a_position - a_prev_position;
        a_prev_position = a_position;
    }


    public Vector3 RandomWalk()
    {
        // creating a variable that will hold the returned vector value
        Vector3 mover;

        // creating random float numbers for the new x. y and z coordinates
        float x = Random.Range(-1.0f, 1.0f);
        float y = Random.Range(-0.5f, 0.5f);
        float z = Random.Range(-1.0f, 1.0f);

        // creating a new Vector, and instance of the Vector 3 class 
        mover = new Vector3(x, y, z);

        // return the calculated random vector as the result of this method's functionality
        return mover;
    }

    public Vector3 AttractToPoint()
    {
        Vector3 seeker;

        // starting point
        Vector3 positionA = transform.position;
        // target point
        Vector3 positionB = attractor.transform.position;

        // constructing the seeker vector, travelling from point A to point B
        seeker = positionB - positionA;

        return seeker;
    }

    public Vector3 Separate(List<Vector3> positions)
    {
        Vector3 separation = Vector3.zero;
        int separation_steps = 0;

        int agentCount = positions.Count;

        for(int i=0; i<agentCount; i++)
        {
            Vector3 repulsion_v = transform.position - positions[i];
            float distance = repulsion_v.magnitude;

            if (distance <= separation_threshold && distance > 0)
            {
                // add calculated repulsion vector to the overall separation vector
                separation = separation + repulsion_v;
                // increase separation steps by one
                separation_steps ++;
            }
        }

        if(separation_steps > 0)
        {
            separation = separation / separation_steps;
        }


        return separation; 
    }

    public Vector3 Cohesion(List<Vector3> positions)
    {
        Vector3 average_point = Vector3.zero;
        Vector3 coh = Vector3.zero;
        int cohesion_steps = 0;

        for(int i =0; i<positions.Count; i++)
        {
            Vector3 pos = positions[i];
            Vector3 diference = transform.position - pos;
            float distance = diference.magnitude;

            // add a condition that checks the current agent's identity against the neighbours idenitites
            // add another if statement here

            if (distance <= cohesion_threshold && distance > 0)
            {
                average_point = average_point + pos;
                cohesion_steps++;
            }
        }

        if(cohesion_steps > 0)
        {
            average_point = average_point / cohesion_steps;
            coh = average_point - transform.position;
        }


        return coh;
    }

    public Vector3 Align(List<Vector3> positions, List<Vector3> velocities)
    {
        Vector3 al = Vector3.zero;
        int alignment_steps = 0;
        

        for(int i=0; i<positions.Count; i++)
        {
            Vector3 pos = positions[i];
            Vector3 vel = velocities[i];

            Vector3 diff = pos - transform.position;
            float distance = diff.magnitude;

            if(distance <= alignment_threshold && distance > 0)
            {
                al = al + vel;
                
                alignment_steps++;
            }
        }

        if (alignment_steps > 0)
        {
            al = al / alignment_steps;
        }

        return al;
    }
}
