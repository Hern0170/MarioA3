using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Enemy
{

    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        enemyType = EEnemyType.FireBall;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
