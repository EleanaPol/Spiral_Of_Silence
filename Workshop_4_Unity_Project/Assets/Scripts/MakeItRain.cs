using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeItRain : MonoBehaviour
{
    public GameObject drop_prefab;
    public BoxCollider rainBox;

    
    public int drop_max_count;
    public float drop_interval;

    private int activeRainDrops;
    private float timer;
    private Vector3 drop_min;
    private Vector3 drop_max;
    
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        drop_min = rainBox.bounds.min;
        drop_max = rainBox.bounds.max;
        rainBox.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= drop_interval)
        {
            Rain();
            timer = 0;
        }

        timer += Time.deltaTime;
    }

    private void Rain()
    {
        if (activeRainDrops < drop_max_count)
        {
           // get random position
           float x = Random.Range(drop_min.x, drop_max.x);
           float y = drop_max.y;
           float z = Random.Range(drop_min.z, drop_max.z);
           
           var new_pos = new Vector3(x,y,z);
           var drop = Instantiate(drop_prefab, new_pos, transform.rotation, transform);
           activeRainDrops++;
        }
        
    }

    private void CheckLifetime()
    {
        var drop_pre_check_count = activeRainDrops;
        for (int i = 0; i < activeRainDrops; i++)
        {
            
        }
    }
    
}
