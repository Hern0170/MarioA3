using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadMario : MonoBehaviour
{
    private new Rigidbody2D rigidbody;
    private float holdInPlaceTimer = MarioConstants.DeadHoldTime;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (holdInPlaceTimer > 0.0f)
        {
            holdInPlaceTimer -= Time.deltaTime;
            if (holdInPlaceTimer <= 0.0f)
            {
                holdInPlaceTimer = 0.0f;

                rigidbody.gravityScale = 2.0f;
                rigidbody.AddForce(new Vector2(0.0f, MarioConstants.DeadForceY), ForceMode2D.Impulse);
            }
        }
    }
}
