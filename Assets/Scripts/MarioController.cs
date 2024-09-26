using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarioController : MonoBehaviour
{
    private Mario mario;
    private MarioMovement marioMovement;
    private MarioState marioState;

    private InputAction MoveInputAction;
    private InputAction RunInputAction;
    private InputAction JumpInputAction;
    private InputAction DuckInputAction;
    private InputAction UpInputAction;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Mario, MarioMovement and MarioState components
        mario = GetComponent<Mario>();
        marioMovement = GetComponent<MarioMovement>();
        marioState = GetComponent<MarioState>();

        // Get the PlayerInput component and get the InputActions
        PlayerInput input = GetComponent<PlayerInput>();
        MoveInputAction = input.actions["Move"];
        RunInputAction = input.actions["Run"];
        JumpInputAction = input.actions["Jump"];
        DuckInputAction = input.actions["Duck"];
        UpInputAction = input.actions["Up"];
    }

    public float GetMoveValue()
    {
        return MoveInputAction.ReadValue<float>();
    }

    public bool IsRunPressed()
    {
        return RunInputAction.IsPressed();
    }

    public bool IsJumpPressed()
    {
        return JumpInputAction.IsPressed();
    }

    public bool IsDuckPressed()
    {
        return DuckInputAction.IsPressed();
    }

    public bool IsUpPressed()
    {
        return UpInputAction.IsPressed();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if ( (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled))
        {
            float direction = context.ReadValue<float>();

            if (direction == 0.0f)
            {
                if (marioState.IsOnGround && marioState.State != EMarioState.Ducking)
                {
                    mario.ApplyStateChange(EMarioState.Idle);
                }
            }
            else
            {
                if (marioState.IsOnGround)
                    mario.ApplyStateChange(EMarioState.Walking);

                if (direction < 0.0f)
                {
                    marioState.Direction = EMarioDirection.Left;

                    Vector3 scale = transform.localScale;
                    scale.x = -1.0f;
                    transform.localScale = scale;
                }
                else if (direction > 0.0f)
                {
                    marioState.Direction = EMarioDirection.Right;

                    Vector3 scale = transform.localScale;
                    scale.x = 1.0f;
                    transform.localScale = scale;
                }
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            float value = context.ReadValue<float>();

            if (value > 0.0f)
                marioMovement.Jump();
            else
                marioMovement.StopJumping();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            float value = context.ReadValue<float>();

            if (value > 0.0f)
                mario.Run();
            else
                mario.StopRunning();
        }
    }

    public void OnDuck(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            float value = context.ReadValue<float>();

            // TODO: Implement ducking
        }
    }

    public void OnUp(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            float value = context.ReadValue<float>();

            // TODO: Implement fortress doors (Final Project)
        }
    }
}
