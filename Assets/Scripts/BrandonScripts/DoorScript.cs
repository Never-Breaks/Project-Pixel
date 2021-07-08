using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class DoorScript : MonoBehaviour
{
    /*
    EveryThings here in Code but i need to figure out how to use new unity 
    Input system
     */

    public int DoorCost = 100;
    public bool IsBought = false;
    [SerializeField] float DistanceAwayToBuy;
    [SerializeField] float TempDistanceToMoveWhenBought;
    float yValueAfterMoving;
    //this will later need to be changed to an array for muliplayer
    GameObject player;
    [SerializeField] Text DoorText;
    bool Interacting;
    public float QValue;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        DoorText = GameObject.Find("Door Text").GetComponent<Text>();
        yValueAfterMoving = transform.position.y - TempDistanceToMoveWhenBought;
        DoorText.text = "Press Q To Buy Door $" + DoorCost + ".";
    }
    void Update()
    {
        QValue = player.GetComponent<PlayerTest>().QValue;

        if (QValue >= 1f)
            Interacting = true;
        else if (QValue < 0.5f)
            Interacting = false;

        if (CheckForDistance())
        {
            if (!IsBought)
            {
                DoorText.enabled = true;
                if (Interacting)
                {
                    BuyDoor();
                }
            }
        }
        if (!CheckForDistance() || IsBought)
            DoorText.enabled = false;
        if (IsBought && transform.position.y > yValueAfterMoving)
            PlayDoorAnimation();
    }
    void BuyDoor()
    {
        //if (player.GetComponent<PlayerScript>().money >= DoorCost)
        // {

        ExpScript.exp += 2;

        IsBought = true;
        // player.GetComponent<PlayerScript>().money -= DoorCost;
        // }
        //  else
        //   print("Not Enough Money");


        //I don't know if we'd want to have like player.GetComponent<PlayerScript>().exp + amountOfExpGained;
    }
    bool CheckForDistance()
    {
        if (Vector3.Distance(gameObject.transform.position, player.transform.position) < DistanceAwayToBuy)
            return true;
        else
            return false;
    }
    void PlayDoorAnimation()
    {
        //right now i just have it going down seeing theirs no animation yet
        transform.position -= (new Vector3(0, 1, 0) * Time.deltaTime);
    }

}
