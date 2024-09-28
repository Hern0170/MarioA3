using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEnemyType : byte
{
    Unknown,
    PiranhaPlant,
    FirePiranhaPlant,
    FireBall
    //TODO: Add additional EnemyType enumerators here
}

public class Enemy : MonoBehaviour
{
    protected EEnemyType enemyType = EEnemyType.Unknown;

    public EEnemyType EnemyType 
    { 
        get { return enemyType; } 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
