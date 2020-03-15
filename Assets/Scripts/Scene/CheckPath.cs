using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaypointManager))]

public class CheckPath : MonoBehaviour
{

    [Tooltip("Check path from this node.")]
    public int sourceIndex;
    [Tooltip("Check path to this node.")]
    public int sinkIndex;

    void OnValidate()
    {
        sourceIndex = Mathf.Clamp(sourceIndex, -1, WaypointManager.NumberOfWPs);
        sinkIndex = Mathf.Clamp(sinkIndex, -1, WaypointManager.NumberOfWPs);

        if (Mathf.Min(sourceIndex, sinkIndex) == -1) return;

        Debug.LogFormat("Check Path from {0}, {1}:", sourceIndex, sinkIndex);
        List<GameObject> samplePath = WaypointManager.GetPath(sourceIndex, sinkIndex);
        string s = "";
        foreach (GameObject go in samplePath)
        {
            s += go.transform.GetSiblingIndex().ToString() + " ";
        }
        Debug.Log(s);
    }

}
