using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Bullet : Projectile
{
    public float lifetime = 30f; // bullet
    private bool updateOrientation = true;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb2D = GetComponent<Rigidbody2D>(); // kinematic
        rb2D.velocity = transform.up * speed;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
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

    // aligns bullet with direction it is traveling
    public void UpdateOrientation()
    {
        rb2D.SetRotation(Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg - 90f);
        updateOrientation = false;
    }
}
