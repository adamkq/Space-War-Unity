using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class asteroidScript : MonoBehaviour
{
    private Rigidbody2D rb2D;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // Add a speed to the enemy
        rb2D.velocity = Random.insideUnitCircle;
        rb2D.velocity.Normalize();
        rb2D.velocity *= 5;

        // Make the enemy rotate on itself
        rb2D.angularVelocity = Random.Range(-200, 200);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "bullet(Clone)")
        {
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
        if (other.name == "spaceship")
        {
            Destroy(gameObject);
        }
    }

    // Function called when the object goes out of the screen
    void OnBecameInvisible()
    {
        // Destroy the enemy
        Destroy(gameObject);
    }
}
