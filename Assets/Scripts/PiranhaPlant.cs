using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum EPiranhaPlantState : byte
{
    Unknown,
    Hiding,
    AnimatingUp,
    Active,
    AnimatingDown
}

public class PiranhaPlant : Enemy
{
    private EPiranhaPlantState state = EPiranhaPlantState.Unknown;
    private Vector2 hidingLocation = Vector2.zero;
    private Vector2 activeLocation = Vector2.zero;
    private float holdTimer = 0.0f;
    private float animationTimer = 0.0f;

    public EPiranhaPlantState State
    { 
        get { return state; } 
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the enemy type
        enemyType = EEnemyType.PiranhaPlant;

        // Capture the starting location (hiding) and from that calculate the active (on-screen) location
        hidingLocation = transform.position;
        activeLocation = hidingLocation + new Vector2(0.0f, EnemyConstants.PiranhaPlantOffsetY);

        // Set the state to hiding
        SetState(EPiranhaPlantState.Hiding);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == EPiranhaPlantState.Hiding)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EPiranhaPlantState.AnimatingUp);
            }
        }
        else if (state == EPiranhaPlantState.AnimatingUp)
        {
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.PiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(hidingLocation.x, activeLocation.x, pct);
            float locationY = Mathf.Lerp(hidingLocation.y, activeLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EPiranhaPlantState.Active);
            }
        }
        else if (state == EPiranhaPlantState.Active)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EPiranhaPlantState.AnimatingDown);
            }
        }
        else if (state == EPiranhaPlantState.AnimatingDown)
        {
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.PiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(activeLocation.x, hidingLocation.x, pct);
            float locationY = Mathf.Lerp(activeLocation.y, hidingLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EPiranhaPlantState.Hiding);
            }
        }
    }

    private void SetState(EPiranhaPlantState newState)
    {
        if (state != newState)
        {
            state = newState;

            if (state == EPiranhaPlantState.Hiding)
            {
                transform.position = hidingLocation;
                holdTimer = UnityEngine.Random.Range(EnemyConstants.PiranhaPlantHiddenDurationMin, EnemyConstants.PiranhaPlantHiddenDurationMax);
            }
            else if (state == EPiranhaPlantState.AnimatingUp)
            {
                // Get Mario's location
                Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;

                // Check if Mario is on top of the pipe, if he is, don't spawn the piranha plant
                bool checkY = Mathf.Clamp(marioLocation.x, activeLocation.x - 1.0f, activeLocation.x + 1.0f) == marioLocation.x;
                if (checkY && Mathf.Abs(activeLocation.y - marioLocation.y) <= 0.51f)
                {
                    SetState(EPiranhaPlantState.Hiding);
                    return;
                }

                animationTimer = EnemyConstants.PiranhaPlantAnimationDuration;
            }
            else if (state == EPiranhaPlantState.Active)
            {
                transform.position = activeLocation;
                holdTimer = EnemyConstants.PiranhaPlantActiveDuration;
            }
            else if (state == EPiranhaPlantState.AnimatingDown)
            {
                animationTimer = EnemyConstants.PiranhaPlantAnimationDuration;
            }
        }
    }
}
