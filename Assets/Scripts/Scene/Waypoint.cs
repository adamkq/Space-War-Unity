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
            if (wp.gameObject == gameObject) continue; // no self-LOS

            // ignore hazards during gameplay but not during pre-game path matrix phase
            if (WaypointManager.HasLOS(gameObject, wp.gameObject, false, new HashSet<WallType>() { WallType.EntityPass }))
            {
                Vector2 direction = wp.gameObject.transform.position - transform.position;
                LOSWPs.Add(wp, direction.magnitude);
            }
        }

        LineRenderer line = GetComponent<LineRenderer>();
        float radius = GetComponent<CircleCollider2D>().radius;
        GetComponent<Highlighter>().DrawPolygon(line, radius, Color.green, 4);
    }

    private void OnDrawGizmosSelected()
    {
        foreach (KeyValuePair<Waypoint, float> entry in LOSWPs)
        {
            Waypoint wp = entry.Key;
            Vector2 direction = wp.gameObject.transform.position - transform.position;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direction);
        }
        
    }
}
