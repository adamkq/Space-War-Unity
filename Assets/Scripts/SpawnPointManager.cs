using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    // this class manages all the spawn points.
    // in general the exact location of the spawn point is needed, only:
    // a) if it is occupied, b) the team it belongs to, and c) the spawn delay
    // this class can then choose an appropriate spawner at random.

    private KeyCode[] alphaKeyCodes =
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6
    };

    private SpawnPoint[] spawnPoints;
    private readonly System.Random rnd = new System.Random();

    public GameObject asteroid;
    public GameObject enemy;

    // Start is called before the first frame update
    void Start()
    {
        // sort all spawns by team
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        ManualSpawn();
    }

    void ManualSpawn()
    {
        for (int i = 0; i < alphaKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(alphaKeyCodes[i]))
            {
                SpawnEntity(enemy, (Team)i);
            }
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

    public GameObject SpawnEntity(GameObject obj, Team team= Team.NoTeam)
    {
        // https://answers.unity.com/questions/710968/how-to-tell-if-a-gameobject-is-an-instance-or-a-pr.html
        // select a suitable spawner at random (if none, just choose an unsuitable one)
        // Order by priority: 1) Team, 2) Unoccupied, 3) Spawn Delay
        // team 0 is the universal team
        // TODO: add team status to entities
        Debug.LogFormat("Spawning {0}, Team {1}", obj.name, team);
        

        GameObject newObj;
        // if no spawns, instantiate at the parent transform
        // this requires an instance of the SPM
        if (spawnPoints.Length == 0)
        {
            newObj = Instantiate(obj, transform);
            Debug.LogWarningFormat("Object {0} (Team {1}) Instantiated at default point of parent {2}", obj.name, team, name);
        }
        else
        {
            List<SpawnPoint> spawnsOfTeam = new List<SpawnPoint>();
            // get list of spawns that match the team and that are unoccupied
            foreach(var sp in spawnPoints)
            {
                if (sp.team == team && !sp.IsOccupied())
                {
                    spawnsOfTeam.Add(sp);
                }
            }

            // if no eligible spawns, add the universal spawns
            if (spawnsOfTeam.Count == 0 && team != Team.NoTeam)
            {
                foreach (var sp in spawnPoints)
                {
                    if (sp.team == Team.NoTeam)
                    {
                        spawnsOfTeam.Add(sp);
                    }
                }
            }

            // if still none, add all spawns
            if (spawnsOfTeam.Count == 0)
            {
                spawnsOfTeam = new List<SpawnPoint>(spawnPoints);
            }

            // choose random spawn from those eligible.
            Transform tx = spawnsOfTeam[rnd.Next(spawnsOfTeam.Count)].gameObject.transform;

            newObj = Instantiate(obj, tx.position, tx.rotation);
        }

        // prevent an extra (clone) suffix
        newObj.name = obj.name;
        newObj.GetComponent<Entity>().team = team;

        return newObj;
    }
}
