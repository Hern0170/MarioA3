using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MarioConstants
{
    // Jump constants
    public const float GravityScale = 3.75f;
    public const float AirControl = 1.0f;
    public const float JumpForce = 26.5f;
    public const float JumpMaxHoldTimeWalking = 0.1525f;
    public const float JumpMaxHoldTimeRunning = 0.19125f;
    public const float JumpIncreasePerSegment = 0.0025f;
    public const float BounceForce = 40.0f;

    // Walking constants
    public const float MinWalkSpeed = 0.7f;
    public const float MaxWalkSpeed = 14.0f;
    public const float WalkAcceleration = 19.5f;
    public const float Deceleration = 42.0f;
    public const float DecelerationSkid = 64.0f;

    // Running constants
    public const int MaxRunningMeter = 7;
    public const float RunAcceleration = 27.0f;
    public const float RunSpeedPerSegment = 1.1f;
    public const float RunSegmentIncrementDuration = 0.2f;
    public const float RunSegmentDecrementDuration = 0.05f;

    // Dead
    public const float DeadHoldTime = 1.5f;
    public const float DeadForceY = 26.0f;
}
