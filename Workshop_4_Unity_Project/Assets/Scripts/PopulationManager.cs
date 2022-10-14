using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{  
    public int population_count;
    public BoxCollider agent_region;
    public GameObject agent_prefab;

    [Header("Agent Force Strengths")]
    public float random_strength;
    public float attract_strength;
    public float separate_strength;
    public float cohesion_strength;
    public float alignment_strength;
    public float stigmergy_strength;

    [Header("Force Thresholds")]
    public float separate_threshold;
    public float cohesion_threshold;
    public float alignment_threshold;

    [Header("Referenced Elements")]
    public GameObject attractor;
    public StigmergyManager stigmergy;

    [Header("Personality")]
    public List<Color> personality_colors;
    public float identity_reveal_interval;

    private Vector3 min_pt;
    private Vector3 max_pt;

    [HideInInspector]
    public List<Vector3> agent_positions = new List<Vector3>();
    public List<Vector3> agent_velocities = new List<Vector3>();
    public List<int> agent_personalities = new List<int>();

    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        min_pt = agent_region.bounds.min;
        max_pt = agent_region.bounds.max;

        Debug.Log(min_pt);
        Debug.Log(max_pt);

        InitializePopulation();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {

        UpdateAgentParameters();

        if(timer>= identity_reveal_interval)
        {
            UpdateIdentities();
            timer = 0;
        }
        
        timer = timer + Time.deltaTime;
    }

    public void InitializePopulation()
    {
        // iterate as many times as the number of agents we want to generate
        for (int i=0; i<population_count; i++)
        {
            // ========= GENERATE A RANDOM POSITION FOR EACH AGENT OF THE POPULATION

            //Debug.Log("iteration for element " + i.ToString());
            float ax = Random.Range(min_pt.x, max_pt.x);
            float az = Random.Range(min_pt.z, max_pt.z);

            //Debug.Log("creating coordinate x " + ax.ToString());
            //Debug.Log("creating coordinate z " + az.ToString());
            ///Debug.Log("-------end iteration " + i.ToString());

            Vector3 agent_pos = new Vector3(ax, 0, az);
            agent_positions.Add(agent_pos);
            agent_velocities.Add(Vector3.zero);
            agent_personalities.Add(0);

            // ========= GENERATE A NEW AGENT AND PLACE IT ON THE POSITION WE JUST GENERATED
            GameObject new_agent = Instantiate(agent_prefab);
            new_agent.transform.position = agent_pos;
            new_agent.name = "agent_" + i.ToString();
            new_agent.transform.SetParent(transform);
            Agent agent = new_agent.GetComponent<Agent>();

            // ========= APPLY AGENT PERSONALITY INFORMATION
            agent.identity = 0;
            agent.hidden_identity = Mathf.RoundToInt( Random.Range(1, 4.1f));
            agent.probability_to_speak = Random.Range(0.00f, 0.3f);// Random.value;
            //agent.GetComponent<MeshRenderer>().material.color = personality_colors[agent.hidden_identity-1];

            // ========= APPLY AGENT MOVEMENT INFORMATION
            // get the agent component from the gameobject

            agent.a_position = agent_pos;
            agent.a_prev_position = agent_pos;
            agent.a_velocity = Vector3.zero;

            agent.randomness_strength = random_strength;
            agent.attraction_strength = attract_strength;
            agent.separation_strength = separate_strength;
            agent.cohesion_strength = cohesion_strength;
            agent.alignment_strength = alignment_strength;
            agent.stigmergy_strength = stigmergy_strength;

            agent.separation_threshold = separate_threshold;
            agent.cohesion_threshold = cohesion_threshold;
            agent.alignment_threshold = alignment_threshold;

            agent.attractor = attractor;
            agent.stigmergy = stigmergy;
        }
    }

    public void UpdateAgentParameters()
    {
        // iterate the agent population to update parameters and positions
        for(int i=0; i<population_count; i++)
        {
            // current object --> specific child object --> specific child component
            Agent agent = transform.GetChild(i).GetComponent<Agent>();

            agent.randomness_strength = random_strength;
            agent.attraction_strength = attract_strength;
            agent.separation_strength = separate_strength;
            agent.cohesion_strength = cohesion_strength;
            agent.alignment_strength = alignment_strength;

            agent.separation_threshold = separate_threshold;
            agent.cohesion_threshold = cohesion_threshold;
            agent.alignment_threshold = alignment_threshold;

            agent.attractor = attractor;
            //agent.stigmergy = stigmergy;

            
            // grab the current agent's position
            Vector3 current_agent_position = transform.GetChild(i).transform.position;

            // update the list of agent positions
            agent_positions[i] = current_agent_position;
            agent_velocities[i] = agent.a_velocity;
            agent_personalities[i] = agent.identity;

        }

        // iterate the agent population to calculate and apply all forces
        for(int i=0; i<population_count; i++)
        {
            // current object --> specific child object --> specific child component
            Agent agent = transform.GetChild(i).GetComponent<Agent>();

            agent.CalculateAgentForces(agent_positions, agent_velocities, agent_personalities);
            agent.MoveAgent();
        }
    }

    public void UpdateIdentities()
    {
        for(int i=0; i < population_count; i++)
        {
            float personality_reveal = Random.value;
            Agent agent = transform.GetChild(i).GetComponent<Agent>();
            float probability = agent.probability_to_speak;
            if(probability >= personality_reveal)
            {
                int true_identity = agent.hidden_identity;
                //Debug.Log(true_identity);
                transform.GetChild(i).GetComponent<MeshRenderer>().material.color = personality_colors[true_identity-1];
            }
        }
    }
}
