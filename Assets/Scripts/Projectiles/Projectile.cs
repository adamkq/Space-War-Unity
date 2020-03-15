using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private static readonly HashSet<string> exemptTriggers = new HashSet<string>() { "Projectile", "Respawn", "Waypoint", "AreaEffect" };
    private Rigidbody2D rb2D;
    private float timeFired;

    public int damage = 1;
    public float speed = 10.0f;
    public float lifetime;
    public float speedDecay = 1f;

    public GameObject FiredBy { get; set; }

    void Start()
    {
        transform.parent = GameObject.Find("Projectiles").transform;
        rb2D = GetComponent<Rigidbody2D>(); // kinematic
        rb2D.velocity = transform.up * speed;

        timeFired = Time.time;
    }

    private void Update()
    {
        if (Time.time - timeFired > lifetime) Terminate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        // projectile does not collide with Agent that fired it, or "invisible" objects
        // what if FiredBy does not exist?
        if (other != FiredBy && !exemptTriggers.Contains(other.tag)) Terminate();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // change orientation based on bounce, black holes, etc
        rb2D.SetRotation(Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg - 90f);
    }

    private void Terminate()
    {
        // deactivate and add to queue
        Destroy(gameObject);
    }

    public void FireProjectile(GameObject projectile, Transform tform, float speed, float lifetime=30f, int damage=1)
    {
        // Take bullet from queue
        // Set its tform (This is needed to make sure that the bullet doesn't collide with the agent firing it)
        // set other params
    }
}
