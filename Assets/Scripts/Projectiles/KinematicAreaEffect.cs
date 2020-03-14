using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicAreaEffect : MonoBehaviour
{
    // select behaviour based on effector type
    private PointEffector2D pe2D;
    private AreaEffector2D ae2D;

    // Start is called before the first frame update
    void Awake()
    {
        pe2D = GetComponent<PointEffector2D>();
        ae2D = GetComponent<AreaEffector2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // apply a pseudo-force to every kinematic rb2D in the area of influence
        GameObject other = collision.gameObject;
        Rigidbody2D rb2Dk = other.GetComponent<Rigidbody2D>();
        if (rb2Dk && rb2Dk.isKinematic)
        {
            Vector2 relPos = rb2Dk.worldCenterOfMass - (Vector2)transform.position; 
            float forceMag = 0f;

            if (pe2D) // inv linear
            {
                forceMag = pe2D.forceMagnitude;
            }
            else if (ae2D)
            {
                forceMag = ae2D.forceMagnitude;
                // transform rotation relative to up-axis, but ae force angle relative to right axis
                relPos = Quaternion.AngleAxis(transform.parent.rotation.z + ae2D.forceAngle - 90f, Vector3.forward) * Vector2.up;
            }
            
            relPos.Normalize();
            rb2Dk.velocity += relPos * forceMag * Time.fixedDeltaTime;
        }
    }
}
