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
    public GameObject defaultCameraFollowTarget;

    public GameObject aimCameraFollowTarget;

    //main camera's transform
    public GameObject mainCam;

    public GameObject aimCam;

    public GameObject defaultCam;

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

    //movement direction normalized
    Vector3 direction;

    Animator anim;

    public float verticalVelocity;
    public float gravity = 14.0f;
    public float jumpForce = 10.0f;
    public float groundCheckWaitTime = 0.25f;
    public float timer = 0f;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.transform.childCount);
        //get character controller component
        cc = GetComponent<CharacterController>();

        //set player state to default
        playerState = PlayerState.Default;

        //get PlayerInput component
        playerInput = GetComponent<PlayerInput>();

        //get model
        model = this.gameObject.transform.GetChild(0).gameObject;

        //get main cam
        mainCam = this.transform.GetChild(1).gameObject;

        defaultCam = this.gameObject.transform.GetChild(2).gameObject;

        aimCam = this.gameObject.transform.GetChild(3).gameObject;

        defaultCameraFollowTarget = this.gameObject.transform.GetChild(4).gameObject;

        aimCameraFollowTarget = this.gameObject.transform.GetChild(5).gameObject;

        //set the camera's rotation to the follow target's rotationn
        newCameraRot = defaultCameraFollowTarget.transform.localRotation.eulerAngles;

        anim = GetComponent<Animator>();

        //lock cursor to center of screen;
        Cursor.lockState = CursorLockMode.Locked;
        aimCam.gameObject.SetActive(false);

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
        if (playerInput.currentControlScheme == "GamePad")
        {
            moveVal = value.Get<Vector2>();
        }
        else if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            moveVal = value.Get<Vector2>();
        }
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

            timer = groundCheckWaitTime;

            //switch playerstate to jumping
            playerState = PlayerState.Jumping;

            ////switch controls to jump mode
            //playerInput.SwitchCurrentActionMap("PlayerJump");
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

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
                    //smooth angle for player rotation
                    float angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    Vector3 move = moveDirection.normalized * speed;
                    //move
                    cc.Move(new Vector3(move.x,verticalVelocity,move.z) * Time.deltaTime);
                }
                else
                {
                    direction = new Vector3(0, verticalVelocity, 0);
                    cc.Move(direction  * Time.deltaTime);
                }

                #endregion                
                break;
            case PlayerState.Jumping:

                #region Player Movement

                isGrounded = cc.isGrounded;

                verticalVelocity -= gravity * Time.deltaTime;

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
                    //smooth angle for player rotation
                    float angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    Vector3 move = moveDirection.normalized * speed;

                    //move
                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                }
                else
                {
                    direction = new Vector3(0, verticalVelocity, 0);
                    cc.Move(direction * Time.deltaTime);
                }
                #endregion

                break;
            case PlayerState.Aiming: 

                Ray rayOrigin = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hitInfo;

                if(Physics.Raycast(rayOrigin, out hitInfo, 100f))
                {

                    if (hitInfo.collider != null)
                    {
                        Debug.Log("hitting");
                        Vector3 d = hitInfo.point - transform.position;
                        Debug.DrawRay(transform.position, d, Color.green);
                    }

                }

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

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
                    //smooth angle for player rotation
                    float angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    Vector3 move = moveDirection.normalized * speed;
                    //move
                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                }
                else
                {
                    direction = new Vector3(0, verticalVelocity, 0);
                    cc.Move(direction * Time.deltaTime);
                }

                #endregion   

                break;
        }
    }

    void Update()
    {
        // Debug.Log(playerInput.currentActionMap);

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
                defaultCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);

                #endregion


                // Debug.Log(playerInput.currentActionMap);
                direction = new Vector3(moveVal.x, 0, moveVal.y);
                Debug.Log(direction);

                #region Animation
                if (playerInput.currentControlScheme == "GamePad")
                {
                    //top right movement on joystick
                    if (direction.x > 0f && direction.z > 0f)
                    {
                        if (direction.x > direction.z)
                        {
                            if (direction.x > 0.7f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                            }
                        }
                        else if (direction.z > direction.x)
                        {
                            if (direction.z > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                            }
                        }
                    }

                    //top left movement on joystick
                    else if (direction.x < 0f && direction.z > 0f)
                    {
                        if (-direction.x > direction.z)
                        {
                            if (-direction.x > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                            }
                        }
                        else if (direction.z > -direction.x)
                        {
                            if (direction.z > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                            }
                        }
                    }

                    //bottom right movement on joystick
                    else if (direction.x > 0f && direction.z < 0f)
                    {
                        if (direction.x > -direction.z)
                        {
                            if (direction.x > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                            }
                        }
                        else if (-direction.z > direction.x)
                        {
                            if (-direction.z > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                            }
                        }
                    }

                    //bottom left movement on joystick
                    else if (direction.x < 0f && direction.z < 0f)
                    {
                        if (-direction.x > -direction.z)
                        {
                            if (-direction.x > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                            }
                        }
                        else if (-direction.z > -direction.x)
                        {
                            if (-direction.z > 0.5f)
                            {
                                anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            }
                            else
                            {
                                anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                            }
                        }
                    }

                    //no joystick movement
                    else if (direction.x == 0 && direction.z == 0)
                    {
                        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                    }
                }
                else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                {
                    //left and right on keyboard
                    if (direction.x > 0f && direction.z == 0)
                    {
                        anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                    }
                    else if (direction.x < 0f && direction.z == 0)
                    {
                        anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                    }

                    //forwards and backwards on keyboard
                    else if (direction.z > 0f && direction.x == 0)
                    {
                        anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                    }
                    else if (direction.z < 0f && direction.x == 0)
                    {
                        anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                    }
                    
                    //top right movement on keyboard
                    else if (direction.x > 0 && direction.z > 0)
                    {
                        anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                    }

                    //bottom right movement on keyboard
                    else if (direction.x > 0 && direction.z < 0)
                    {
                        anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                    }

                    //top left movement on keyboard
                    else if (direction.x < 0 && direction.z > 0)
                    {
                        anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                    }

                    //bottom left movement on keyboard
                    else if (direction.x < 0 && direction.z < 0)
                    {
                        anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                    }

                    //no movement on keyboard
                    else if (direction.x == 0 && direction.z == 0)
                    {
                        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                    }
                }

                #endregion

                //if the aim button is fuly pressed
                if (aimVal >= 1f)
                {
                    //switch cinemachine cam to aiming cam
                    defaultCam.gameObject.SetActive(false);
                    aimCam.gameObject.SetActive(true);

                    aimCameraFollowTarget.transform.localRotation = defaultCameraFollowTarget.transform.localRotation;

                    //switch playerstate to aiming
                    playerState = PlayerState.Aiming;
                }
                break;

            case PlayerState.Jumping:

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
                defaultCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);

                #endregion


                direction = new Vector3(moveVal.x, 0, moveVal.y);

                timer -= Time.deltaTime;

               if(timer <= 0)
                {
                    timer = 0;
                    if (isGrounded)
                    {
                        //switch playerstate to default
                        playerState = PlayerState.Default;

                        ////switch controls to default mode
                        //playerInput.SwitchCurrentActionMap("PlayerMove");
                    }
                }
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
                aimCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);

                #endregion

                direction = new Vector3(moveVal.x, 0, moveVal.y);
                Debug.Log(direction);

                //if the aim button is released
                if (aimVal < 0.5f)
                {
                    //switch cinemachine cam to default cam
                    defaultCam.gameObject.SetActive(true);
                    aimCam.gameObject.SetActive(false);

                    ////switch controls to default mode
                    //playerInput.SwitchCurrentActionMap("PlayerMove");

                    //switch playerstate to default
                    playerState = PlayerState.Default;
                }
                break;       
        }
    }
}
