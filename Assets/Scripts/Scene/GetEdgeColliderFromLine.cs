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

    LineRenderer line;
    EdgeCollider2D ec2D;
    int numVertices;
    Vector2[] verts;

    public bool setTriggerColliders; // if true, the script will set the colliders as triggers so that the linewall can do something other than physics
    public bool derenderLineOnRuntime; // should the line be visible when the game runs?
    public float wallOffset; // increase or decrease the collider size relative to the line

    public PhysicsMaterial2D mat2D; // specify friction or bounce in the material

    // Start is called before the first frame update
    void OnValidate()
    {
        line = GetComponent<LineRenderer>();
        numVertices = line.positionCount;

        ec2D = gameObject.GetComponent<EdgeCollider2D>();
        verts = new Vector2[numVertices + (line.loop ? 1 : 0)];
        ec2D.edgeRadius = Mathf.Max(0, line.startWidth/2 + wallOffset);
        line.enabled = !derenderLineOnRuntime;

        for (int i = 0; i < numVertices; i++)
        {
            verts[i] = line.GetPosition(i);
        }

        if (line.loop) verts[numVertices] = verts[0];

        ec2D.points = verts;

        if (mat2D && !setTriggerColliders) ec2D.sharedMaterial = mat2D;
    }
}
