using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Entity
{

    [System.Serializable]
    public class Dynamics
    {
        internal Rigidbody2D rb2D;

        public float maxLinearVel;
        public float maxAngularVel;
    }

    public Dynamics dynamics;

    private void Awake()
    {
        dynamics.rb2D = GetComponent<Rigidbody2D>();

        // Random velocities
        dynamics.rb2D.velocity = Random.insideUnitCircle;
        dynamics.rb2D.velocity.Normalize();
        dynamics.rb2D.velocity *= dynamics.maxLinearVel;
        dynamics.rb2D.angularVelocity = Random.Range(-dynamics.maxAngularVel, dynamics.maxAngularVel);
    }

    protected override void Start()
    {
        transform.parent = GameObject.Find("Hazards").transform;
        base.Start();
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
