using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.OSX;

public class Mario : MonoBehaviour
{
    private Animator animator;

    private MarioController marioController;
    private MarioMovement marioMovement;
    private MarioState marioState;

    private float runningSegmentTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        marioController = GetComponent<MarioController>();
        marioMovement = GetComponent<MarioMovement>();
        marioState = GetComponent<MarioState>();

        // Movement properties
        marioMovement.MinWalkSpeed = MarioConstants.MinWalkSpeed;
        marioMovement.MaxWalkSpeed = MarioConstants.MaxWalkSpeed;
        marioMovement.Acceleration = MarioConstants.WalkAcceleration;
        marioMovement.Deceleration = MarioConstants.Deceleration;
        marioMovement.DecelerationSkid = MarioConstants.DecelerationSkid;

        // Jump properties
        marioMovement.GravityScale = MarioConstants.GravityScale;
        marioMovement.AirControl = MarioConstants.AirControl;
        marioMovement.BounceForce = MarioConstants.BounceForce;
        marioMovement.JumpForce = MarioConstants.JumpForce;
        marioMovement.JumpMaxHoldTime = MarioConstants.JumpMaxHoldTimeWalking;

        // Delegates
        marioMovement.FallingDelegate = OnFalling;
        marioMovement.JumpedDelegate = OnJumped;
        marioMovement.JumpApexDelegate = OnJumpApex;
        marioMovement.JumpLandedDelegate = OnJumpLanded;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();

        marioMovement.LocalTimeScale = Game.Instance.LocalTimeScale;

