using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]

public class Waypoint : MonoBehaviour
{
    public Dictionary<Waypoint, float> LOSWPs = new Dictionary<Waypoint, float>();

    // click to validate
    public bool refresh;

    public void OnValidate()
    {
        refresh = false;
        // find distances to all waypoints with LOS
        LOSWPs.Clear();
        Waypoint[] allWPs = transform.parent.gameObject.GetComponentsInChildren<Waypoint>();

        foreach (Waypoint wp in allWPs)
        {
            if (wp.gameObject == gameObject)
            {
                // no self-LOS
                continue;
            }

            // cast ray to wp
            // rays hit triggers, so it might give a false positive on a certain ray
            Vector2 direction = wp.gameObject.transform.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction);
            // may use gizmos/handles for this bit
            Debug.DrawLine(transform.position, hit.point, Color.green);

            if (hit.collider == wp.gameObject.GetComponent<CircleCollider2D>())
            {
                LOSWPs.Add(wp, direction.magnitude);
            }
        }

        Debug.LogFormat("{0} LOS: {1}", name, LOSWPs.Count);

        LineRenderer line = GetComponent<LineRenderer>();
        float radius = GetComponent<CircleCollider2D>().radius;
        GetComponent<Highlighter>().DrawPolygon(line, radius, Color.green, 4);
    }
}
