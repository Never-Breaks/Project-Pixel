using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    //player speed
    public float speed = 10.0f;

    //movement input from keyboard or joystick
    public Vector2 moveVal;

    //look input from mouse or joystick
    public Vector2 lookVal;
    
    //trigger press value
    public float aimVal;

    //player input component
    public PlayerInput playerInput;

    //player states
    public enum PlayerState {Default, Aiming, Jumping}

    //current player state
    public PlayerState playerState;

    //character controller
    public CharacterController cc;

    //check to see if we are on the ground
    public bool isGrounded;

    //follow target's transform
    public Transform cameraLookAt;

    //main camera's transform
    public Transform mainCam;

    //new rotation for camera
    public Vector3 newCameraRot;

    //mouse sensitivity on x axis
    public float mouseSensitivityX;

    //mouse sensitivity on y axis
    public float mouseSensitivityY;

    //controller sensitivity on x axis
    public float controllerSensitivityX;

    //controller sensitivity on y axis
    public float controllerSensitivityY;

    //holds sensitivity on x axis of either controller or mouse depending on input
    public float sensitivityX;

    //holds sensitivity on y axis of either controller or mouse depending on input
    public float sensitivityY;

    //invert x axis on controller or mouse
    public bool viewInvertedX;

    //invert y axis on controller or mouse
    public bool viewInvertedY;

    //min clamp value for pitch rotation
    public float viewClampYmin;

    //max clamp value for pitch rotation
    public float viewClampYmax;

    //camera acceleration
    public Vector2 acceleration;

    //camera velocity
    public Vector2 velocity;

    //player rotation smoothing time
    public float turnSmoothTime = 0.05f;

    //rplayer otation smoothing  velocity
    public float turnSmoothVelocity;

    //The model
    public GameObject model;

    //the wanted velocity when moving the mouse or joystick
    Vector2 wantedVelocity;

    Vector3 direction;

    public float verticalVelocity;
    public float gravity = 14.0f;
    public float jumpForce = 10.0f;


    // Start is called before the first frame update
    void Start()
    {
        //get character controller component
        cc = GetComponent<CharacterController>();

        //set player state to default
        playerState = PlayerState.Default;

        //get PlayerInput component
        playerInput = GetComponent<PlayerInput>();

        //set the camera's rotation to the follow target's rotationn
        newCameraRot = cameraLookAt.localRotation.eulerAngles;

        //get model
        model = this.transform.GetChild(0).gameObject;

        //get main cam
        mainCam = this.transform.GetChild(1);

        //lock cursor to center of screen;
        Cursor.lockState = CursorLockMode.Locked;

        //if you are using a gamepad set sensititvity to controller's sensitivity settings
        if (playerInput.currentControlScheme == "GamePad")
        {
            sensitivityX = controllerSensitivityX;
            sensitivityY = controllerSensitivityY;
        }
        //otherwise use mouse's sensitivity settings
        else
        {
            sensitivityX = mouseSensitivityX;
            sensitivityY = mouseSensitivityY;
        }

    }

    public void OnMove(InputValue value)
    {
        moveVal = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookVal = value.Get<Vector2>();

        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            lookVal *= 0.5f;
            lookVal *= 0.1f;
        }
    }

    public void OnJump (InputValue value)
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;

           // cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

            //switch controls to jump mode
            playerInput.SwitchCurrentActionMap("PlayerJump");

            //switch playerstate to jumping
            playerState = PlayerState.Jumping;
        }
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

    public void OnControlsChanged()
    {
        if (playerInput.currentControlScheme == "GamePad")
        {
            sensitivityX = controllerSensitivityX;
            sensitivityY = controllerSensitivityY;
        }
        else
        {
            sensitivityX = mouseSensitivityX;
            sensitivityY = mouseSensitivityY;
        }
    }

    private void FixedUpdate()
    {
        switch (playerState)
        {
            case PlayerState.Default:

                #region Camera Control

                wantedVelocity = lookVal * new Vector2(mouseSensitivityX, mouseSensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                velocity = new Vector2(
                    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                //rotate the cameralook at with the cameras new rotation
                cameraLookAt.localRotation = Quaternion.Euler(newCameraRot);

                #endregion

                #region Player Movement 

                isGrounded = cc.isGrounded;

                if (isGrounded)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }           
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                }
                direction = new Vector3(moveVal.x, 0, moveVal.y).normalized;

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
                    //smooth angle for player rotation
                    float angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    moveDirection.y = verticalVelocity;
                    //move
                    cc.Move(moveDirection.normalized * speed * Time.deltaTime);
                }
                else
                {
                    direction = new Vector3(0, verticalVelocity, 0);
                    cc.Move(direction * Time.deltaTime);
                }

                #endregion                
                break;
            case PlayerState.Jumping:

                #region Player Movement

                verticalVelocity -= gravity * Time.deltaTime;

                direction = new Vector3(moveVal.x, 0, moveVal.y).normalized;

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
                    //smooth angle for player rotation
                    float angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    moveDirection.y = verticalVelocity;

                    //move
                    cc.Move(moveDirection.normalized * speed * Time.deltaTime);
                }
                else
                {
                    direction = new Vector3(0, verticalVelocity, 0);
                    cc.Move(direction * Time.deltaTime);
                }
                #endregion

                break;
            case PlayerState.Aiming:

                #region Camera Control

                wantedVelocity = lookVal * new Vector2(mouseSensitivityX, mouseSensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                velocity = new Vector2(
                    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                //rotate the cameralook at with the cameras new rotation
                cameraLookAt.localRotation = Quaternion.Euler(newCameraRot);

                #endregion


                Ray rayOrigin = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hitInfo;

                if(Physics.Raycast(rayOrigin, out hitInfo, 5f))
                {
                    if (hitInfo.collider != null)
                    {
                        Debug.Log("hitting");
                        Vector3 d = hitInfo.point - transform.position;
                        Debug.DrawRay(transform.position, d, Color.green);
                    }

                }

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

                //if the player has grounded
                //if (isGrounded)
                //{
                //    Debug.Log("yessir");
                //    //switch controls to default mode
                //    playerInput.SwitchCurrentActionMap("PlayerMove");

                //    //switch playerstate to default
                //    playerState = PlayerState.Default;
                //}
                break;

            case PlayerState.Aiming:

                #region Player Movement

                //player rotates with camera on yaw
                
                //player moves left right forward and backwards

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

    //public bool CheckForGround()
    //{
    //    Vector3 dir = Vector3.down;
    //    float dist = 5f;
    //    RaycastHit hit;

    //    if (Physics.Raycast(transform.position, dir, out hit, dist))
    //    {
    //        if(hit.collider.gameObject.CompareTag("Ground"))
    //        {
    //            //if ray hits ground return true
    //            return true;
    //        }
    //    }

    //    return false;
       
    //}
}
