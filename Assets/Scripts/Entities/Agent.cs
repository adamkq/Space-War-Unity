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
        internal int health;
        
        public bool playerControlled;
        public bool invuln;
        public int startHealth;
        public int score = 0;
        public ShipTypes shipType;
    }

    [System.Serializable]
    public class Dynamics
    {
        // state
        internal Rigidbody2D rb2D;
        internal Vector2 accLinear = Vector2.zero;
        internal float accAngular = 0f;
        public float fwdSpeed = 0f; // derived

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

    private void Awake()
    {
        dynamics.rb2D = GetComponent<Rigidbody2D>();
        Actuate(); // initialize dynamics
    }

    protected override void Start()
    {
        transform.parent = GameObject.Find("Agents").transform;
        basic.health = basic.startHealth;
        base.Start();
    }

    private void FixedUpdate()
    {
        Actuate();
        if (basic.health < 1) Kill();
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
            if (proj.FiredBy && proj.FiredBy.GetComponent<Entity>().team != team) IncrementHealth(-proj.damage);
        }
    }

    void IncrementHealth(int dHealth)
    {
        if (basic.invuln && dHealth < 0) return;

        basic.health += dHealth;
    }

    void Kill()
    {
        // TODO: Implement spawn delay
        Respawn();
        Destroy(gameObject);
    }

    void Actuate()
    {
        dynamics.fwdSpeed = Vector2.Dot(transform.up, dynamics.rb2D.velocity);

        dynamics.accLinear = dynamics.fwdSpeed > 0
            ? (Vector2)Vector3.ClampMagnitude(dynamics.accLinear, dynamics.accFwdLimit)
            : (Vector2)Vector3.ClampMagnitude(dynamics.accLinear, -dynamics.accRevLimit);

        dynamics.accAngular = Mathf.Clamp(dynamics.accAngular, -dynamics.accTurnLimit, dynamics.accTurnLimit);

        // apply
        dynamics.rb2D.AddForce(dynamics.accLinear);
        dynamics.rb2D.AddTorque(dynamics.accAngular);

        // reset
        dynamics.accAngular = 0f;
        dynamics.accLinear = Vector3.zero;
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
        _bullet.speed += dynamics.fwdSpeed;
        _bullet.speed = Mathf.Max(_bullet.speed, 1f);

        // ID bullet with Agent
        _bullet.FiredBy = gameObject;
    }
}
