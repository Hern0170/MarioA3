using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMarioState : byte
{
    Idle,
    Walking,
    Jumping,
    Falling,
    Ducking,
    Dead
}

public enum EMarioDirection : byte
{
    Right,
    Left
}

public class MarioState : MonoBehaviour
{
    private EMarioState state = EMarioState.Idle;
    private EMarioDirection direction = EMarioDirection.Right;

    private int runningMeter = 0;

    private bool isRunning = false;
    private bool isOnGround = true;

    public EMarioState State 
    { 
        get { return state; }
        set { state = value; }
    }

    public EMarioDirection Direction 
    { 
        get { return direction; } 
        set {  direction = value; }    
    }

    public float DirectionScalar
    {
        get { return Direction == EMarioDirection.Left ? -1.0f : 1.0f; }
    }

    public int RunningMeter
    {
        get { return runningMeter; }
        set { runningMeter = value; }
    }

    public bool IsRunning
    {
        get { return isRunning; }
        set {  isRunning = value; }
    }

    public bool IsOnGround
    { 
        get { return isOnGround; } 
        set { isOnGround = value; } 
    }
}
