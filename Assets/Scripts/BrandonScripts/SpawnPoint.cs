using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    //stupid name i know
    bool hasSpawnedPointed;
    GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPoints[0].position;
            hasSpawnedPointed = true;
        }
        else
            hasSpawnedPointed = false;
    }
    private void Update()
    {
        if(!hasSpawnedPointed)
        {
            if(player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            else
            {
                player.transform.position = spawnPoints[0].position;
                hasSpawnedPointed = true;
            }

        }
    }
    public void RespawnPlayer()
    {
        player.transform.position = spawnPoints[GetRandomPos()].position;
    }
    int GetRandomPos()
    {
        int randInt = Random.Range(0, spawnPoints.Length - 1);

        return randInt;
    }
}
