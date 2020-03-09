using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // will revamp this into generic 'projectile' script
    // draw p's from a queue
    // https://www.raywenderlich.com/847-object-pooling-in-unity
    public float speed = 10.0f;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
    }
    
    void Update()
    {

    }

    void OnBecameInvisible()
    {
        // Destroy the bullet 
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.name != "bullet(Clone)")
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        GameObject other = collision.gameObject;
        // TODO: Implement wall types. Some walls will destroy the projectile, others will bounce it
        if (other.name != "bullet(Clone)")
        {
            Destroy(gameObject);
        }
        
    }
}
