using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Projectile
{
    
    private CircleCollider2D[] cc2Ds;
    private PointEffector2D pe2D; // blast zone
    private int health;

    public int startHealth = 1;
    public float triggerRadius = 3f; // will detonate when agent comes within this distance
    public float blastRadius = 8f; // will apply force and damage to objects within this distance
    public float blastForce = 100f;

    protected virtual void OnValidate()
    {
        startHealth = Mathf.Max(startHealth, 1);
        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        pe2D = GetComponentInChildren<PointEffector2D>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.gameObject.CompareTag("Projectile")) // fuze
            {
                cc2D.radius = triggerRadius;
            }
            else if (cc2D.gameObject.CompareTag("AreaEffect"))// blast
            {
                cc2D.radius = blastRadius;
            }
            else // actual bomb
            {
                cc2D.radius = 0.5f;
            }
        }
        pe2D.forceMagnitude = blastForce;
    }

    protected override void Start()
    {
        base.Start();
        rb2D = GetComponentInChildren<Rigidbody2D>(); // kinematic
        rb2D.velocity = transform.up * speed;
        rb2D.isKinematic = false;

        pe2D = GetComponentInChildren<PointEffector2D>();

        health = startHealth;
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    private void FixedUpdate()
    {

        if (health < 1) Detonate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        Debug.LogFormat("Trigger Enter: {0}", other.name);
        //if (other.CompareTag("Projectile") && !other.name.Contains("Bomb"))
        //{
        //    // damage from bullets and lasers
        //    Projectile proj = other.GetComponent<Projectile>();
        //    IncrementHealth(-proj.damage);
        //}
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        Debug.LogFormat("Collision Enter: {0}", other.name);
    }

    void IncrementHealth(int dHealth)
    {
        health += dHealth;
    }

    void Detonate()
    {
        pe2D.enabled = true;
        Destroy(gameObject);
    }
}
