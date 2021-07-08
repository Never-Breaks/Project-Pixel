using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    Transform PlayerPos;
    [SerializeField] int health;
    [SerializeField] int damage;
    public NavMeshAgent agent;
    public Animator anim;
    
    void Start()
    {
        PlayerPos = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            if (Vector3.Distance(PlayerPos.position, transform.position) > 2)
            {
                //Hunts the player
                agent.SetDestination(PlayerPos.position);
            }
            else
            {
                //stops the AI if it's next to the player
                agent.SetDestination(transform.position);

                //attack player here
                //DoDamage();
            }
        }
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
    }
    public void DoDamage()
    {
        //player.health -= damage;
    }
    void Death()
    {
        agent.enabled = false;
        //we can do an animation here then destroy the gameobject.
        //but for right now it just gets destroyed
        Destroy(gameObject);
    }
}
