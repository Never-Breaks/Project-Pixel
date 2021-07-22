using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEnemies : MonoBehaviour
{
    Spawner[] spawners;
    void Start()
    {
        spawners = FindObjectsOfType<Spawner>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            BasicAI[] arrayOfEnemies = FindObjectsOfType<BasicAI>();
            foreach(BasicAI ai in arrayOfEnemies)
            {
                Destroy(ai.gameObject);
            }
            foreach (Spawner s in spawners)
            {
                s.WaveStarted = false;
            }
        }
    }
}
