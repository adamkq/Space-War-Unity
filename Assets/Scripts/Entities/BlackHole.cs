using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : Entity
{
    private CircleCollider2D[] cc2Ds;
    private ParticleSystem ps;
    private PointEffector2D pe2D;

    public float singularityRadius = 1f;
    public float areaEffectRadius = 2f;
    public float forceMagnitude = -25f;

    protected override void OnValidate()
    {
        base.OnValidate();
        invuln = true;

        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        ps = GetComponent<ParticleSystem>();
        pe2D = GetComponentInChildren<PointEffector2D>();

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

        sRend.transform.localScale = new Vector3(2 * singularityRadius, 2 * singularityRadius, 1f);
        pe2D.forceMagnitude = forceMagnitude;

        if (ps)
        {
            // arbitrary
            ParticleSystem.ShapeModule pss = ps.shape;
            pss.radius = (singularityRadius + areaEffectRadius) / 2f;
            pss.radiusThickness = 1 - singularityRadius / pss.radius;

            // delete particles inside the singularity
            ps.trigger.SetCollider(0, cc2Ds[0]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, areaEffectRadius);
    }
}
