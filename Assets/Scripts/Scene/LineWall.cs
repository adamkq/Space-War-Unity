using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Collider2D))]

public class LineWall : MonoBehaviour
{
    private LineRenderer lRend;
    private Collider2D c2D;

    public WallType wallType;

    public PhysicsMaterial2D mat2DDefault;
    public PhysicsMaterial2D mat2DBounce;

    void OnValidate()
    {
        lRend = GetComponent<LineRenderer>();
        lRend.startColor = Walls.wallColors[wallType];
        lRend.endColor = lRend.startColor;

        c2D = GetComponent<Collider2D>();

        if (wallType == WallType.Default) c2D.sharedMaterial = mat2DDefault;

        c2D.sharedMaterial = (wallType == WallType.Bouncy || wallType == WallType.HotWall) ? mat2DBounce : mat2DDefault;

        c2D.isTrigger = (wallType == WallType.EntityPass || wallType == WallType.ProjectilePass);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        if (wallType == WallType.HotWall)
        {
            // Entities can have many different tags
            Entity entity = other.GetComponent<Entity>();
            if (entity)
            {
                entity.IncrementHealth(-1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        // bounce bullets
        GameObject other = collision.gameObject;
        if ((wallType == WallType.HotWall || wallType == WallType.Bouncy) && other.CompareTag("Projectile"))
        {
            if (wallType == WallType.HotWall) other.GetComponent<Projectile>().damage += 1;
            Bounce(other, c2D.sharedMaterial.bounciness);
        }

        // bounce agents
        else if (wallType == WallType.ProjectilePass && !other.CompareTag("Projectile"))
        {
            Bounce(other, c2D.sharedMaterial.bounciness);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        
        GameObject other = collision.gameObject;
        if (wallType == WallType.ProjectilePass && !other.CompareTag("Projectile"))
        {
            Bounce(other, c2D.sharedMaterial.bounciness);
        }
    }

    // Bounces colliding objects selectively based on wall type
    private void Bounce(GameObject other, float elasticity = 1f)
    {
        // should be offset by collider offset but probably won't matter since we only need the angle
        Vector2 offCollider = other.transform.position - 1f * other.transform.up;
        Vector2 inNormal = offCollider - c2D.ClosestPoint(offCollider);
        // in range 0-90
        Rigidbody2D rb2D = other.GetComponent<Rigidbody2D>();
        if (rb2D)
        {
            bool wasKinematic = rb2D.isKinematic;
            rb2D.isKinematic = true;
            rb2D.velocity = Vector3.Reflect(rb2D.velocity, inNormal.normalized);
            rb2D.velocity *= elasticity;
            rb2D.isKinematic = wasKinematic;
        }
    }
}
