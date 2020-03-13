using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutronStar : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private CircleCollider2D[] cc2Ds;
    private PointEffector2D[] pe2Ds;

    [Tooltip("The number of seconds to complete 1 rotation. Enter 0 to not rotate.")]
    public float rotationalPeriod = 0f;
    [Tooltip("Radius of the center collider.")]
    public float starRadius = 1f;
    [Tooltip("Radius of the push and pull effectors.")]
    public float areaEffectRadius = 4f;
    [Tooltip("The distance between the centers of the two effectors. Should be around 100-150% of radius for best results.")]
    public float effectorDistance = 6f;
    public float forceMagnitude = 50f;

    private void OnValidate()
    {
        starRadius = Mathf.Max(0, starRadius);

        cc2Ds = GetComponentsInChildren<CircleCollider2D>();
        pe2Ds = GetComponentsInChildren<PointEffector2D>();

        foreach (CircleCollider2D cc2D in cc2Ds)
        {
            if (cc2D.isTrigger) // area effect
            {
                cc2D.radius = areaEffectRadius;
                cc2D.offset = new Vector2(0f, 0.5f * effectorDistance * (cc2D.gameObject.name.Contains("ush") ? 1 : -1)); // push
            }
            else // star
            {
                cc2D.radius = starRadius;
            }
        }

        foreach (PointEffector2D pe2D in pe2Ds)
        {
            pe2D.forceMagnitude = Mathf.Sign(pe2D.forceMagnitude) * forceMagnitude;
        }
        
    }

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (rotationalPeriod != 0)
        {
            rb2D.angularVelocity = 360 / rotationalPeriod;
        }
    }
}
