using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Projectile
{
    
    private CircleCollider2D[] cc2Ds;
    private Dictionary<GameObject, Team> objectsInBlastZone;
    private int health;
    private Team team;
    
    public int startHealth = 1;
    public float bombRadius = 0.5f; // radius of the actual bomb object
    public float triggerRadius = 3f; // will detonate when agent comes within this distance
    public float blastRadius = 8f; // will apply force and damage to objects within this distance
    public float blastForce = 100f;
    public bool noDamageFallOff;

    protected virtual void OnValidate()
    {
        startHealth = Mathf.Max(startHealth, 1);
        cc2Ds = GetComponents<CircleCollider2D>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.isTrigger) // blast
            {
                cc2D.radius = blastRadius;
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

        objectsInBlastZone = new Dictionary<GameObject, Team>();
        health = startHealth;

        if (FiredBy) team = FiredBy.GetComponent<Agent>().team;
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
        // entities; all can have forces applied to them, but only some can suffer damage
        if (collision.CompareTag("Agent") || collision.CompareTag("Hazard"))
        {
            objectsInBlastZone.Add(other, other.GetComponent<Entity>().team);
        }

        // only track other bombs for chain-reaction purposes. won't worry about tracking bullets or lasers
        else if (other.CompareTag("Projectile") && other.name.ToLower().Contains("bomb"))
        {
            if (!objectsInBlastZone.ContainsKey(other))
            {
                objectsInBlastZone.Add(other, other.GetComponent<Bomb>().team);
            } 
        }
    }

    // called for every object in the blast zone, so no need to loop
    // detonate when a hostile agent gets close
    // bullet and laser hits are triggers, not collisions, so must be handled here
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Agent") && Vector2.Distance(transform.position, collision.transform.position) < triggerRadius)
        {
            if (collision.gameObject == FiredBy)
            {
                return;
            }
            if (objectsInBlastZone.ContainsKey(collision.gameObject))
            {
                if (objectsInBlastZone[collision.gameObject] != team || objectsInBlastZone[collision.gameObject] == Team.NoTeam) Detonate();
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        // remove object from collection of objects in blast zone
        GameObject other = collision.gameObject;
        objectsInBlastZone.Remove(other);
    }

    // BH and NS collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Hazard") && !other.name.ToLower().Contains("asteroid")) Detonate();

        if (other.name.ToLower().Contains("laser"))
        {
            Debug.Log("laser");
        }

    }

    public void IncrementHealth(int dHealth)
    {
        health += dHealth;
    }

    // apply blast force and damage to objects
    void Detonate()
    {
        
        Rigidbody2D rb2DBlast;
        Vector2 relPos;
        int blastDamage;

        foreach (var obj in objectsInBlastZone)
        {
            // blast force; only objects w/ RBs are tracked, so no need to check if each one has an RB
            if (!obj.Key)
            {
                continue;
            }
            rb2DBlast = obj.Key.GetComponent<Rigidbody2D>();
            relPos = rb2DBlast.position - (Vector2)transform.position;
            rb2DBlast.AddForce(relPos.normalized * blastForce / relPos.magnitude, ForceMode2D.Impulse);

            // damage; agents track who they were last hit by, so they need a special function
            if (obj.Key != FiredBy && (obj.Value != team || obj.Value == Team.NoTeam))
            {
                blastDamage = noDamageFallOff ? damage : Mathf.FloorToInt(damage / relPos.magnitude);
                if (obj.Key.CompareTag("Hazard")) obj.Key.GetComponent<Entity>().IncrementHealth(-blastDamage);
                else if (obj.Key.CompareTag("Agent")) obj.Key.GetComponent<Agent>().ApplyBombBlastDamage(-blastDamage, FiredBy);
            }

            // chain reaction
            if (obj.Key.CompareTag("Projectile") && obj.Key.name.ToLower().Contains("bomb"))
            {
                obj.Key.GetComponent<Bomb>().IncrementHealth(-999);
            }
        }
        Destroy(gameObject);
    }
}
