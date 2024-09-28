using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Enemy
{
    private Rigidbody2D rb;
    private Vector2 _direction;
    public Vector2 Direction 
    { 
        get { return _direction; } 
        set { _direction = value; }
    }
    void Start()
    {
        enemyType = EEnemyType.FireBall;
        rb = GetComponent<Rigidbody2D>();
        Launch(_direction);
    }

    void Update()
    {
    }


    public void Launch(Vector2 direction)
    {
        if(rb ==null) return;
        rb.velocity = direction * EnemyConstants.FireBallSpeed;
    }

}