        // Ensure that the player isn't Dead
        if (marioState.State != EMarioState.Dead)
        {
            // If Mario falls of the edge, he is no longer on the ground, the movement component 
            // catches this situation, update the IsOnGround value
            if (marioMovement.IsFalling() && marioState.IsOnGround)
                marioState.IsOnGround = false;

            // Handle the situation where Mario is Running 
            if (marioState.IsRunning && marioMovement.IsMovementBlocked == false)
            {
                if (marioController.GetMoveValue() != 0.0f && marioState.IsOnGround)
                {
                    // Increment the segment timer
                    runningSegmentTimer += Time.deltaTime * Game.Instance.LocalTimeScale;

                    // Is the segment timer greater than the duration constant?
                    if (runningSegmentTimer > MarioConstants.RunSegmentIncrementDuration)
                    {
                        // Reset the segment timer and increment the running meter
                        runningSegmentTimer = 0.0f;
                        marioState.RunningMeter++;

                        // Cap the running meter to the max constant and update the flipbook so that Mario's arms are out
                        if (marioState.RunningMeter >= MarioConstants.MaxRunningMeter)
                        {
                            marioState.RunningMeter = MarioConstants.MaxRunningMeter;
                            UpdateAnimator();
                        }

                        // Set the jump max time to factor in the running meter, running faster means Mario jumps higher and thus farther
                        marioMovement.JumpMaxHoldTime = MarioConstants.JumpMaxHoldTimeRunning + MarioConstants.JumpIncreasePerSegment * (float)marioState.RunningMeter;

                        // Set character movement's max walk speed to factor in the running meter
                        marioMovement.MaxWalkSpeed = MarioConstants.MaxWalkSpeed + (marioState.RunningMeter * MarioConstants.RunSpeedPerSegment);

                        // Set the flipbook's playback rate based on the running meter
                        float playRate = 1.0f + marioState.RunningMeter * 0.75f;
                        animator.speed = playRate;
                    }
                }
            }
            else
            {
                // If there's still some running meter built up, but the player is 
                // not running, gradually decrease the running meter
                if (marioState.RunningMeter > 0)
                {
                    // Increment the segment timer
                    runningSegmentTimer += Time.deltaTime * Game.Instance.LocalTimeScale;

                    // Is the segment timer greater than the duration constant?
                    if (runningSegmentTimer > MarioConstants.RunSegmentDecrementDuration)
                    {
                        // Reset the segment timer and decrement the running meter
                        runningSegmentTimer = 0.0f;
                        marioState.RunningMeter--;

                        // Ensure the running meter doesn't go lower than zero
                        if (marioState.RunningMeter <= 0)
                        {
                            marioState.RunningMeter = 0;

                            // Reset the Jump max time and the character's max walk speed to the walking default
                            marioMovement.JumpMaxHoldTime = MarioConstants.JumpMaxHoldTimeWalking;
                            marioMovement.MaxWalkSpeed = MarioConstants.MaxWalkSpeed;

                            // Reset the flipbook's playback rate to 1.0f
                            animator.speed = 1.0f;
                        }
                    }
                    else
                    {
                        // Set the jump max time to factor in the running meter, running faster means Mario jumps higher and thus farther
                        marioMovement.JumpMaxHoldTime = MarioConstants.JumpMaxHoldTimeRunning + MarioConstants.JumpIncreasePerSegment * (float)marioState.RunningMeter;

                        // Set character movement's max walk speed to factor in the running meter
                        marioMovement.MaxWalkSpeed = MarioConstants.MaxWalkSpeed + (marioState.RunningMeter * MarioConstants.RunSpeedPerSegment);

                        // Set the flipbook's playback rate based on the running meter
                        float playRate = 1.0f + marioState.RunningMeter * 0.75f;
                        animator.speed = playRate;
                    }
                }
            }

            // Mario has fallen off the edge of the level and has died
            if (transform.position.y < GameConstants.DestroyActorAtY)
                MarioHasDied(false);

            Debug.Log("Running meter: " + marioState.RunningMeter);
        }
    }

    public void ResetMario(Vector2 location)
    {
        gameObject.SetActive(true);

        transform.position = location;

        if (marioState.State == EMarioState.Dead)
        {
            Vector3 scale = transform.localScale;
            scale.x = 1.0f;
            transform.localScale = scale;

            marioState.Direction = EMarioDirection.Right;
        }

        // Set the state back to Idle
        ApplyStateChange(EMarioState.Idle);

        // Clear the movement forces
        marioMovement.ClearAccumulatedForces();
    }

    public void Run()
    {
        marioState.IsRunning = true;

        // Reset the the Running segment timer to zero, this 
        // timer determines when to increase the running meter
        runningSegmentTimer = 0.0f;

        // Set the jump max time to the running constant
        marioMovement.JumpMaxHoldTime = MarioConstants.JumpMaxHoldTimeRunning;
    }

    public void StopRunning()
    {
        // Set the PlayerState's IsRunning flag to false
        marioState.IsRunning = false;

        // Reset the the Running segment timer to zero
        runningSegmentTimer = 0.0f;
    }

    public void ApplyStateChange(EMarioState newState)
    {
        // Ensure the new mario state is different than the current state
        if (marioState.State == newState)
            return;

        // Assign the new state
        marioState.State = newState;

        if (newState == EMarioState.Jumping)
        {
            marioMovement.CheckJumpApex = true;
        }

        // Lastly, update the animator
        UpdateAnimator();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            EEnemyType enemyType = collider.gameObject.GetComponent<Enemy>().EnemyType;

            if (enemyType == EEnemyType.PiranhaPlant)
            {
                PiranhaPlant piranhaPlant = collider.gameObject.GetComponent<PiranhaPlant>();
                if (piranhaPlant != null && piranhaPlant.State != EPiranhaPlantState.Hiding)
                {
                    MarioHasDied(true);
                }
            }
            else if (enemyType == EEnemyType.FirePiranhaPlant)
            {
                FirePiranhaPlant firePiranhaPlant = collider.gameObject.GetComponent<FirePiranhaPlant>();
                if (firePiranhaPlant != null && firePiranhaPlant.State != EFirePiranhaPlantState.Hiding)
                {
                    MarioHasDied(true);
                }
            }
            else if (enemyType == EEnemyType.FireBall)
            {
                FireBall fireBall = collider.gameObject.GetComponent<FireBall>();
                if (fireBall != null)
                {
                    MarioHasDied(true);
                }
            }
        }
    }

    private void MarioHasDied(bool spawnDeadMario)
    {
        // Ensure that mario isn't dead
        if (marioState.State != EMarioState.Dead)
        {
            // Set the state change to Dead
            ApplyStateChange(EMarioState.Dead);

            // Deactivate the gameObject
            gameObject.SetActive(false);

            // Spawn the Dead mario
            Game.Instance.MarioHasDied(spawnDeadMario);
        }
    }

    private void OnFalling()
    {
        marioState.IsOnGround = false;
        ApplyStateChange(EMarioState.Falling);
    }

    private void OnJumped()
    {
        marioState.IsOnGround = false;
        ApplyStateChange(EMarioState.Jumping);
    }

    private void OnJumpApex()
    {
        if (marioState.IsOnGround == false)
            ApplyStateChange(EMarioState.Falling);
    }

    private void OnJumpLanded()
    {
        marioState.IsOnGround = true;

        if (marioController.IsDuckPressed())
        {
            ApplyStateChange(EMarioState.Ducking);
        }
        else
        {
            if (marioController.GetMoveValue() == 0.0f)
            {
                ApplyStateChange(EMarioState.Idle);
            }
            else
            {
                ApplyStateChange(EMarioState.Walking);
            }
        }
    }

    private void UpdateAnimator()
    {
        if (marioState.State == EMarioState.Idle || marioState.State == EMarioState.Ducking)
        {
            animator.Play("MarioSmallIdle");
        }
        else if (marioState.State == EMarioState.Walking)
        {
            if (marioMovement.IsSkidding() == false)
            {
                if (marioState.IsRunning && marioState.RunningMeter == MarioConstants.MaxRunningMeter)
                {
                    animator.Play("MarioSmallRun");
                }
                else
                {
                    animator.Play("MarioSmallWalk");
                }
            }
            else
            {
                animator.Play("MarioSmallTurn");
            }
        }
        else if (marioState.State == EMarioState.Jumping || marioState.State == EMarioState.Falling)
        {
            if (marioState.IsRunning && marioState.RunningMeter == MarioConstants.MaxRunningMeter)
            {
                animator.Play("MarioSmallRunJump");
            }
            else
            {
                animator.Play("MarioSmallJump");
            }
        }
    }
}
