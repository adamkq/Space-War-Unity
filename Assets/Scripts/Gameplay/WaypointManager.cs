using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public bool refresh;

    private void OnValidate()
    {
        // preprocess the whole map
        refresh = false;
        foreach(Transform child in transform)
        {
            GameObject go = child.gameObject;
            
            Waypoint wp = go.GetComponent<Waypoint>();
            wp.OnValidate();
            go.name = "waypoint" + child.GetSiblingIndex().ToString() + "_" + wp.LOSWPs.Count;
        }
    }
}
