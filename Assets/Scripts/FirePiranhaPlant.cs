using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.OSX;


public enum EFirePiranhaPlantState : byte
{
    Unknown,
    Hiding,
    AnimatingUp,
    Active,
    Fire,
    FiringDelay,
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
    private bool shooted = false;

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
            SetLookingDirection(marioLocation.x < transform.position.x, marioLocation.y < transform.position.y);
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
            SetLookingDirection(marioLocation.x < transform.position.x, marioLocation.y < transform.position.y);
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
            SetFireDirection();
            if (!shooted)
            {
                shooted = true;
                Fire();
            }
            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaPlantState.FiringDelay);
            }
        }
        else if (state == EFirePiranhaPlantState.FiringDelay)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;
            SetLookingDirection(marioLocation.x < transform.position.x, marioLocation.y < transform.position.y);
            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaPlantState.AnimatingDown);
            }
        }
        else if (state == EFirePiranhaPlantState.AnimatingDown)
        {
            SetLookingDirection(marioLocation.x < transform.position.x, marioLocation.y < transform.position.y);
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
                shooted = false;
                holdTimer = UnityEngine.Random.Range(EnemyConstants.FirePiranhaPlantHiddenDurationMin, EnemyConstants.FirePiranhaPlantHiddenDurationMax);
            }
            else if (state == EFirePiranhaPlantState.AnimatingUp)
            {
                // Get Mario's location
                Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position ;

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
                holdTimer = EnemyConstants.FirePiranhaPlantAnimationFireTimer;
            }
            else if (state == EFirePiranhaPlantState.FiringDelay)
            {
                holdTimer = holdTimer = UnityEngine.Random.Range(EnemyConstants.FirePiranhaPlantHiddenDurationMin, EnemyConstants.FirePiranhaPlantHiddenDurationMax);
            }
            else if (state == EFirePiranhaPlantState.AnimatingDown)
            {
                animationTimer = EnemyConstants.FirePiranhaPlantAnimationDuration;
            }
        }
    }

    private void SetFireDirection()
    {
        // Asegura que la escala y la animación se configuran correctamente según la dirección actual
        if (directionX == EFireplantDirectionX.Left)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Voltea a la izquierda
        }
        else
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f); // Voltea a la derecha
        }

        // Determina y reproduce la animación basada en la dirección vertical actual
        if (directionY == EFireplantDirectionY.Up)
        {

            animator.Play("FirePiranhaPlantUpOpen"); // Asumiendo que este es el nombre correcto de la animación
        }
        else
        {

            animator.Play("FirePiranhaPlantDownOpen"); // Asumiendo que este es el nombre correcto de la animación
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
            Vector2 fireDirection = new Vector2(0, 0);
            Vector2 spawnPosition = transform.position + new Vector3(0, 1.5f, 0); // Ajusta la posición para que la bola de fuego aparezca un poco más arriba de la planta.

            if (directionX == EFireplantDirectionX.Right && directionY == EFireplantDirectionY.Up)
            {
                fireDirection = new Vector2(3.5f, 3.5f);
            }
            else if (directionX == EFireplantDirectionX.Right && directionY == EFireplantDirectionY.Down)
            {
                fireDirection = new Vector2(3.5f, -3.5f);
            }
            else if (directionX == EFireplantDirectionX.Left && directionY == EFireplantDirectionY.Up)
            {
                fireDirection = new Vector2(-3.5f, 3.5f);
            }
            else // Left and Down
            {
                fireDirection = new Vector2(-3.5f, -3.5f);
            }

            // Llama a la función SpawnFireBall en Game.cs
            Game.Instance.SpawnFireBall(spawnPosition, fireDirection);
        }
    }



}