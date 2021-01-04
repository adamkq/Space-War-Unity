using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Projectile
{
    
    private CircleCollider2D[] cc2Ds;
    private int health;

    public Team team;
    public bool noDamageFallOff;
    public int startHealth = 5;
    public float bombRadius = 0.5f; // radius of the actual bomb object
    public float triggerRadius = 3f; // will detonate when agent comes within this distance
    public float blastRadius = 8f; // will apply force and damage to objects within this distance
    public float blastForce = 999f; // maximum force exerted. Force must be high since it is only applied for 1 frame.
    public AudioClip explosionSound;

    void OnValidate()
    {
        startHealth = Mathf.Max(startHealth, 1);
        cc2Ds = GetComponents<CircleCollider2D>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.isTrigger) // trigger
            {
                cc2D.radius = triggerRadius;
            }
            else // actual bomb
            {
                cc2D.radius = bombRadius;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        rb2D = GetComponentInChildren<Rigidbody2D>(); // kinematic by default
        rb2D.velocity = transform.up * speed;
        rb2D.isKinematic = false;

        health = startHealth;

        if (FiredBy) team = FiredBy.GetComponent<Agent>().team;
    }

    private void FixedUpdate()
    {
        if (health < 1) StartCoroutine(Detonate());
    }

    // called for every object in the blast zone, so no need to loop
    // detonate when a hostile agent gets close
    // bullet and laser hits are triggers, not collisions, so must be handled here
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Agent"))
        {
            
            // no-self triggering, but can damage other NoTeam players
            if (collision.gameObject == FiredBy)
            {
                return;
            }
            
            Team otherTeam = collision.gameObject.GetComponent<Entity>().team;
            if (otherTeam != team || otherTeam == Team.NoTeam)
            {
                IncrementHealth(-999);
            }
        }

        float dist = Vector2.Distance(transform.position, collision.transform.position);
        
        if (collision.name.ToLower().Contains("bullet") && dist < bombRadius)
        {
            // damage from bullets; teams can destory their own bombs
            Projectile proj = collision.GetComponent<Projectile>();
            IncrementHealth(-proj.damage);
        }
    }

    // BH and NS collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Hazard") && !other.name.ToLower().Contains("asteroid")) IncrementHealth(-999);
    }

    public void IncrementHealth(int dHealth)
    {
        health += dHealth;
    }

    // apply blast force and damage to objects
    internal IEnumerator Detonate()
    {
        // facilitates chain-explosions
        yield return new WaitForSeconds(Random.Range(0.05f,0.5f));

        // Play sound in front of the camera to mitigate "panning" L / R effect.
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z + 5f), 0.9f);

        GameObject _bombBlast = gameObject.transform.GetChild(0).gameObject;
        CircleCollider2D cc2D = _bombBlast.GetComponent<CircleCollider2D>();
        cc2D.enabled = true;

        yield return new WaitForFixedUpdate();

        Destroy(gameObject);
    }
}
