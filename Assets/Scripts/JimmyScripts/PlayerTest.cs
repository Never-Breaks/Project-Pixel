using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    public float speed = 10.0f;
    public Vector2 moveVal;
    public Vector2 lookVal;
    public float aimVal;
    public PlayerInput playerInput;
    public enum PlayerState {Default, Aiming, Jumping}
    public PlayerState playerState;
    public CharacterController cc;
    public bool isGrounded;

    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    public Transform cameraLookAt;
    public Vector3 newCameraRot;
    public float sensitivityX;
    public float sensitivityY;
    public bool viewInvertedX;
    public bool viewInvertedY;
    public float viewClampYmin;
    public float viewClampYmax;




    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerState = PlayerState.Default;
        playerInput = GetComponent<PlayerInput>();
        newCameraRot = cameraLookAt.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMove(InputValue value)
    {
        moveVal = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookVal = value.Get<Vector2>();
        //lookVal *= 0.5f;
        //lookVal *= 0.1f;
        //Debug.Log(lookVal);
    }

    public void OnJump (InputValue value)
    {
        //add force on rigid body to make player jump

        //switch controls to jump mode
        playerInput.SwitchCurrentActionMap("PlayerJump");

        //switch playerstate to jumping
        playerState = PlayerState.Jumping;
    }

    public void OnAim(InputValue value)
    {
        //update aim value
        aimVal = value.Get<float>();
    }

    public void OnFire(InputValue value)
    {
        Debug.Log("fire");
    }

    private void FixedUpdate()
    {
        switch(playerState)
        {
            case PlayerState.Default:

                //#region Camera Control
                ////pitch rotation clamped to a min and max value
                //newCameraRot.x += sensitivityY * lookVal.y * Time.deltaTime;
                //newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                ////yaw rotation 
                //newCameraRot.y += sensitivityX * lookVal.x * Time.deltaTime;

                //cameraLookAt.localRotation = Quaternion.Euler(newCameraRot);
                //#endregion

                #region Player Movement

                Vector3 direction = new Vector3(moveVal.x, 0, moveVal.y);
                Vector3 movement = transform.TransformDirection(direction) * speed;
                isGrounded = cc.SimpleMove(movement);

                #endregion                
                break;
            case PlayerState.Jumping:
                #region Player Movement

                #endregion
                
                break;
            case PlayerState.Aiming:
                break;
        }
    }

    void Update()
    {
       // Debug.Log(playerInput.currentActionMap);

        switch (playerState)
        {
            case PlayerState.Default:
               // Debug.Log(playerInput.currentActionMap);

            

                //if the aim button is fuly pressed
                if (aimVal >= 1f)
                {
                    //switch cinemachine cam to aiming cam

                    //switch controls to aim mode
                    playerInput.SwitchCurrentActionMap("PlayerAim");

                    //switch playerstate to aiming
                    playerState = PlayerState.Aiming;
                }
                break;

            case PlayerState.Jumping:

               
                bool isGrounded = true;
                Debug.Log(isGrounded);

                //if the player has grounded
                if (isGrounded)
                {
                    //switch controls to default mode
                    playerInput.SwitchCurrentActionMap("PlayerMove");

                    //switch playerstate to default
                    playerState = PlayerState.Default;
                }
                break;

            case PlayerState.Aiming:

                #region Player Movement

                #endregion

                //if the aim button is released
                if (aimVal < 0.5f)
                {
                    //switch cinemachine cam to default cam

                    //switch controls to default mode
                    playerInput.SwitchCurrentActionMap("PlayerMove");

                    //switch playerstate to default
                    playerState = PlayerState.Default;
                }
                break;       
        }
    }

    public bool CheckForGround()
    {
        //cast a ray to see if we are in the air or on the ground

        //if ray hits ground return true
        return true;
    }
}
