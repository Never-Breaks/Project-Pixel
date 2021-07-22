using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButton : MonoBehaviour
{
    Spawner[] spawners;
    void Start()
    {
        spawners = FindObjectsOfType<Spawner>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            print("Shesh");
            foreach(Spawner s in spawners)
            {
                s.WaveStarted = true;
            }
        }
    }
}
