using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Agent : Entity
{
    // A general class for player and enemy characters

    [System.Serializable]
    // Agents will have different ship types to give the game a little more variety
    // These will be distinguished by sprite and by parameter values rather than being split into classes
    public enum ShipTypes { Assault, Sentry, Infiltrator, Mechanic, Sapper };

    [System.Serializable]
    public class Basic
    {
        // basic info for an Agent

        // used for scoring and targeting
        internal GameObject lastHitBy;
        internal GameObject killedBy;

        // settings
        [Tooltip("Toggle between Player and AI controllers (if those components have been added)")]
        public bool playerControlled;
        [Tooltip("Agent score (persists after death)")]
        public int score = 0;
        [Tooltip("Seconds between death and respawn")]
        public float respawnDelay = 3f;
        [Tooltip("Ship Type. Affects appearance, powerups, weapons, and other abilities.")]
        public ShipTypes shipType;
    }

    [System.Serializable]
    public class Dynamics
    {
        // state
        internal float accLinear = 0f, accAngular = 0f;

        // limits
        [Tooltip("Limit the acceleration of the agent.")]
        public float accFwdLimit = 10f, accRevLimit = -5f, accTurnLimit = 2f;
    }
    
    [System.Serializable]
    public class Weapons
    {
        // contains and spawns weapon prefabs
        // TODO: Add projectile queue system
        internal float timeLastFired = float.NegativeInfinity; // rate of fire
        internal Vector2 offset; // used to offset projectiles from the agent

        [Tooltip("Disables weapons.")]
        public bool pacifist;
        [Tooltip("Bullet prefab to instantiate/take from object pool.")]
        public GameObject bullet;
        [Tooltip("Number of guns. Multiplies the ROF.")]
        public int numberOfGuns = 1;
        [Tooltip("Bullets per second. Enter 0 to fire as fast as possible (once per fixed update)")]
        public float bulletROF;
        [Tooltip("Bullets spread in degrees, either side")]
        public float bulletSpread;
        public GameObject bomb;
        public uint numberOfBombs;
        public GameObject laser;

    }

    public Basic basic;
    public Dynamics dynamics;
    public Weapons weapons;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (basic.playerControlled)
        {
            if (GetComponent<PlayerController>()) GetComponent<PlayerController>().enabled = true;
            if (GetComponent<AIController>()) GetComponent<AIController>().enabled = false;
        }
        if (!basic.playerControlled)
        {
            if (GetComponent<PlayerController>()) GetComponent<PlayerController>().enabled = false;
            if (GetComponent<AIController>()) GetComponent<AIController>().enabled = true;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (c2D.GetType() == typeof(CircleCollider2D))
        {
            float radius = GetComponent<CircleCollider2D>().radius;
            weapons.offset = new Vector2(radius, radius);
        }
        else if (c2D.GetType() == typeof(BoxCollider2D) || c2D.GetType() == typeof(CapsuleCollider2D))
        {
            weapons.offset = GetComponent<BoxCollider2D>().size;
        }
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
        if (!alive)
        {
            return;
        }
        Actuate();
        if (health < 1) Kill();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Hazard"))
        {
            if (other.name.Contains("Black Hole")) IncrementHealth(-50);

            else if (other.name.Contains("Neutron Star")) IncrementHealth(-5);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Projectile"))
        {
            // general projectile damage
            Projectile proj = other.GetComponent<Projectile>();
            if (proj && proj.FiredBy && proj.FiredBy.GetComponent<Entity>().team != team)
            {
                basic.lastHitBy = proj.FiredBy;
                IncrementHealth(-proj.damage);
            }
        }
    }

    void Kill()
    {
        if (basic.lastHitBy)
        {
            basic.killedBy = basic.lastHitBy;
            basic.killedBy.GetComponent<Agent>().IncrementScore(100);
        }
        else
        {
            basic.killedBy = null;
        }
        StartCoroutine(RespawnWithDelay(basic.respawnDelay, basic.killedBy));
        basic.lastHitBy = null;
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

    public void IncrementScore(int dScore)
    {
        basic.score += dScore;
        Debug.LogFormat("{0} New Score: {1}", name, basic.score);
    }

    public void FireBullet()
    {
        // TODO: take from pool
        if (!alive || weapons.pacifist || (weapons.bulletROF != 0 && Time.time - weapons.timeLastFired < Mathf.Max(Time.fixedDeltaTime, 1 / weapons.bulletROF)))
        {
            return;
        }
        weapons.timeLastFired = Time.time;

        for (int gun = 0; gun < weapons.numberOfGuns; gun++)
        {
            // instantiate bullets at different spots on the Agent
            float offset = (weapons.numberOfGuns == 1) ? 0 : (2 * gun /(weapons.numberOfGuns - 1) - 1) * weapons.offset.x;

            // initialize and add spread
            GameObject bullet = Instantiate(weapons.bullet, transform.position + offset * transform.right, transform.rotation);
            bullet.transform.Rotate(bullet.transform.forward, Random.Range(-weapons.bulletSpread, weapons.bulletSpread));

            // speed vector add: cross(A, B) = AB cos(angle(A,B))
            // do not fire backwards
            Projectile _bullet = bullet.GetComponent<Projectile>();

            // allow other objects to be fired from here
            if (_bullet)
            {
                _bullet.speed = Mathf.Max(_bullet.speed + Vector2.Dot(transform.up, rb2D.velocity), 1f);

                // ID bullet with Agent
                _bullet.FiredBy = gameObject;
            }
        }
    }

    public void FireLaser()
    {
        if (!alive || weapons.pacifist)
        {
            return;
        }

        GameObject laser = Instantiate(weapons.laser, transform.position + transform.up * weapons.offset.y, transform.rotation);

        Projectile _laser = laser.GetComponent<Projectile>();
        if (_laser) _laser.FiredBy = gameObject;
    }

    public void LaunchBomb()
    {
        if (!alive || weapons.pacifist)
        {
            return;
        }
        // TODO: charge shot
        GameObject bomb = Instantiate(weapons.bomb, transform.position + transform.up * weapons.offset.y, transform.rotation);

        // speed vector add: cross(A, B) = AB cos(angle(A,B))
        // do not fire backwards
        Projectile _bomb = bomb.GetComponent<Projectile>();

        // allow other objects to be fired from here
        if (_bomb)
        {
            _bomb.speed = Mathf.Max(_bomb.speed + Vector2.Dot(transform.up, rb2D.velocity), 1f);

            // ID bullet with Agent
            _bomb.FiredBy = gameObject;
        }
    }
}
