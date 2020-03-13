using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicAreaEffect : MonoBehaviour
{
    private PointEffector2D pe2D;
    // Start is called before the first frame update
    void Awake()
    {
        pe2D = GetComponent<PointEffector2D>();
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
