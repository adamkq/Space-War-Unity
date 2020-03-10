using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]

public class BlackHole : MonoBehaviour
{
    private CircleCollider2D[] cc2Ds;
    private ParticleSystem ps;
    private PointEffector2D pe2D;

    public float singularityRadius = 1f;
    public float areaEffectRadius = 2f;
   
    void Start()
    {
        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        pe2D = GetComponentInChildren<PointEffector2D>();
    }

    void Update()
    {
        
    }

    private void OnValidate()
    {
        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        ps = GetComponent<ParticleSystem>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.isTrigger) // area effect
            {
                cc2D.radius = areaEffectRadius;
            }
            else // singularity
            {
                cc2D.radius = singularityRadius;
            }
        }

        if (ps)
        {
            // arbitrary
            ParticleSystem.ShapeModule pss = ps.shape;
            pss.radius = (singularityRadius + areaEffectRadius) / 2f;
            pss.radiusThickness = 1 - singularityRadius / pss.radius;

            ps.trigger.SetCollider(0, cc2Ds[0]);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // will likely destroy the object, but this should be called in each relevant object
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // apply a pseudo-force to every kinematic rb2D in the area of influence
        GameObject other = collision.gameObject;
        Rigidbody2D rb2Dk = other.GetComponent<Rigidbody2D>();
        if (rb2Dk && rb2Dk.isKinematic)
        {
            float forceMag = pe2D.forceMagnitude;
            float pseudoForce;
            Vector2 relPos = rb2Dk.worldCenterOfMass - (Vector2)transform.position;
            relPos.Normalize();

            // inv linear
            pseudoForce = forceMag / Vector3.Magnitude(relPos) * Time.fixedDeltaTime;
            
            rb2Dk.velocity += relPos * pseudoForce;
        }
    }
}
