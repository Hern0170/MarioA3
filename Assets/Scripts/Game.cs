using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    private static Game sInstance;

    public GameObject marioGameObject;
    public GameObject deadMarioPrefab;

    private GameObject deadMario = null;
    private Vector2 marioSpawnLocation = Vector2.zero;
    private float localTimeScale = 1.0f;

    public static Game Instance
    {
        get { return sInstance; }
    }

    public GameObject MarioGameObject
    {
        get { return marioGameObject; }
    }

    public Mario GetMario
    {
        get { return marioGameObject.GetComponent<Mario>(); }
    }

    public MarioState GetMarioState
    {
        get { return marioGameObject.GetComponent<MarioState>(); }
    }

    public MarioMovement GetMarioMovement
    {
        get { return marioGameObject.GetComponent<MarioMovement>(); }
    }

    public float LocalTimeScale
    { 
        get { return localTimeScale; } 
    }

    // Start is called before the first frame update
    void Start()
    {
        // Setup the static instance of the Game class
        if (sInstance != null && sInstance != this)
        {
            Destroy(this);
        }
        else
        {
            sInstance = this;
        }

        // Get Mario's spawn location
        marioSpawnLocation = marioGameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (deadMario != null)
        {
            if (deadMario.transform.position.y < GameConstants.DestroyActorAtY)
            {
                Destroy(deadMario);
                deadMario = null;

                UnpauseActors();

                GetMario.ResetMario(marioSpawnLocation);
            }
        }
    }

    public void PauseActors()
    {
        localTimeScale = 0.0f;

        // get root objects in scene
        List<GameObject> gameObjects = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(gameObjects);

        // iterate root objects and do something
        for (int i = 0; i < gameObjects.Count; ++i)
        {
            if (gameObjects[i].CompareTag("Mario"))
            {
                gameObjects[i].GetComponent<MarioMovement>().Pause();
            }
            else
            {
                Animator animator = gameObjects[i].GetComponent<Animator>();

                if (animator != null)
                    animator.speed = 0.0f;
            }
        }
    }

    public void UnpauseActors()
    {
        localTimeScale = 1.0f;

        // get root objects in scene
        List<GameObject> gameObjects = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(gameObjects);

        // iterate root objects and do something
        for (int i = 0; i < gameObjects.Count; ++i)
        {
            if (gameObjects[i].CompareTag("Mario"))
            {
                gameObjects[i].GetComponent<MarioMovement>().Unpause();
            }
            else
            {
                Animator animator = gameObjects[i].GetComponent<Animator>();

                if (animator != null)
                    animator.speed = 1.0f;
            }
        }
    }

    public void MarioHasDied(bool spawnDeadMario)
    {
        // Do we spawn dead mario or not?
        if (spawnDeadMario)
        {
            SpawnDeadMario(marioGameObject.transform.position);
        }
        else
        {
            GetMario.ResetMario(marioSpawnLocation);
        }
    }

    private void SpawnDeadMario(Vector2 location)
    {
        if (deadMario == null)
        {
            PauseActors();

            if (deadMarioPrefab != null)
            {
                deadMario = Instantiate(deadMarioPrefab, new Vector3(location.x, location.y, -1.5f), Quaternion.identity);
            }
        }
    }
}
