using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    //player speed
    public float speed;

    public float defaultWalkSpeed = 5f;

    public float aimWalkSpeed = 2;

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

    public float mouseAimSensitivityX;

    //mouse sensitivity on y axis
    public float mouseSensitivityY;

    public float mouseAimSensitivityY;

    //controller sensitivity on x axis
    public float controllerSensitivityX;

    public float controllerAimSensitivityX;

    //controller sensitivity on y axis
    public float controllerSensitivityY;

    public float controllerAimSensitivityY;

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

    public float attackSmoothTime = 0.01f;

    //rplayer otation smoothing  velocity
    public float turnSmoothVelocity;

    //The model
    public GameObject model;

    //the wanted velocity when moving the mouse or joystick
    Vector2 wantedVelocity;

    //movement direction normalized
    Vector3 direction;

    Animator anim;

    public float attackVal;

    public float verticalVelocity;
    public float gravity = 14.0f;
    public float jumpForce = 10.0f;
    public float groundCheckWaitTime = 0.25f;
    public float timer = 0f;

    public bool isAttacking = false;

    public bool isJumping = false;



    // Start is called before the first frame update
    void Start()
    {
        speed = defaultWalkSpeed;

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

        defaultCam.SetActive(true);
        aimCam.SetActive(false);

        //if you are using a gamepad set sensititvity to controller's sensitivity settings
        if (playerInput.currentControlScheme == "GamePad")
        {
            sensitivityX = controllerSensitivityX;
            sensitivityY = controllerSensitivityY;           
        }
        //otherwise use mouse's sensitivity settings
        else if (playerInput.currentControlScheme == "Keyboard&Mouse")
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
            lookVal *= 1;

            Debug.Log(lookVal);
        }
    }

    public void OnJump (InputValue value)
    {
        #region Jumping
        if (isGrounded)
        {
            //add jump force to our vertical velocity
            verticalVelocity = jumpForce;

            //set timer to ground check waiting time
            timer = groundCheckWaitTime;

            //switch playerstate to jumping
            playerState = PlayerState.Jumping;
        }
        #endregion
    }

    public void OnAim(InputValue value)
    {
        //update aim value
        aimVal = value.Get<float>();
    }

    public void OnFire(InputValue value)
    {
        //update attackval
        attackVal = value.Get<float>();
    }

    public void OnControlsChanged()
    {
        #region Sensitivity Update
        if (playerInput.currentControlScheme == "GamePad")
        {
            sensitivityX = controllerSensitivityX;
            sensitivityY = controllerSensitivityY;  
        }
        else if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {   
            sensitivityX = mouseSensitivityX;
            sensitivityY = mouseSensitivityY;     
        }
        #endregion
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

                    //rotation of model when attacking or not attacking while moving
                    if (!isAttacking)
                    {
                        //rotate model by smooth angle so follow target doesnt also rotate
                        model.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    else if (isAttacking)
                    {

                        //smooth angle for player rotation
                        float defaultRotateAngleSmooth = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, Camera.main.transform.localEulerAngles.y, ref turnSmoothVelocity, attackSmoothTime);

                        //rotate model by smooth angle so follow target doesnt also rotate
                        model.transform.rotation = Quaternion.Euler(0f, defaultRotateAngleSmooth, 0f);
                    }

                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    Vector3 move = moveDirection.normalized * speed;
                    //move
                    cc.Move(new Vector3(move.x,verticalVelocity,move.z) * Time.deltaTime);
                }
                else
                {
                    //rotation of model when attacking or not attacking while moving
                    if (isAttacking)
                    {
                        //smooth angle for player rotation
                        float defaultRotateAngleSmooth = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, Camera.main.transform.localEulerAngles.y, ref turnSmoothVelocity, attackSmoothTime);

                        //rotate model by smooth angle so follow target doesnt also rotate
                        model.transform.rotation = Quaternion.Euler(0f, defaultRotateAngleSmooth, 0f);
                    }

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

                #region Attack Raycasting
                if (playerInput.currentControlScheme == "Keyboard&Mouse")
                {
                    if (isAttacking)
                    {
                        Ray rayOrigin = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                        RaycastHit hitInfo;

                        if (Physics.Raycast(rayOrigin, out hitInfo, 100f))
                        {

                            if (hitInfo.collider != null)
                            {
                                Debug.Log("hitting");
                                Vector3 d = hitInfo.point - transform.position;
                                Debug.DrawRay(transform.position, d, Color.green);
                            }

                        }
                    }
                }
                else if (playerInput.currentControlScheme == "GamePad")
                {
                    if (isAttacking)
                    {
                        // Create a vector at the center of our camera's viewport
                        Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

                        // Declare a raycast hit to store information about what our raycast has hit
                        RaycastHit hit;

                        // Check if our raycast has hit anything
                        if (Physics.Raycast(rayOrigin, mainCam.transform.forward, out hit, 100f))
                        {
                            Vector3 d = hit.point - transform.position;

                            // Rest of your code - what to do when raycast hits anything
                            Debug.DrawRay(transform.position, d, Color.black);
                        }
                    
                    }
                }
                #endregion

                #region Player Rotation
                //smooth angle for player rotation
                float aimRotateAngleSmooth = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, Camera.main.transform.localEulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);

                //rotate model by smooth angle so follow target doesnt also rotate
                model.transform.rotation = Quaternion.Euler(0f, aimRotateAngleSmooth, 0f);
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

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
                    
                    //turn rotation to direction. direction you want to move in
                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //vector 3 of normalized movement input times speed
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

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

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

                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                #region Aim Check
                //if the aim button is fuly pressed
                if (aimVal >= 1f)
                {
                    speed = aimWalkSpeed;

                    if (playerInput.currentControlScheme == "GamePad")
                    {
                        sensitivityX = controllerAimSensitivityX;
                        sensitivityY = controllerAimSensitivityY;
                    }
                    else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                    {
                        sensitivityX = mouseAimSensitivityX;
                        sensitivityY = mouseAimSensitivityY;
                    }

                    //switch cinemachine cam to aiming cam
                    defaultCam.SetActive(false);
                    aimCam.SetActive(true);

                    aimCameraFollowTarget.transform.localRotation = defaultCameraFollowTarget.transform.localRotation;

                    //switch playerstate to aiming
                    playerState = PlayerState.Aiming;
                }
                #endregion

                #region Player Attack

                if (attackVal >= 1f)
                {
                    isAttacking = true;                    
                }
                else if (attackVal < 0.5f)
                {
                    isAttacking = false;
                }

                #endregion

                break;

            case PlayerState.Jumping:

                #region Camera Control

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

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

                //update input direction
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                #region Jump Grounded Check Delay
                //decrease timer
                timer -= Time.deltaTime;

                //slight delay before checking if we are on the ground
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
                #endregion

                break;

            case PlayerState.Aiming:

                #region Camera Control

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

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

                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                #region Aim Check
                //if the aim button is released
                if (aimVal < 0.5f)
                {
                    speed = defaultWalkSpeed;

                    if (playerInput.currentControlScheme == "GamePad")
                    {
                        sensitivityX = controllerSensitivityX;
                        sensitivityY = controllerSensitivityY;
                    }
                    else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                    {
                        sensitivityX = mouseSensitivityX;
                        sensitivityY = mouseSensitivityY;
                    }

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    ////switch controls to default mode
                    //playerInput.SwitchCurrentActionMap("PlayerMove");

                    //switch playerstate to default
                    playerState = PlayerState.Default;
                }
                #endregion

                #region Player Attack

                if (attackVal >= 1f)
                {
                    isAttacking = true;
                }
                else if (attackVal < 0.5f)
                {
                    isAttacking = false;
                }
                #endregion

                break;       
        }
    }
}
