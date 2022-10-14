using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent_007 : MonoBehaviour
{
    public float step_length;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // =============== random
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-0.5f, 0.5f);
        float z = Random.Range(-1f, 1f);

        Vector3 new_position = new Vector3(x, y, z) * step_length;

        transform.position = transform.position + new_position;
    }
}
