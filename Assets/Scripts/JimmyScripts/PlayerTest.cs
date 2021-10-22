using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    //player speed
    public float speed;

    public float walkSpeed = 2.5f;

    public float runSpeed = 5;

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
    public enum PlayerState {Default, Aiming, Jumping, Melee, Spell}

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
    public float turnSmoothTime;

    //default attack smoothing time
    public float defaultAttackRotationSmoothTime;

    //aim smoothing time
    public float aimRotationSmoothTime;

    //rplayer otation smoothing  velocity
    public float turnSmoothVelocity;

    //The model
    public GameObject model;

    //the wanted velocity when moving the mouse or joystick
    Vector2 wantedVelocity;

    //movement direction normalized
    Vector3 direction;

    //animator
    Animator anim;

    //attack button value
    public float attackVal;

    public float jumpVal;

    public float runVal;

    //velocity on the y for when we jump and fall
    public float verticalVelocity;

    public float gravity = 14.0f;

    public float slopeForce;

    public float slopeForceRayLength;

    public float jumpForce = 10.0f;

    public float groundCheckWaitTime = 0.25f;

    public float timer = 0f;

    public Transform attackRaycastTransform;

    public bool isAttacking = false;

    public bool isJumping = false;

    float targetAngle;

    Vector3 moveDirection;

    Vector3 move;

    float angle;

    float attackDelay;

    bool canAttack = false;
    
    [HideInInspector]
    public float QValue;

    // Start is called before the first frame update
    void Start()
    {
        //set speed to walking speed
        speed = walkSpeed;

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

        //get default camera
        defaultCam = this.gameObject.transform.GetChild(2).gameObject;

        //get aiming camera
        aimCam = this.gameObject.transform.GetChild(3).gameObject;

        //get follow target for default camera
        defaultCameraFollowTarget = this.gameObject.transform.GetChild(4).gameObject;

        //get follow target for aim camera
        aimCameraFollowTarget = this.gameObject.transform.GetChild(5).gameObject;

        //attack raycast transform
        attackRaycastTransform = model.transform.GetChild(0).gameObject.transform;

        //set the camera's rotation to the follow target's rotationn
        newCameraRot = defaultCameraFollowTarget.transform.localRotation.eulerAngles;

        //get animator component
        anim = GetComponent<Animator>();

        //lock cursor to center of screen;
        Cursor.lockState = CursorLockMode.Locked;

        //turn default camera on
        defaultCam.SetActive(true);

        //turn aim camera on
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
    public void OnInteract(InputValue value)
    {
        QValue = value.Get<float>();
        print("QQQQQQ");
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
        //uppdate jump button balue
        jumpVal = value.Get<float>();
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

    public void OnRun(InputValue value)
    {
        //update attackval
        runVal = value.Get<float>();
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
                //if (direction.magnitude >= 0.1f)
                //{
                //    //move
                //    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.fixedDeltaTime);

                //    if (OnSlope())
                //    {
                //        cc.Move(2 * cc.height * slopeForce * Time.fixedDeltaTime * Vector3.down);
                //    }

                //}
                //else
                //{
                //    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime);
                //}

                #endregion

                break;
            case PlayerState.Jumping:

                #region Player Movement

                //if (direction.magnitude >= 0.1f)
                //{
                //    //move
                //    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.fixedDeltaTime);
                //}
                //else
                //{
                //    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime);
                //}
                #endregion

                break;
            case PlayerState.Aiming:

                #region Player Movement 

                //if (direction.magnitude >= 0.1f)
                //{
                //    //move
                //    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                //}
                //else
                //{
                //    //move
                //    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime);
                //}
                #endregion   

                break;

            case PlayerState.Melee:

                #region Attack Raycasting

                if (playerInput.currentControlScheme == "Keyboard&Mouse")
                {
                    if (isAttacking && canAttack)
                    {
                        canAttack = false;
                        attackDelay = 0.3f;

                        // Bit shift the index of the layer (7) to get a bit mask
                        int layerMask = 1 << 7;

                        // This would cast rays only against colliders in layer 7.
                        // But instead we want to collide against everything except layer 7. The ~ operator does this, it inverts a bitmask.
                        layerMask = ~layerMask;

                        //ray from camera to cursor
                        Ray rayOrigin = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                        // Does the ray intersect any objects excluding the player layer
                        if (Physics.Raycast(rayOrigin, out RaycastHit hitInfo, 5f, layerMask))
                        {
                            if (hitInfo.collider != null)
                            {
                                Debug.Log(hitInfo.collider.gameObject);
                                Vector3 d = hitInfo.point - attackRaycastTransform.position;
                                Debug.DrawRay(attackRaycastTransform.position, d, Color.black);
                               // Destroy(hitInfo.collider.gameObject);
                            }
                        }
                    }
                }
                else if (playerInput.currentControlScheme == "GamePad")
                {
                    if (isAttacking && canAttack)
                    {
                        canAttack = false;
                        attackDelay = 0.3f;

                        //delay when releasing attack or jumping
                       // timer = 0.4f;

                        // Bit shift the index of the layer (7) to get a bit mask
                        int layerMask = 1 << 7;

                        // This would cast rays only against colliders in layer 7.
                        // But instead we want to collide against everything except layer 7. The ~ operator does this, it inverts a bitmask.
                        layerMask = ~layerMask;

                        // Create a vector at the center of our camera's viewport
                        Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

                        // Does the ray intersect any objects excluding the player layer
                        if (Physics.Raycast(rayOrigin, mainCam.transform.forward, out RaycastHit hit, 5f, layerMask))
                        {
                            Debug.Log(hit.collider.gameObject);
                            Vector3 d = hit.point - attackRaycastTransform.position;
                            Debug.DrawRay(attackRaycastTransform.position, d, Color.black);
                        }
                    }
                }
                #endregion

                #region Player Movement 
                //if (direction.magnitude >= 0.1f)
                //{
                //    //move
                //    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.fixedDeltaTime);

                //    if (OnSlope())
                //    {
                //        cc.Move(2 * cc.height * slopeForce * Time.fixedDeltaTime * Vector3.down);
                //    }

                //}
                //else
                //{
                //    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime);
                //}

                #endregion

                break;

            case PlayerState.Spell:

                #region Attack Raycasting
                if (playerInput.currentControlScheme == "Keyboard&Mouse")
                {
                    if (isAttacking)
                    {
                        Ray rayOrigin = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                        RaycastHit hitInfo;

                        if (Physics.Raycast(rayOrigin, out hitInfo, 10f))
                        {

                            if (hitInfo.collider != null)
                            {
                                Debug.Log(hitInfo.collider.gameObject);
                                Vector3 d = hitInfo.point - attackRaycastTransform.position;
                                Debug.DrawRay(attackRaycastTransform.position, d, Color.green);
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
                        if (Physics.Raycast(rayOrigin, mainCam.transform.forward, out hit, 10f))
                        {
                            Debug.Log(hit.collider.gameObject);

                            Vector3 d = hit.point - attackRaycastTransform.position;

                            // Rest of your code - what to do when raycast hits anything
                            Debug.DrawRay(attackRaycastTransform.position, d, Color.black);
                        }

                    }
                }
                #endregion

                #region Player Movement 

                //if (direction.magnitude >= 0.1f)
                //{
                //    //move
                //    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                //}
                //else
                //{
                //    //move
                //    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime);
                //}
                #endregion

                break;
        }
    }

    private void LateUpdate()
    {
        switch (playerState)
        {
            case PlayerState.Default:

                #region Camera Movement

                //rotate the cameralook at with the cameras new rotation
                defaultCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);
                #endregion

                break;
            case PlayerState.Jumping:

                #region Camera Movement

                //rotate the cameralook at with the cameras new rotation
                defaultCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);

                #endregion

                break;
            case PlayerState.Aiming:

                #region Camera Movement

                //rotate the cameralook at with the cameras new rotation
                aimCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);
                #endregion

                break;

            case PlayerState.Melee:

                #region Camera Movement

                //rotate the cameralook at with the cameras new rotation
                defaultCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);
                #endregion

                break;

            case PlayerState.Spell:

                #region Camera Movement

                //rotate the cameralook at with the cameras new rotation
                aimCameraFollowTarget.transform.localRotation = Quaternion.Euler(newCameraRot);
                #endregion

                break;
        }
    }

    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Default:
              
                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                //Ground check
                isGrounded = cc.isGrounded;

                #region Update Gravity
                if (isGrounded)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                }
                #endregion                

                //we are falling
                if (!isGrounded)
                {
                    //play falling animation
                    anim.SetBool("isFalling", true);

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    //break out of the loop
                    break;
                }

                #region Animation
                if (playerInput.currentControlScheme == "GamePad")
                {
                    //top right movement on joystick
                    if (direction.x > 0f && direction.z > 0f)
                    {
                        if (direction.x > direction.z)
                        {
                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }


                            //if (direction.x > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;
                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;
                            //}
                        }
                        else if (direction.z > direction.x)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (direction.z > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                    }

                    //top left movement on joystick
                    else if (direction.x < 0f && direction.z > 0f)
                    {
                        if (-direction.x > direction.z)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }
                            //if (-direction.x > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                        else if (direction.z > -direction.x)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (direction.z > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                    }

                    //bottom right movement on joystick
                    else if (direction.x > 0f && direction.z < 0f)
                    {
                        if (direction.x > -direction.z)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (direction.x > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                        else if (-direction.z > direction.x)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (-direction.z > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                    }

                    //bottom left movement on joystick
                    else if (direction.x < 0f && direction.z < 0f)
                    {
                        if (-direction.x > -direction.z)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (-direction.x > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
                        }
                        else if (-direction.z > -direction.x)
                        {

                            if (runVal < 0.5f)
                            {
                                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                                speed = walkSpeed;
                            }
                            else if (runVal >= 1f)
                            {
                                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                                speed = runSpeed;
                            }

                            //if (-direction.z > 0.5f)
                            //{
                            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                            //    speed = runSpeed;

                            //}
                            //else
                            //{
                            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                            //    speed = walkSpeed;

                            //}
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
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }
                    else if (direction.x < 0f && direction.z == 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //forwards and backwards on keyboard
                    else if (direction.z > 0f && direction.x == 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }
                    else if (direction.z < 0f && direction.x == 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //top right movement on keyboard
                    else if (direction.x > 0 && direction.z > 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //bottom right movement on keyboard
                    else if (direction.x > 0 && direction.z < 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //top left movement on keyboard
                    else if (direction.x < 0 && direction.z > 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //bottom left movement on keyboard
                    else if (direction.x < 0 && direction.z < 0)
                    {
                        if (runVal < 0.5f)
                        {
                            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                            speed = walkSpeed;
                        }
                        else if (runVal >= 1f)
                        {
                            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                            speed = runSpeed;
                        }

                        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                        //speed = runSpeed;
                    }

                    //no movement on keyboard
                    else if (direction.x == 0 && direction.z == 0)
                    {
                        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                        speed = runSpeed;
                    }
                }

                #endregion

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

                    anim.SetBool("isAiming", true);

                    //switch playerstate to aiming
                    playerState = PlayerState.Aiming;

                    //break out of the loop 
                    break;
                }
                #endregion

                #region Jump Check
                if (jumpVal >= 1f && isGrounded)
                {
                    //set the jumping flag to true
                    isJumping = true;                 
                }

                if(isJumping)
                {
                    //add jump force to our vertical velocity
                    verticalVelocity = jumpForce;

                    //set timer to ground check waiting time
                    timer = groundCheckWaitTime;

                    anim.SetBool("isJumping", true);

                    //reduce speed when jumping
                    //speed = walkSpeed;

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    //break out of the loop
                    break;
                }
                #endregion

                #region Player Attack Check

                if (attackVal >= 1f)
                {
                    isAttacking = true;                    
                }
                else if (attackVal < 0.5f)
                {
                    isAttacking = false;
                }

                if(isAttacking)
                {
                    anim.SetBool("isDefaultAttack", true);
                    
                    //delay in between swing
                    attackDelay = 0.3f;

                    //delay when releasing attack or jumping
                    timer = 0.35f;

                    playerState = PlayerState.Melee;

                    break;
                }

                #endregion

                #region Camera Update

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                //velocity = new Vector2(
                //    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                //    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                velocity = new Vector2(wantedVelocity.x, wantedVelocity.y);

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                #endregion

                #region Update Player Movement Variables
                if (direction.magnitude >= 0.1f)
                {
                    if (!isAttacking)
                    {
                        //get angle to see how much we need to rotate on y axis from moving relative to camera
                        targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                        //turn rotation to direction. direction you want to move in
                        moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                        //normalized movement vector
                        move = moveDirection.normalized * speed;

                        //smoothed angle for player rotation
                        angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                        //rotate model by smooth angle so follow target doesnt also rotate
                        model.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    //else if (isAttacking)
                    //{
                    //    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    //    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                    //    //turn rotation to direction. direction you want to move in
                    //    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //    //normalized movement vector
                    //    move = moveDirection.normalized * speed;

                    //    //smoothed angle for player rotation
                    //    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, defaultAttackRotationSmoothTime);

                    //    //rotate model by smooth angle so follow target doesnt also rotate
                    //    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    //}
                    ////move
                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);

                    if (OnSlope())
                    {
                        cc.Move(2 * cc.height * slopeForce * Time.deltaTime * Vector3.down);
                    }
                }
                else
                {
                    ////rotation of model when attacking or not attacking while moving
                    //if (isAttacking)
                    //{
                    //    Debug.Log("ee");
                    //    ////smooth angle for player rotation
                    //    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, defaultAttackRotationSmoothTime);

                    //    //rotate model by smooth angle so follow target doesnt also rotate
                    //    model.transform.rotation = Quaternion.Euler(0f, angle, 0f); ;
                    //}

                    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

                }
                #endregion

                break;

            case PlayerState.Jumping:

                //update input direction
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                //update gravity
                verticalVelocity -= gravity * Time.deltaTime;

                //update grounded
                isGrounded = cc.isGrounded;

                #region Animation
                //if (playerInput.currentControlScheme == "GamePad")
                //{
                //    //top right movement on joystick
                //    if (direction.x > 0f && direction.z > 0f)
                //    {
                //        if (direction.x > direction.z)
                //        {
                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }


                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;
                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;
                //            //}
                //        }
                //        else if (direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //top left movement on joystick
                //    else if (direction.x < 0f && direction.z > 0f)
                //    {
                //        if (-direction.x > direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }
                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom right movement on joystick
                //    else if (direction.x > 0f && direction.z < 0f)
                //    {
                //        if (direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom left movement on joystick
                //    else if (direction.x < 0f && direction.z < 0f)
                //    {
                //        if (-direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //no joystick movement
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //    }
                //}
                //else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                //{
                //    //left and right on keyboard
                //    if (direction.x > 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.x < 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //forwards and backwards on keyboard
                //    else if (direction.z > 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.z < 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top right movement on keyboard
                //    else if (direction.x > 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom right movement on keyboard
                //    else if (direction.x > 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top left movement on keyboard
                //    else if (direction.x < 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom left movement on keyboard
                //    else if (direction.x < 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //no movement on keyboard
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //        speed = runSpeed;
                //    }
                //}

                #endregion

                #region Jump Grounded Check Delay

                //decrease timer
                timer -= Time.deltaTime;

                //slight delay before checking if we are on the ground
                if (timer <= 0)
                {
                    //reset timer
                    timer = 0;

                    //if we are grounded
                    if (isGrounded)
                    {
                        //reset jumping bool
                        isJumping = false;

                        anim.SetBool("isJumping", false);
                        anim.SetBool("isFalling", false);

                        //switch playerstate to default
                        playerState = PlayerState.Default;

                        //break out of the loop
                        break;
                    }
                }
                #endregion

                #region Camera Update

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                //velocity = new Vector2(
                //    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                //    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                velocity = new Vector2(wantedVelocity.x, wantedVelocity.y);

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                #endregion

                #region Update Player Movement Variables

                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                    //turn rotation to direction. direction you want to move in
                    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //move vector
                    move = moveDirection.normalized * speed;

                    //smooth angle for player rotation
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);

                }
                else
                {
                    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
                }
                #endregion

                break;

            case PlayerState.Aiming:

                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                //Ground check
                isGrounded = cc.isGrounded;

                #region Update Gravity
                if (isGrounded)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                }
                #endregion

                if (!isGrounded)
                {
                    //set timer to ground check waiting time
                    timer = groundCheckWaitTime;

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    //go back to basic locomotion
                    anim.SetBool("isAiming", false);

                    //play falling animation
                    anim.SetBool("isFalling", true);

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    //break out of the loop
                    break;
                }

                //update animation float to control strafe anims
                anim.SetFloat("InputX", direction.x);
                anim.SetFloat("InputZ", direction.z);                

                #region Animation
                //if (playerInput.currentControlScheme == "GamePad")
                //{
                //    //top right movement on joystick
                //    if (direction.x > 0f && direction.z > 0f)
                //    {
                //        if (direction.x > direction.z)
                //        {
                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }


                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;
                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;
                //            //}
                //        }
                //        else if (direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //top left movement on joystick
                //    else if (direction.x < 0f && direction.z > 0f)
                //    {
                //        if (-direction.x > direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }
                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom right movement on joystick
                //    else if (direction.x > 0f && direction.z < 0f)
                //    {
                //        if (direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom left movement on joystick
                //    else if (direction.x < 0f && direction.z < 0f)
                //    {
                //        if (-direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //no joystick movement
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //    }
                //}
                //else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                //{
                //    //left and right on keyboard
                //    if (direction.x > 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.x < 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //forwards and backwards on keyboard
                //    else if (direction.z > 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.z < 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top right movement on keyboard
                //    else if (direction.x > 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom right movement on keyboard
                //    else if (direction.x > 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top left movement on keyboard
                //    else if (direction.x < 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom left movement on keyboard
                //    else if (direction.x < 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //no movement on keyboard
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //        speed = runSpeed;
                //    }
                //}

                #endregion

                #region Aim Button Check
                //if the aim button is released
                if (aimVal < 0.5f)
                {
                    //speed = walkSpeed;                   

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    anim.SetBool("isAiming", false);

                    //switch playerstate to default
                    playerState = PlayerState.Default;

                    //break out of the loop
                    break;
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

                if(isAttacking)
                {
                    anim.SetBool("isSpellAttack", true);
                    playerState = PlayerState.Spell;
                    break;
                }

                #endregion

                #region Jump Check

                if (jumpVal >= 1f && isGrounded)
                {
                    isJumping = true;
                }

                if (isJumping)
                {
                    //add jump force to our vertical velocity
                    verticalVelocity = jumpForce;

                    //set timer to ground check waiting time
                    timer = groundCheckWaitTime;

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    anim.SetBool("isAiming", false);
                    anim.SetBool("isJumping", true);

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    break;
                }
                #endregion

                #region Camera Update

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                //velocity = new Vector2(
                //    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                //    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                velocity = new Vector2(wantedVelocity.x, wantedVelocity.y);

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                #endregion

                #region Update Player Movement Variables
                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                    //turn rotation to direction. direction you want to move in
                    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //normalized movement vector
                    move = moveDirection.normalized * speed;

                    //smoothed angle for player rotation
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, defaultAttackRotationSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                }
                else
                {
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, aimRotationSmoothTime);

                    ////rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
                }
                #endregion

                break;

            case PlayerState.Melee:

                attackDelay -= Time.deltaTime;

                speed = walkSpeed;
                
                if (attackDelay <= 0f)
                {
                    attackDelay = 0f;
                }

                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                //Ground check
                isGrounded = cc.isGrounded;

                #region Update Gravity
                if (isGrounded)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                }
                #endregion

                if (!isGrounded)
                {
                    //go back to basic locomotion blend tree
                    anim.SetBool("isDefaultAttack", false);

                    //play falling animation
                    anim.SetBool("isFalling", true);

                    //set timer to ground check waiting time
                    timer = groundCheckWaitTime;

                    attackDelay = 0;

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    //break out of the loop
                    break;
                }

                //update animation float to control strafe anims
                anim.SetFloat("InputX", direction.x);
                anim.SetFloat("InputZ", direction.z);

                #region Animation
                //if (playerInput.currentControlScheme == "GamePad")
                //{
                //    //top right movement on joystick
                //    if (direction.x > 0f && direction.z > 0f)
                //    {
                //        if (direction.x > direction.z)
                //        {
                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }


                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;
                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;
                //            //}
                //        }
                //        else if (direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //top left movement on joystick
                //    else if (direction.x < 0f && direction.z > 0f)
                //    {
                //        if (-direction.x > direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }
                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom right movement on joystick
                //    else if (direction.x > 0f && direction.z < 0f)
                //    {
                //        if (direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //bottom left movement on joystick
                //    else if (direction.x < 0f && direction.z < 0f)
                //    {
                //        if (-direction.x > -direction.z)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.x > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //        else if (-direction.z > -direction.x)
                //        {

                //            if (runVal < 0.5f)
                //            {
                //                anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //                speed = walkSpeed;
                //            }
                //            else if (runVal >= 1f)
                //            {
                //                anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //                speed = runSpeed;
                //            }

                //            //if (-direction.z > 0.5f)
                //            //{
                //            //    anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //            //    speed = runSpeed;

                //            //}
                //            //else
                //            //{
                //            //    anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //            //    speed = walkSpeed;

                //            //}
                //        }
                //    }

                //    //no joystick movement
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //    }
                //}
                //else if (playerInput.currentControlScheme == "Keyboard&Mouse")
                //{
                //    //left and right on keyboard
                //    if (direction.x > 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.x < 0f && direction.z == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.x, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //forwards and backwards on keyboard
                //    else if (direction.z > 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }
                //    else if (direction.z < 0f && direction.x == 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", -direction.z, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top right movement on keyboard
                //    else if (direction.x > 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom right movement on keyboard
                //    else if (direction.x > 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //top left movement on keyboard
                //    else if (direction.x < 0 && direction.z > 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //bottom left movement on keyboard
                //    else if (direction.x < 0 && direction.z < 0)
                //    {
                //        if (runVal < 0.5f)
                //        {
                //            anim.SetFloat("MoveVal", 0.5f, 0.1f, Time.deltaTime);
                //            speed = walkSpeed;
                //        }
                //        else if (runVal >= 1f)
                //        {
                //            anim.SetFloat("MoveVal", 1f, 0.1f, Time.deltaTime);
                //            speed = runSpeed;
                //        }

                //        //anim.SetFloat("MoveVal", 1, 0.1f, Time.deltaTime);
                //        //speed = runSpeed;
                //    }

                //    //no movement on keyboard
                //    else if (direction.x == 0 && direction.z == 0)
                //    {
                //        anim.SetFloat("MoveVal", 0, 0.1f, Time.deltaTime);
                //        speed = runSpeed;
                //    }
                //}

                #endregion

                #region Jump Check
                if (jumpVal >= 1f && isGrounded)
                {
                    isJumping = true;                   
                }

                if(isJumping)
                {
                    timer -= Time.deltaTime;

                    if (timer <= 0f)
                    {
                        //reset attack delay to 0
                        attackDelay = 0;                     

                        //add jump force to our vertical velocity
                        verticalVelocity = jumpForce;

                        //set timer to ground check waiting time
                        timer = groundCheckWaitTime;

                        //go back to basic locomotion blend tree
                        anim.SetBool("isDefaultAttack", false);

                        //go to jumping anim clip
                        anim.SetBool("isJumping", true);

                        //reduce speed when jumping
                        //speed = walkSpeed;

                        //switch playerstate to jumping
                        playerState = PlayerState.Jumping;

                        //break out of the loop
                        break;
                    }
                }
                #endregion

                #region Player Attack Check

                if (attackVal >= 1f)
                {
                    isAttacking = true;
                }
                else if (attackVal < 0.5f)
                {
                    isAttacking = false;
                }

                if (!isAttacking && attackDelay <= 0f)
                {
                    timer -= Time.deltaTime;

                    if (timer <= 0f)
                    {
                        canAttack = true;

                        attackDelay = 0;
                        timer = 0.35f;

                        anim.SetBool("isDefaultAttack", false);

                        playerState = PlayerState.Default;

                        break;
                    }
                }
                else if (isAttacking && attackDelay <= 0f)
                {
                    //timer -= Time.deltaTime;

                    //if (timer <= 0f)
                    //{
                        canAttack = true;
                        attackDelay = 0.35f;
                        Debug.Log("uo");
                    //}
                }

                #endregion

                #region Camera Update

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                //velocity = new Vector2(
                //    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                //    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                velocity = new Vector2(wantedVelocity.x, wantedVelocity.y);

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                #endregion

                #region Update Player Movement Variables
                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                    //turn rotation to direction. direction you want to move in
                    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //normalized movement vector
                    move = moveDirection.normalized * speed;

                    //smoothed angle for player rotation
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, defaultAttackRotationSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);

                    if (OnSlope())
                    {
                        cc.Move(2 * cc.height * slopeForce * Time.deltaTime * Vector3.down);
                    }
                }
                else
                {
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, aimRotationSmoothTime);

                    ////rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
                }
                #endregion

                break;

            case PlayerState.Spell:

                speed = 0;

                //update movement input
                direction = new Vector3(moveVal.x, 0, moveVal.y);

                //Ground check
                isGrounded = cc.isGrounded;

                #region Update Gravity
                if (isGrounded)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                }
                #endregion

                if (!isGrounded)
                {
                    //set timer to ground check waiting time
                    timer = groundCheckWaitTime;

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    //go back to basic locomotion
                    anim.SetBool("isAiming", false);

                    //play falling animation
                    anim.SetBool("isFalling", true);

                    //switch playerstate to jumping
                    playerState = PlayerState.Jumping;

                    //break out of the loop
                    break;
                }

                #region Aim Button Check
                //if the aim button is released
                if (aimVal < 0.5f)
                {
                    //speed = walkSpeed;                   

                    //switch cinemachine cam to default cam
                    defaultCam.SetActive(true);
                    aimCam.SetActive(false);

                    anim.SetBool("isSpellAttack", false);
                    anim.SetBool("isAiming", false);

                    //switch playerstate to default
                    playerState = PlayerState.Default;

                    //break out of the loop
                    break;
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
                    anim.SetBool("isSpellAttack", false);
                    speed = aimWalkSpeed;
                    playerState = PlayerState.Aiming;
                }
                #endregion

                #region Camera Update

                wantedVelocity = lookVal * new Vector2(sensitivityX, sensitivityY);

                //slowy accelerate to a velocity. this is for camera smoothing
                //velocity = new Vector2(
                //    Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
                //    Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime));

                velocity = new Vector2(wantedVelocity.x, wantedVelocity.y);

                //camera pitch rotation clamped to a min and max value
                newCameraRot.x += sensitivityY * -velocity.y /*lookVal.y*/ * Time.deltaTime;
                newCameraRot.x = Mathf.Clamp(newCameraRot.x, viewClampYmin, viewClampYmax);

                //camera yaw rotation 
                newCameraRot.y += sensitivityX * velocity.x /*lookVal.x*/ * Time.deltaTime;

                #endregion

                #region Update Player Movement Variables
                if (direction.magnitude >= 0.1f)
                {
                    //get angle to see how much we need to rotate on y axis from moving relative to camera
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + newCameraRot.y;

                    //turn rotation to direction. direction you want to move in
                    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    //normalized movement vector
                    move = moveDirection.normalized * speed;

                    //smoothed angle for player rotation
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, defaultAttackRotationSmoothTime);

                    //rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
                }
                else
                {
                    angle = Mathf.SmoothDampAngle(model.transform.eulerAngles.y, newCameraRot.y, ref turnSmoothVelocity, aimRotationSmoothTime);

                    ////rotate model by smooth angle so follow target doesnt also rotate
                    model.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
                }
                #endregion

                break;
        }
    }

    bool OnSlope()
    {
        RaycastHit hit;

        //cast ray to ground
        if(Physics.Raycast(transform.position, Vector3.down, out hit, cc.height / 2 * slopeForceRayLength))
        {
            //if the normal from the hit is not 0,1,0 we are on a slope and need to do some additional force
            if(hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }
}
