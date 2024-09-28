using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public enum EFirePiranhaPlantState : byte
{
    Unknown,
    Hiding,
    AnimatingUp,
    Active,
    Fire,
    AnimatingDown
}

public enum EFireplantDirectionX : byte
{
    Right,
    Left,
}
public enum EFireplantDirectionY : byte
{
    Up,
    Down,
}

public class FirePiranhaPlant : Enemy
{
    private Animator animator;
    private EFirePiranhaPlantState state = EFirePiranhaPlantState.Unknown;
    private EFireplantDirectionX directionX = EFireplantDirectionX.Right;
    private EFireplantDirectionY directionY = EFireplantDirectionY.Down;
    private Vector2 hidingLocation = Vector2.zero;
    private Vector2 activeLocation = Vector2.zero;
    private float holdTimer = 0.0f;
    private float animationTimer = 0.0f;
    private float animationFireTimer = 0.0f;

    public GameObject fireBallPrefab;
    public float fireBallSpeed = 3.5f;

    public EFirePiranhaPlantState State
    {
        get { return state; }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        // Set the enemy type
        enemyType = EEnemyType.FirePiranhaPlant;

        // Capture the starting location (hiding) and from that calculate the active (on-screen) location
        hidingLocation = transform.position;
        activeLocation = hidingLocation + new Vector2(0.0f, EnemyConstants.FirePiranhaPlantOffsetY);

        // Set the state to hiding
        SetState(EFirePiranhaPlantState.Hiding);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;

        // Determina la dirección horizontal.
        SetLookingDirection(marioLocation.x < transform.position.x, marioLocation.y < transform.position.y);
        if (state == EFirePiranhaPlantState.Hiding)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaPlantState.AnimatingUp);
            }
        }
        else if (state == EFirePiranhaPlantState.AnimatingUp)
        {
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.FirePiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(hidingLocation.x, activeLocation.x, pct);
            float locationY = Mathf.Lerp(hidingLocation.y, activeLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EFirePiranhaPlantState.Active);
            }
        }
        else if (state == EFirePiranhaPlantState.Active)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaPlantState.Fire);
            }
        }
        else if (state == EFirePiranhaPlantState.Fire)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;
            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaPlantState.AnimatingDown);
            }
        }
        else if (state == EFirePiranhaPlantState.AnimatingDown)
        {
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.FirePiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(activeLocation.x, hidingLocation.x, pct);
            float locationY = Mathf.Lerp(activeLocation.y, hidingLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EFirePiranhaPlantState.Hiding);
            }
        }
    }

    private void SetState(EFirePiranhaPlantState newState)
    {
        if (state != newState)
        {
            state = newState;

            if (state == EFirePiranhaPlantState.Hiding)
            {
                transform.position = hidingLocation;
                holdTimer = UnityEngine.Random.Range(EnemyConstants.FirePiranhaPlantHiddenDurationMin, EnemyConstants.FirePiranhaPlantHiddenDurationMax);
            }
            else if (state == EFirePiranhaPlantState.AnimatingUp)
            {
                // Get Mario's location
                Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;

                // Check if Mario is on top of the pipe, if he is, don't spawn the piranha plant
                bool checkY = Mathf.Clamp(marioLocation.x, activeLocation.x - 1.0f, activeLocation.x + 1.0f) == marioLocation.x;
                if (checkY && Mathf.Abs(activeLocation.y - marioLocation.y) <= 0.51f)
                {
                    SetState(EFirePiranhaPlantState.Hiding);
                    return;
                }

                animationTimer = EnemyConstants.FirePiranhaPlantAnimationDuration;
            }
            else if (state == EFirePiranhaPlantState.Active)
            {
                transform.position = activeLocation;
                holdTimer = EnemyConstants.FirePiranhaPlantActiveDuration;
            }
            else if (state == EFirePiranhaPlantState.Fire)
            {
                SetFireDirection();
                SetState(EFirePiranhaPlantState.AnimatingDown);
            }
            else if (state == EFirePiranhaPlantState.AnimatingDown)
            {
                animationTimer = EnemyConstants.FirePiranhaPlantAnimationDuration;
            }
        }
    }

    private void SetFireDirection()
    {
        if (directionX != EFireplantDirectionX.Right)
        {
            directionX = EFireplantDirectionX.Left;
            Vector3 scale = transform.localScale;
            scale.x = 1.0f;  // Voltea a la izquierda.
            transform.localScale = scale;
        }
        else
        {
            directionX = EFireplantDirectionX.Right;
            Vector3 scale = transform.localScale;
            scale.x = -1.0f;  // Voltea a la derecha.
            transform.localScale = scale;
        }

        if (directionY != EFireplantDirectionY.Up)
        {
            directionY = EFireplantDirectionY.Down;
            animator.Play("FirePiranhaPlantDownOpen");
        }
        else
        {
            directionY = EFireplantDirectionY.Up;
            animator.Play("FirePiranhaPlantUpOpen");
        }
    }

    private void SetLookingDirection(bool isLeft, bool isDown)
    {
        if (isLeft)
        {
            directionX = EFireplantDirectionX.Left;
            Vector3 scale = transform.localScale;
            scale.x = 1.0f;  // Voltea a la izquierda.
            transform.localScale = scale;
        }
        else
        {
            directionX = EFireplantDirectionX.Right;
            Vector3 scale = transform.localScale;
            scale.x = -1.0f;  // Voltea a la derecha.
            transform.localScale = scale;
        }

        if (isDown)
        {
            directionY = EFireplantDirectionY.Down;
            animator.Play("FirePiranhaPlantDownClose");
        }
        else
        {
            directionY = EFireplantDirectionY.Up;
            animator.Play("FirePiranhaPlantUpClose");
        }
    }

    private void Fire()
    {
        if (fireBallPrefab != null)
        {
            GameObject fireBall = Instantiate(fireBallPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();

            Vector2 fireDirection = Vector2.zero;
            if (directionX == EFireplantDirectionX.Right && directionY == EFireplantDirectionY.Down)
            {
                fireDirection = new Vector2(fireBallSpeed, -fireBallSpeed);
            }
            else if (directionX == EFireplantDirectionX.Right && directionY == EFireplantDirectionY.Up)
            {
                fireDirection = new Vector2(fireBallSpeed, fireBallSpeed);
            }
            else if (directionX == EFireplantDirectionX.Left && directionY == EFireplantDirectionY.Down)
            {
                fireDirection = new Vector2(-fireBallSpeed, -fireBallSpeed);
            }
            else if (directionX == EFireplantDirectionX.Left && directionY == EFireplantDirectionY.Up)
            {
                fireDirection = new Vector2(-fireBallSpeed, fireBallSpeed);
            }

            if (rb != null)
            {
                rb.velocity = fireDirection;
            }
        }

    }
}