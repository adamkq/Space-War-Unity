using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    // this class manages all the spawn points.
    // in general the exact location of the spawn point is needed, only:
    // a) if it is occupied, b) the team it belongs to, and c) the spawn delay
    // this class can then choose an appropriate spawner at random.

    private SpawnPoint[] spScripts;
    private readonly System.Random rnd = new System.Random();

    public GameObject asteroid;
    public GameObject enemy;

    // Start is called before the first frame update
    void Start()
    {
        // sort all spawns by team
        spScripts = GetComponentsInChildren<SpawnPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        ManualSpawn();
    }

    void ManualSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnEntity(enemy, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnEntity(enemy, 2);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Destroy(GameObject.Find("enemy"));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnEntity(asteroid);
        }
    }

    public void SpawnEntity(GameObject obj, byte team=0)
    {
        // select a suitable spawner at random (if none, just choose an unsuitable one)
        // Order by priority: 1) Team, 2) Unoccupied, 3) Spawn Delay
        // team 0 is the universal team
        // TODO: add team status to entities
        Debug.LogFormat("Spawning {0}, Team {1}", obj.name, team);
        

        GameObject newObj;
        // if no spawns, instantiate at the parent transform
        // this requires an instance of the SPM
        if (spScripts.Length == 0)
        {
            newObj = Instantiate(obj, transform);
            Debug.LogWarningFormat("Object {0} (Team {1}) Instantiated at default point.", obj.name, team);
        }
        else
        {
            List<SpawnPoint> spScriptsOfTeam = new List<SpawnPoint>();
            foreach(var script in spScripts)
            {
                if (team == 0 || script.team == 0 || script.team == team)
                {
                    spScriptsOfTeam.Add(script);
                }
            }

            // if no eligible spawns, choose any one
            if (spScriptsOfTeam.Count == 0)
            {
                spScriptsOfTeam = new List<SpawnPoint>(spScripts);
            }

            // choose random spawn. cycle through list of spawn points twice max
            int spCount = spScriptsOfTeam.Count;
            int maxIterations = spCount * 2;
            int index;
            SpawnPoint spawnPoint;

            do
            {
                maxIterations -= 1;
                index = rnd.Next(spCount);
                spawnPoint = spScriptsOfTeam[index];
            }
            while (maxIterations > 0 && spawnPoint.IsOccupied());

            newObj = Instantiate(obj, spawnPoint.gameObject.transform.position, spawnPoint.gameObject.transform.rotation);
        }

        // prevent an extra (clone) suffix
        newObj.name = obj.name;
        newObj.GetComponent<Entity>().team = team;
        

        // group in hierarchy
        //newObj.transform.SetParent(GameObject.Find("Spawned Objects").transform);
    }
}
