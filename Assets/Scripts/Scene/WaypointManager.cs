using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    // keep track of the node sequence for a given path
    // first node is the source node index, second is the sink node index
    private static int[,] pathMatrix;
    // allows certain functions to be static
    private static Transform parentTransform;

    // number of waypoints
    public static int NumberOfWPs { get; private set; }

    public bool refresh;

    private void OnValidate()
    {
        refresh = false;
        parentTransform = transform;
        NumberOfWPs = transform.childCount;

        foreach (Transform child in transform)
        {
            GameObject go = child.gameObject;
            
            Waypoint wp = go.GetComponent<Waypoint>();
            wp.OnValidate();
            go.name = "waypoint" + child.GetSiblingIndex().ToString() + "_" + wp.LOSWPs.Count;
        }

        Debug.Log("Building Waypoint Matrix...");
        BuildPathMatrix();
        Debug.Log("Matrix Complete");


    }
    
    private void BuildPathMatrix()
    {
        // preprocess the whole map
        int nodes = transform.childCount;
        pathMatrix = new int[nodes, nodes];

        if (nodes < 2) return;

        // set all values to -1 to indicate that they have not yet been checked
        // after population, the -1 value will end up on the diagonal of the Path Matrix.
        for (int i = 0; i < nodes; i++)
        {
            for (int j = 0; j < nodes; j++)
            {
                pathMatrix[i, j] = -1;
            }
        }

        // call A* on Source, Sink.
        for (int i = 0; i < nodes; i++)
        {
            for (int j = i + 1; j < nodes; j++)
            {
                AStar(i, j);
            }
        }
    }

    private void AStar(int source, int sink)
    {
        // A* search for nodes based on sibling index
        // If the search encounters a pre-existing path, it will stop early
        // https://brilliant.org/wiki/a-star-search/
        // https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2

        // f(n) = g(n) (dist so far) + h(n) (estimated dist to end)
        // if the sub-path has already been found, then h(n) = the known path length
        // if not, h(n) = euclidean distance (admissible heuristic)

        // frontier of nodes and their g-scores
        Dictionary<Transform, float> open = new Dictionary<Transform, float>();
        // successors; allows reconstruction of path
        Dictionary<Transform, Transform> parent = new Dictionary<Transform, Transform>();
        // already checked
        List<Transform> closed = new List<Transform>();

        open.Add(transform.GetChild(source), 0f);

        while (open.Count > 0 && closed.Count < NumberOfWPs)
        {
            // find minimum f(n) in open
            
            Transform current = open.Keys.First();
            float _gN = open[current];
            foreach (Transform tf in open.Keys)
            {
                float hN = Vector2.Distance(tf.position, transform.GetChild(sink).position);
                float _hN = Vector2.Distance(current.position, transform.GetChild(sink).position);
                
                if (open[tf] + hN < _gN + _hN)
                {
                    _gN = open[tf];
                    current = tf;
                }
            }

            closed.Add(current);

            // check if at goal
            if (current == transform.GetChild(sink) || pathMatrix[current.GetSiblingIndex(), sink] != -1)
            {
                // sink found, populate pathMatrix with path found in from.
                // If current is in the pathMatrix and has a path to sink, also return
                while(parent.Keys.Contains(current))
                {
                    int ci = current.GetSiblingIndex(), pi = parent[current].GetSiblingIndex();
                    // path from parent to sink starts with current
                    // path from current to source starts with parent
                    pathMatrix[pi, sink] = ci;
                    pathMatrix[ci, source] = pi;
                    current = parent[current];
                }
                return;
            }

            Waypoint wp = current.gameObject.GetComponent<Waypoint>();

            // iterate thru neighbors
            foreach (Waypoint wpNext in wp.LOSWPs.Keys)
            {
                Transform neighbor = wpNext.gameObject.transform;
                if (closed.Contains(neighbor)) continue;

                // score; should account for TODO wormholes once I program those
                float gN = open[current] + wp.LOSWPs[wpNext];

                if (open.ContainsKey(neighbor))
                {
                    // this path is the best found so far
                    if (open[neighbor] > gN)
                    {
                        open[neighbor] = gN;
                        parent[neighbor] = current;
                    }
                }
                else
                {
                    open.Add(neighbor, gN);
                    parent[neighbor] = current;
                }

            }
            open.Remove(current);
        }
        // path not found
        Debug.LogWarningFormat("Warning: no path found for waypoints with sibling indexes {0}, {1}", source, sink);
    }
    
    public static List<GameObject> GetPath(int source, int sink)
    {
        // the actual points for an agent to navigate, based on sibling index
        List<GameObject> path = new List<GameObject>();

        if (!(-1 < source && source < NumberOfWPs) || !(-1 < sink && sink < NumberOfWPs)) return path;


        while (source != -1 && source != sink && path.Count < NumberOfWPs)
        {
            path.Add(parentTransform.GetChild(source).gameObject);
            source = pathMatrix[source, sink];
        }
        
        path.Add(parentTransform.GetChild(sink).gameObject);

        return path;
    }

    public static int GetClosestWaypointWithLOS(GameObject go)
    {
        // the index of the waypoint closest to an agent with LOS
        // LOS could be blocked by hazards, so if no nodes are found with LOS, iterate again w/o LOS
        float dist = float.PositiveInfinity;
        int siblingIndex = -1;

        foreach(Transform child in parentTransform)
        {
            float _dist = Vector2.Distance(go.transform.position, child.position);
            if (_dist < dist && HasLOS(go, child.gameObject, false))
            {
                dist = _dist;
                siblingIndex = child.GetSiblingIndex();
            }
        }

        if (siblingIndex > -1) return siblingIndex;

        // check for hazards
        foreach (Transform child in parentTransform)
        {
            float _dist = Vector2.Distance(go.transform.position, child.position);
            if (_dist < dist && HasLOS(go, child.gameObject))
            {
                dist = _dist;
                siblingIndex = child.GetSiblingIndex();
            }
        }

        if (siblingIndex == -1)
        {
            Debug.LogWarningFormat("Gameobject '{0}' could not find LOS to any waypoint at location {1}", go.name, go.transform.position);
        }
        return siblingIndex;
    }

    public static bool HasLOS(GameObject go1, GameObject go2, bool ignoreEntities=true)
    {
        // draw ray from go1 to go2
        // go2 must have a collider attached
        // TODO: Look into collision masking

        if (!(go1 && go2)) return false;

        Vector2 direction = go2.transform.position - go1.transform.position;
        Vector2 hitPoint = go1.transform.position;
        RaycastHit2D hit;
        // could have this return the collider to the calling function to avoid this GC call.
        Collider2D c2D = go2.GetComponent<Collider2D>();

        if (!c2D) return false;

        do
        {
            // offset the new ray a bit so it doesn't just hit the same trigger repeatedly
            hit = Physics2D.Raycast(hitPoint + direction * 0.0001f, direction);
            if (!hit.collider) break; // null

            hitPoint = hit.point;

            // rays hit triggers, so continue through these (projectiles, spawnpoints, other such objects)
            // break if it hits a wall or (hits an entity and entities aren't being ignored)
            if (!hit.collider.isTrigger)
            {
                if (hit.collider.gameObject.CompareTag("Untagged")) break; // assume anything that is unspecified stops the ray

                if (!(ignoreEntities && (hit.collider.gameObject.CompareTag("Hazard") || hit.collider.gameObject.CompareTag("Agent")))) break;
            }
        } while (hit.collider != c2D);

        return hit.collider == c2D;
    }
}
