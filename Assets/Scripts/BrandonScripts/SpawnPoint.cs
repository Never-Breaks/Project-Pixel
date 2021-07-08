using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject[] spawnPoints;
    GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = spawnPoints[0].transform.position;
    }


    public void RespawnPlayer()
    {
        player.transform.position = spawnPoints[GetRandomPos()].transform.position;
    }

    int GetRandomPos()
    {
        int randInt = Random.Range(0, spawnPoints.Length - 1);

        return randInt;
    }
}
