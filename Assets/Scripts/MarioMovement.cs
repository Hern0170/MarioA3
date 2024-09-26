using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MarioMovement : MonoBehaviour
{
    // Movement delegates
    public delegate void OnFallingDelegate();
    public delegate void OnJumpedDelegate();
    public delegate void OnJumpApexDelegate();
    public delegate void OnJumpLandedDelegate();

    private OnFallingDelegate fallingDelegate;
    private OnJumpedDelegate jumpedDelegate;
    private OnJumpApexDelegate jumpApexDelegate;
    private OnJumpLandedDelegate jumpLandedDelegate;

    // Components
    private MarioState marioState;
    private MarioController marioController;
    private new Rigidbody2D rigidbody;

    private float localTimeScale = 1.0f;

    // Walking / running members
    private Vector2 lastPosition = Vector2.zero;
    private Vector2 velocity = Vector2.zero;
    private Vector2 lastVelocity = Vector2.zero;
    private float minWalkSpeed = 0.0f;
    private float maxWalkSpeed = 0.0f;
    private float acceleration = 0.0f;
    private float deceleration = 0.0f;
    private float decelerationSkid = 0.0f;
    private bool isMovementBlocked = false;
    private int framesMovementBlocked = 0;

    // Jump members
    private float gravityScale = 1.0f;
    private float airControl = 0.7f;
    private float bounceForce = 0.0f;
    private float jumpForce = 0.0f;
    private float jumpKeyHoldTime = 0.0f;
    private float jumpMaxHoldTime = 0.0f;
    private int jumpCurrentCount = 0;
    private int jumpMaxCount = 1;
    private bool isJumpPressed = false;
    private bool wasJumping = false;
    private bool checkJumpApex = false;

    public OnFallingDelegate FallingDelegate
    {
        get { return fallingDelegate; }
        set { fallingDelegate = value; }
    }

    public OnJumpedDelegate JumpedDelegate
    {
        get { return jumpedDelegate; }
        set { jumpedDelegate = value; }
    }

    public OnJumpApexDelegate JumpApexDelegate
    {
        get { return jumpApexDelegate; }
        set { jumpApexDelegate = value; }
    }

    public OnJumpLandedDelegate JumpLandedDelegate
    {
        get { return jumpLandedDelegate; }
        set { jumpLandedDelegate = value; }
    }

    public float LocalTimeScale
    {
        get { return localTimeScale; }
        set { localTimeScale = value; }
    }

    public Vector2 Velocity
    {
        get { return velocity; }
    }

    public float MinWalkSpeed
    {
        get { return minWalkSpeed; }
        set { minWalkSpeed = value; }
    }

    public float MaxWalkSpeed
    {
        get { return maxWalkSpeed; }
        set { maxWalkSpeed = value; }
    }

    public float Acceleration
    {
        get { return acceleration; }
        set { acceleration = value; }
    }

    public float Deceleration
    {
        get { return deceleration; }
        set { deceleration = value; }
    }

    public float DecelerationSkid
    {
        get { return decelerationSkid; }
        set { decelerationSkid = value; }
    }

    public bool IsMovementBlocked
    {
        get { return isMovementBlocked; }
    }

    public float GravityScale
    {
        get { return gravityScale; }
        set { gravityScale = value; GetComponent<Rigidbody2D>().gravityScale = gravityScale; }
    }

    public float AirControl
    {
        get { return airControl; }
        set { airControl = value; }
    }

    public float BounceForce
    {
        get { return bounceForce; }
        set { bounceForce = value; }
    }

    public float JumpForce
    {
        get { return jumpForce; }
        set { jumpForce = value; }
    }

    public float JumpMaxHoldTime
    {
        get { return jumpMaxHoldTime; }
        set { jumpMaxHoldTime = value; }
    }

    public int JumpMaxCount
    {
        get { return jumpMaxCount; }
        set { jumpMaxCount = value; }
    }

    public bool CheckJumpApex
    {
        get { return checkJumpApex; }
        set { checkJumpApex = value; }
    }

    static private bool IsClose(float a, float b, float tolerance)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    // Start is called before the first frame update
    void Start()
    {
        marioState = GetComponent<MarioState>();
        marioController = GetComponent<MarioController>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        lastVelocity = velocity;

        // If the rigid body velocity is zero, it means we hit an obstacle and should clear the velocity on the x-axis
        if (MarioMovement.IsClose(rigidbody.velocity.x, 0.0f, 0.01f) && velocity.x != 0.0f)
        {
            velocity.x = 0.0f;
        }

        // Set the y velocity to match the rigid body's y velocity
        velocity.y = rigidbody.velocity.y;

        // Detect when mario is on the ground or falling
        if (MarioMovement.IsClose(velocity.y, 0.0f, 0.02f) && lastVelocity.y < 0.0f)
        {
            if (IsFalling() && isJumpPressed == false)
            {
                HandleLanding();
            }
            else if(marioState.State == EMarioState.Idle && isJumpPressed == false)
            {
                if (marioState.IsOnGround == false)
                    marioState.IsOnGround = true;
            }
        }
        else
        {
            if (velocity.y < -0.9f)
            {
                if (marioState.IsOnGround)
                {
                    fallingDelegate();
                }
            }
        }

        // Handle the horizontal movement
        HorizontalMovement();

        // Handle the Jump input
        CheckJumpInput();
        ClearJumpInput();
    }

    private void FixedUpdate()
    {
        // Set the rigidbody's velocity
        rigidbody.velocity = velocity * localTimeScale;

        // Check if the jump has reached the apex
        if (checkJumpApex && rigidbody.velocity.y <= 0.0f)
        {
            checkJumpApex = false;

            if (jumpApexDelegate != null)
                jumpApexDelegate();
        }

        // Check to see if movement is blocked
        if (marioController.GetMoveValue() != 0.0f && IsFalling() == false)
        {
            if (IsClose(rigidbody.position.x, lastPosition.x, 0.01f))
                framesMovementBlocked++;
            else
                framesMovementBlocked = 0;

            isMovementBlocked = framesMovementBlocked >= 2;
            lastPosition = rigidbody.position;
        }
        else
        {
            isMovementBlocked = false;
        }
    }

    public void Pause()
    {
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Unpause()
    {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Jump()
    {
        isJumpPressed = true;
        jumpKeyHoldTime = 0.0f;
    }

    public void StopJumping()
    {
        isJumpPressed = false;
        ResetJumpState();
    }

    public bool IsFalling()
    {
        return marioState.State == EMarioState.Falling || marioState.State == EMarioState.Jumping;
    }

    public bool IsMovingOnGround()
    {
        return (marioState.State == EMarioState.Walking || marioState.State == EMarioState.Idle) && marioState.IsOnGround;
    }

    public bool IsSkidding()
    {
        float inputX = marioController.GetMoveValue();
        float direction = Mathf.Sign(velocity.x);

        if (direction == 1.0f && inputX < 0.0f && marioController.IsDuckPressed() == false)
            return true;

        if (direction == -1.0f && inputX > 0.0f && marioController.IsDuckPressed() == false)
            return true;

        return false;
    }

    public void ApplyBounce()
    {
        rigidbody.AddForce(new Vector2(0.0f, bounceForce), ForceMode2D.Impulse);
        marioState.IsOnGround = false;
    }

    public void ClearAccumulatedForces()
    {
        velocity = Vector2.zero;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = 0.0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0)
        {
            if (collision.contacts[0].normal.y >= 0.8f)
            {
                HandleLanding();
            }
        }
    }

    private void HorizontalMovement()
    {
        // Get the x input
        float inputX = marioController.GetMoveValue();
        float direction = Mathf.Sign(velocity.x);

        if (marioState.IsOnGround && IsFalling() == false)
        {
            // Ground physics
            if (Mathf.Abs(velocity.x) < minWalkSpeed)
            {
                velocity.x = 0.0f;

                if (marioController.IsDuckPressed() == false)
                    velocity.x += inputX * minWalkSpeed * localTimeScale;
            }
            else if (Mathf.Abs(velocity.x) >= minWalkSpeed)
            {
                if (direction == 1.0f) 
                {
                    if (inputX > 0.0f && marioController.IsDuckPressed() == false)
                    {
                        velocity.x += acceleration * Time.deltaTime * localTimeScale;
                    }
                    else if (inputX < 0.0f && marioController.IsDuckPressed() == false)
                    {
                        velocity.x -= decelerationSkid * Time.deltaTime * localTimeScale;
                    }
                    else
                    {
                        velocity.x -= deceleration * Time.deltaTime * localTimeScale;
                    }
                }
                else if (direction == -1.0f)
                {
                    if (inputX < 0.0f && marioController.IsDuckPressed() == false)
                    {
                        velocity.x -= acceleration * Time.deltaTime * localTimeScale;
                    }
                    else if (inputX > 0.0f && marioController.IsDuckPressed() == false)
                    {
                        velocity.x += decelerationSkid * Time.deltaTime * localTimeScale;
                    }
                    else
                    {
                        velocity.x += deceleration * Time.deltaTime * localTimeScale;
                    }
                }
            }
        }
        else
        {
            // Air controls
            if (inputX > 0.0f)
            {
                velocity.x += acceleration * airControl * Time.deltaTime * localTimeScale;
            }
            else if (inputX < 0.0f)
            {
                velocity.x -= acceleration * airControl * Time.deltaTime * localTimeScale;
            }
        }

        // Cap the max velocity
        if (velocity.x >= maxWalkSpeed)
            velocity.x = maxWalkSpeed;

        if (velocity.x <= -maxWalkSpeed)
            velocity.x = -maxWalkSpeed;
    }

    private void HandleLanding()
    {
        if (jumpLandedDelegate != null)
            jumpLandedDelegate();

        ResetJumpState();
    }

    private void CheckJumpInput()
    {
        if (isJumpPressed)
        {
            bool isFirstJump = jumpCurrentCount == 0;
            if (isFirstJump && IsFalling())
            {
                jumpCurrentCount++;
            }

            bool didJump = CanJump() && DoJump();
            if (didJump)
            {
                if (!wasJumping)
                {
                    jumpCurrentCount++;
                    
                    if (jumpedDelegate != null)
                        jumpedDelegate();
                }
            }

            wasJumping = didJump;
        }
    }

    private void ClearJumpInput()
    {
        if (isJumpPressed)
        {
            jumpKeyHoldTime += Time.deltaTime;

            if (jumpKeyHoldTime >= jumpMaxHoldTime)
                isJumpPressed = false;
        }
        else
        {
            wasJumping = false;
        }
    }

    private void ResetJumpState()
    {
        isJumpPressed = false;
        wasJumping = false;
        jumpKeyHoldTime = 0.0f;

        if (IsFalling() == false)
            jumpCurrentCount = 0;
    }

    private bool DoJump()
    {
        velocity.y = jumpForce;
        return true;
    }

    private bool CanJump()
    {
        bool isDucking = marioState.State == EMarioState.Ducking;
        bool isJumpAllowed = isDucking == false && IsMovingOnGround() || IsFalling();

        if (isJumpAllowed)
        {
            if (!wasJumping || jumpMaxHoldTime <= 0.0f)
            {
                if (jumpCurrentCount == 0 && IsFalling())
                {
                    isJumpAllowed = jumpCurrentCount + 1 < jumpMaxCount;
                }
                else
                {
                    isJumpAllowed = jumpCurrentCount < jumpMaxCount;
                }
            }
            else
            {
                bool jumpKeyHeld = (isJumpPressed && jumpKeyHoldTime < jumpMaxHoldTime);
                isJumpAllowed = jumpKeyHeld && ((jumpCurrentCount < jumpMaxCount) || (wasJumping && jumpCurrentCount == jumpMaxCount));
            }
        }

        return isJumpAllowed;
    }
}
