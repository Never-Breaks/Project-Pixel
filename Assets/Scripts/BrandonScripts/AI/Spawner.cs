using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject AIToSpawn;
    [SerializeField] int AmountToSpawn;
    [SerializeField] int AmountSpawned;
    [SerializeField] int MaxAmountOnMap;
    [SerializeField] BasicAI[] currentlySpawnedAI;
    public bool WaveStarted;
    float waitTimer;
    private void Start()
    {     
        if (MaxAmountOnMap == 0)
        {
            MaxAmountOnMap = 1;
        }
        if (AmountToSpawn == 0)
        {
            AmountToSpawn = 5;
        }
        waitTimer = Random.Range(.1f, 4);
    }
    private void Update()
    {
        if (WaveStarted)
        {
            currentlySpawnedAI = FindObjectsOfType<BasicAI>();
            waitTimer -= Time.fixedDeltaTime;
        }
    }
    private void LateUpdate()
    {
        if(WaveStarted)
        {
            if (currentlySpawnedAI.Length < MaxAmountOnMap && waitTimer < 0)
            {
                SpawnAI();
                waitTimer = Random.Range(.1f, 4);
            }
        }
    }
    void SpawnAI()
    {
        GameObject temp = Instantiate(AIToSpawn, transform.position, Quaternion.identity);
    }
}
