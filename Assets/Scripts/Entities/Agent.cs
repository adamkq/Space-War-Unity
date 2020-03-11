﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Agent : Entity
{
    // A general class for player and enemy characters
    // at the player's whim, an Agent should be able to switch between AI and manual control

    [System.Serializable]
    public class Basic
    {
        internal int health;
        // basic info for a character
        public bool playerControlled;
        public bool invuln;
        public int startHealth;
        public int score = 0;
    }

    [System.Serializable]
    public class Dynamics
    {
        // state
        internal Rigidbody2D rb2D;
        internal Vector2 accLinear;
        internal float accAngular;

        // limits
        public float speedLimit = 20f, accFwdLimit = 10f, accRevLimit = -5f, accTurnLimit = 2f;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Hazard")
        {
            if (other.name.Contains("Black Hole"))
            {
                IncrementHealth(-50);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Projectile")
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj.FiredBy && proj.FiredBy.GetComponent<Entity>().team != team)
            {
                IncrementHealth(-proj.damage);
            }
        }
    }

    void IncrementHealth(int dHealth)
    {
        if (basic.invuln)
        {
            return;
        }

        basic.health += dHealth;
        if (basic.health < 1)
        {
            Kill();
        }
    }

    void Kill()
    {
        Respawn();
        Destroy(gameObject);
    }

    void Actuate()
    {
        // forward
        dynamics.accLinear = Vector2.Dot(transform.up, dynamics.accLinear) > 0
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
        _bullet.speed += Vector2.Dot(transform.up, dynamics.rb2D.velocity);
        _bullet.speed = Mathf.Max(_bullet.speed, 1f);

        // ID bullet with Agent
        _bullet.FiredBy = gameObject;
    }
}