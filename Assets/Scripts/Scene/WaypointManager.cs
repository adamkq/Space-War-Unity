using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    // keep track of the node sequence for a given path
    // first node is the source node index, second is the sink node index
    private int[,] pathMatrix;
    private int maxNodes = 1000;
    
    public bool refresh;

    private void OnValidate()
    {
        refresh = false;
        foreach(Transform child in transform)
        {
            GameObject go = child.gameObject;
            
            Waypoint wp = go.GetComponent<Waypoint>();
            wp.OnValidate();
            go.name = "waypoint" + child.GetSiblingIndex().ToString() + "_" + wp.LOSWPs.Count;
        }
        BuildPathMatrix();
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
        Debug.LogFormat("A* called for nodes {0}, {1}", source, sink);

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

        int i = 0;
        while (open.Count > 0 && i < maxNodes)
        {
            // find minimum f(n) dictionary
            
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

                List<int> path = new List<int>();
                
                while(parent.Keys.Contains(current))
                {
                    int ci = current.GetSiblingIndex(), pi = parent[current].GetSiblingIndex();
                    // the parent dict can be backtracked to get the reverse path
                    path.Add(parent[current].GetSiblingIndex());
                    // path from parent to sink starts with current
                    // path from current to source starts with parent
                    pathMatrix[pi, sink] = ci;
                    pathMatrix[ci, source] = pi;
                    current = parent[current];
                }
                print("Back Path:");
                foreach (var index in path)
                {
                    print(index);
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
            i++;
        }
        Debug.LogWarningFormat("Warning: no path found for waypoints with sibling indexes {0}, {1}", source, sink);
    }
    

    public List<GameObject> GetPath(int source, int sink)
    {
        // the actual points for an agent to navigate, based on sibling index

        return new List<GameObject>();
    }

    public int GetClosestWaypointWithLOS(GameObject go)
    {
        // the index of the waypoint closest to an agent with LOS
        // LOS could be blocked by hazards, so if no nodes are found with LOS, just return the closest node
        float dist = float.PositiveInfinity;
        int siblingIndex = -1;

        foreach(Transform child in transform)
        {
            float _dist = Vector2.Distance(go.transform.position, child.position);
            if (_dist < dist && HasLOS(go, child.gameObject))
            {
                dist = _dist;
                siblingIndex = child.GetSiblingIndex();
            }
        }
        return siblingIndex;
    }

    public static bool HasLOS(GameObject go1, GameObject go2)
    {
        // draw ray from pos1 to pos2
        // go2 must have a collider attached
        Vector2 direction = go2.transform.position - go1.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(go1.transform.position, direction);

        // rays hit triggers, so continue through triggers that aren't waypoints
        int maxIterations = 50;
        while (hit.collider.isTrigger && hit.collider.gameObject != go2 && maxIterations > 0)
        {
            maxIterations -= 1;
            // offset the new ray a bit so it doesn't just hit the same trigger repeatedly
            hit = Physics2D.Raycast(hit.point + direction * 0.0001f, direction);
        }

        if (hit.collider == go2.GetComponent<Collider2D>())
        {
            return true;
        }
        return false;
    }
}
