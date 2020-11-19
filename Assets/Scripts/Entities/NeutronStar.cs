using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutronStar : Entity
{
    private CircleCollider2D[] cc2Ds;
    private PointEffector2D[] pe2Ds;
    private AreaEffector2D[] ae2Ds;
    private ParticleSystem ps;

    [Tooltip("The number of seconds to complete 1 rotation. Enter 0 to not rotate.")]
    public float rotationalPeriod = 0f;
    [Tooltip("Radius of the center collider.")]
    public float starRadius = 1f;
    [Tooltip("Radii of the push and pull effectors.")]
    public float areaEffectRadius = 4f;
    [Tooltip("The distance between the centers of the effectors and the center of the star.")]
    public float effectorDistance = 6f;
    public float forceMagnitude = 50f;

    protected override void OnValidate()
    {
        base.OnValidate();
        invuln = true;

        starRadius = Mathf.Max(0, starRadius);

        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        pe2Ds = GetComponentsInChildren<PointEffector2D>();
        ae2Ds = GetComponentsInChildren<AreaEffector2D>();
        ps = GetComponent<ParticleSystem>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.isTrigger) // area effect; make clover pattern
            {
                cc2D.radius = areaEffectRadius;
                float xOffset = effectorDistance * (cc2D.gameObject.name.Contains("Pull") ? 1 : 0) * (cc2D.gameObject.name.Contains("1") ? 1 : -1);
                float yOffset = effectorDistance * (cc2D.gameObject.name.Contains("Push") ? 1 : 0) * (cc2D.gameObject.name.Contains("1") ? 1 : -1);
                cc2D.offset = new Vector2(xOffset, yOffset);
            }
            else // star
            {
                cc2D.radius = starRadius;
            }
        }

        sRend.transform.localScale = new Vector3(2 * starRadius, 2 * starRadius, 1f);

        foreach (PointEffector2D pe2D in pe2Ds)
        {
            pe2D.forceMagnitude = -forceMagnitude; // pull
        }
        foreach (AreaEffector2D ae2D in ae2Ds)
        {
            ae2D.forceMagnitude = forceMagnitude/2; // push, reduced because colliders have greater distance to accelerate w/in ae field
        }

        if (ps)
        {
            ParticleSystem.MainModule psMainModule = ps.main;
            psMainModule.startSpeed = 60 / rotationalPeriod;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (rotationalPeriod != 0)
        {
            rb2D.angularVelocity = 360 / rotationalPeriod;
        }
    }
}
