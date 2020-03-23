using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // triggers do not terminate bullets
    private static readonly HashSet<string> exemptTriggers = new HashSet<string>() { "Projectile", "Respawn", "Waypoint", "AreaEffect" };
    // these walls do not terminate bullets
    private static readonly HashSet<WallType> exemptWalls = new HashSet<WallType>() { WallType.Bouncy, WallType.ProjectilePass, WallType.HotWall };

    private Rigidbody2D rb2D;
    private float timeFired;
    private bool updateOrientation = true;

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
        if (updateOrientation) UpdateOrientation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        // projectile does not collide with Agent that fired it
        if (FiredBy && other == FiredBy) return;

        // bullets are most likely to collide with walls
        if (other.CompareTag("Wall"))
        {
            if (!exemptWalls.Contains(other.GetComponent<LineWall>().wallType)) Terminate();
            updateOrientation = true;
            return;
        }

        if (!exemptTriggers.Contains(other.tag)) Terminate();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        updateOrientation = true;
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

    // aligns bullet with direction it is traveling
    public void UpdateOrientation()
    {
        rb2D.SetRotation(Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg - 90f);
        updateOrientation = false;
    }
}
