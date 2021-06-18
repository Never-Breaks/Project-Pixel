using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    public Vector2 moveVal;
    public Vector2 lookVal;
    public bool isJumping = false; 

    // Start is called before the first frame update
    void Start()
    {
    }

    public void OnMove(InputValue value)
    {
        moveVal = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookVal = value.Get<Vector2>();
    }

    public void OnJump (InputValue value)
    {
        if (!isJumping)
        {
            isJumping = value.isPressed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isJumping)
        {
            CheckForGround();
        }
    }

    public bool CheckForGround()
    {
        //cast aray to see if we are in the air or on he ground

        return true;
    }
}
