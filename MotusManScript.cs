using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotusManScript : MonoBehaviour
{
    float rotateOffset = 0.0f;
    float speedInAir = 1.0f;                        // var inAirControlAcceleration
    float gravity = 20.0f;                          // gravity (downward pull only, added to vector.y) 
    private Vector3 inAirVelocity = Vector3.zero;   // current currentSpeed while in air 
    private float verticalSpeed = 0.0f;             // speed for vertical use
    private CollisionFlags collisionFlags;          // last collision flag returned from control move

    CharacterController characterController;        // instance of character controller

    static float moveSpeed = 0.0f;                  // current player moving speed
    private Vector3 moveDirection = Vector3.forward;            // store initial forward direction of player
    private float smoothDirection = 10.0f;                      // amount to smooth camera catching up to player

    float timeVal = 0;

    public Camera cameraObject;                                     // player camera  (usually main camera)
    public Animator anim;

    // Use this for initialization
    void Start()
    {
        characterController = GetComponent<CharacterController>();     // get handle to characterController
        characterController.tag = "Player";                            // set tag name to 'Player'	
    }
    void UpdateMoveDirection()
    {
        Vector3 forward = cameraObject.transform.TransformDirection(Vector3.forward);   // forward vector relative to the camera along the x-z plane
        forward.y = 0;                                                                  // up/down is set to 0
        forward = forward.normalized;                                                   // set forward between 0-1	
        Vector3 right = new Vector3(forward.z, 0, -forward.x);                      // right vector relative to the camera, always orthogonal to the forward vector

        float vertical = Input.GetAxisRaw("Vertical");                      // get input vertical
        float horizontal = Input.GetAxisRaw("Horizontal");                  // get input horizontal

        if (vertical == 0.0f && horizontal == 0.0f)
        {
            timeVal = 0;
            moveSpeed = 0.0f;
        }
        else
        {
            timeVal += Time.deltaTime * 3;
            moveSpeed = 1.0f * timeVal;
        }

        Vector3 targetDirection = horizontal * right + vertical * forward;      // target direction relative to the camera

        if (IsGrounded())                // if player on ground
        {
            if (targetDirection != Vector3.zero)    // store currentSpeed and direction separately
            {
                moveDirection = Vector3.Lerp(moveDirection, targetDirection, smoothDirection * Time.deltaTime); // smooth camera follow player direction
                moveDirection = moveDirection.normalized;   // normalize (set to 0-1 value)
            }
            anim.SetFloat("Speed", moveSpeed);
        }
        else
        {         // if in air, move player down based on velocity, direction, time and speed
            inAirVelocity += targetDirection.normalized * Time.deltaTime * speedInAir;  
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        SetGravity();                  // pulls character to the ground 'if' in air
                                       // pulls character to the ground 'if' in air
        UpdateMoveDirection();         // motor, direction and ani for player movement moves player.		
                                       // stores direction with speed (h,v)
        Vector3 movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0) + inAirVelocity;
        movement *= Time.deltaTime;    // set movement to delta time for consistent speed
			
        collisionFlags = characterController.Move(movement); // move the player.	

        if (IsGrounded())   // character is on the ground (set rotation, translation, direction, speed)
        {
            //orients character to the direction he is moving.		
            Quaternion q = Quaternion.LookRotation(moveDirection);
            transform.rotation = new Quaternion(q.x, q.y + rotateOffset, q.z, q.w);
            // turn off check on velocity, set to zero
            // current set to -.1 because zero won't keep him on isGrounded true. goes back and forth			
            inAirVelocity = new Vector3(0, -0.1f, 0);
            if (moveSpeed < 0.15f)  // set movespeed to 0 if it's less than .15
            {
                moveSpeed = 0;
            }
        }
    }
    ////////////////////////////////////////////////////////////////////
    bool IsGrounded()
    {          // check if player is touching the ground or a collision flag
               // if isGround not equal to 0 if it doesn't equal 0
        return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }
    ////////////////////////////////////////////////////////////////////
    void SetGravity()
    {
        if (IsGrounded())
            verticalSpeed = 0.0f;
        else
            verticalSpeed -= gravity * Time.deltaTime;  // if character in air, apply gravity.
    }


}


