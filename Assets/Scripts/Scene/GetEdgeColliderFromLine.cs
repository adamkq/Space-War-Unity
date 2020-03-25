using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]

public class GetEdgeColliderFromLine : MonoBehaviour
{
    // Generate Capusle Colliders 2D from a line segment. This allows easy
    // creation of complex wall objects at runtime with very few pre-existing
    // components. The capsule dimensions are based on the dimensions of the
    // line as well as the line's End Cap Vertices.

    // Assume constant width. Varied line width is stored in an 'animation curve'.

    LineRenderer lRend;
    EdgeCollider2D ec2D;
    Vector2[] verts;

    public bool setTriggerColliders; // if true, the script will set the colliders as triggers so that the linewall can do something other than physics
    public bool derenderLineOnRuntime; // should the line be visible when the game runs?
    public float wallOffset; // increase or decrease the collider size relative to the line

    void OnValidate()
    {
        lRend = GetComponent<LineRenderer>();

        ec2D = gameObject.GetComponent<EdgeCollider2D>();
        verts = new Vector2[lRend.positionCount + (lRend.loop ? 1 : 0)];
        ec2D.edgeRadius = Mathf.Max(0, lRend.startWidth/2 + wallOffset);

        for (int i = 0; i < lRend.positionCount; i++)
        {
            verts[i] = lRend.GetPosition(i);
        }

        if (lRend.loop) verts[lRend.positionCount] = verts[0];

        ec2D.points = verts;

        // LineWall supersedes this
        if (!GetComponent<LineWall>()) ec2D.isTrigger = setTriggerColliders;
    }

    private void Start()
    {
        OnValidate();
        lRend.enabled = !derenderLineOnRuntime;
    }
}
