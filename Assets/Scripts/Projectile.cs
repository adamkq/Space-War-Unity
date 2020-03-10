using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private float timeFired;

    public int damage = 1;
    public float speed = 10.0f;
    public float lifetime;

    public GameObject FiredBy { get; set; }

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.velocity = transform.up * speed;
        timeFired = Time.time;
    }

    private void Update()
    {
        // change orientation based on bounce, black holes, etc
        float rot = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg - 90f;
        if (Mathf.Abs(rb2D.rotation - rot) > 0.1f)
        {
            rb2D.SetRotation(rot);
        }

        if (Time.time - timeFired > lifetime)
        {
            Terminate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        // TODO: Implement wall types. Some walls will terminate the projectile, others will bounce it
        if (other.tag != "Respawn" && other.tag != "Projectile" && other.tag != "AreaEffect")
        {
            Terminate();
        }
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
