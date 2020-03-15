using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Agent : Entity
{
    // A general class for player and enemy characters
    // at the player's whim, an Agent should be able to switch between AI and manual control TODO

    [System.Serializable]
    // Agents will have different ship types to give the game a little more variety
    // These will be distinguished by sprite and by parameter values rather than being split into classes
    public enum ShipTypes { Assault, Sentry, Infiltrator, Mechanic, Sapper };

    [System.Serializable]
    public class Basic
    {
        // basic info for an Agent
        internal bool alive = true;

        public bool playerControlled;
        public int score = 0;
        public float respawnDelay = 0f;
        public ShipTypes shipType;
    }

    [System.Serializable]
    public class Dynamics
    {
        // state
        internal float accLinear = 0f, accAngular = 0f;

        // limits
        public float accFwdLimit = 10f, accRevLimit = -5f, accTurnLimit = 2f;
    }


    [System.Serializable]
    public class Weapons
    {
        // contains and spawns weapon prefabs
        // TODO: Add projectile queue system
        internal float timeLastFired = float.NegativeInfinity;

        public bool pacifist; // disable weapons
        public GameObject bullet;
        public float bulletROF; // per second. Enter 0 to fire as fast as possible (once per fixed update)
        public float bulletSpread; // degrees, either side
        public GameObject bomb;
        public uint numberOfBombs;
        public GameObject laser;

    }

    public Basic basic;
    public Dynamics dynamics;
    public Weapons weapons;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        transform.parent = GameObject.Find("Agents").transform;
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        Actuate();
        if (health < 1) Kill();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Hazard"))
        {
            if (other.name.Contains("Black Hole")) IncrementHealth(-50);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Projectile"))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj.FiredBy && proj.FiredBy.GetComponent<Entity>().team != team)
            {
                IncrementHealth(-proj.damage);
            }
        }
    }

    void Kill()
    {
        // TODO: Implement spawn delay
        Respawn();
        Destroy(gameObject);
    }

    void Actuate()
    {
        dynamics.accLinear = Mathf.Clamp(dynamics.accLinear, dynamics.accRevLimit, dynamics.accFwdLimit);
        dynamics.accAngular = Mathf.Clamp(dynamics.accAngular, -dynamics.accTurnLimit, dynamics.accTurnLimit);

        // apply
        rb2D.AddForce(transform.up * dynamics.accLinear);
        rb2D.AddTorque(dynamics.accAngular);

        // reset
        dynamics.accLinear = 0f;
        dynamics.accAngular = 0f;
        
    }

    public void FireBullet()
    {
        // TODO: take from Queue
        if (weapons.pacifist || (weapons.bulletROF != 0 && Time.time - weapons.timeLastFired < Mathf.Max(Time.fixedDeltaTime, 1 / weapons.bulletROF)))
        {
            return;
        }
        weapons.timeLastFired = Time.time;

        // initialize and add spread
        GameObject bullet = Instantiate(weapons.bullet, transform.position, transform.rotation);
        bullet.transform.Rotate(new Vector3(0f, 0f, Random.Range(-weapons.bulletSpread, weapons.bulletSpread)));

        // speed vector add: cross(A, B) = AB cos(angle(A,B))
        // do not fire backwards
        Projectile _bullet = bullet.GetComponent<Projectile>();
        _bullet.speed = Mathf.Max(_bullet.speed + Vector2.Dot(transform.up, rb2D.velocity), 1f);

        // ID bullet with Agent
        _bullet.FiredBy = gameObject;
    }
}
