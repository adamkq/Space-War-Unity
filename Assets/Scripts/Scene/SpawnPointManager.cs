using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    // this class manages all the spawn points.
    // spawnpoints selected based on a) if it is occupied, b) the team it belongs to
    // this class can then choose an appropriate spawner at random.

    private static KeyCode[] alphaKeyCodes =
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6
    };

    private static SpawnPoint[] spawnPoints;
    private static Transform parentTransform;

    public GameObject asteroid;
    public GameObject enemy;

    private void Awake()
    {
        parentTransform = transform;
    }
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
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Destroy(GameObject.Find("enemy"));
                }
                else
                {
                    SpawnEntity(enemy, (Team)i);
                }
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            SpawnEntity(asteroid);
            Debug.LogFormat("Total Asteroids: {0}", Asteroid.NumberInstantiated);
        }
    }

    public static GameObject SpawnEntity(GameObject obj, Team team= Team.NoTeam)
    {
        // https://answers.unity.com/questions/710968/how-to-tell-if-a-gameobject-is-an-instance-or-a-pr.html
        // select a suitable spawner at random (if none, just choose an unsuitable one)
        // Order by priority: 1) Team, 2) Unoccupied, 3) Spawn Delay
        // team 0 is the universal team
        Debug.LogFormat("Spawning {0} (Team {1})", obj.name, team);
        

        GameObject newObj;
        // if no spawns, instantiate at the parent transform
        // this requires an instance of the SPM
        if (spawnPoints.Length == 0)
        {
            newObj = Instantiate(obj, parentTransform);
            Debug.LogWarningFormat("Object {0} (Team {1}) Instantiated at default point", obj.name, team);
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

            // choose random spawn from selection
            Transform tx = spawnsOfTeam[Random.Range(0, spawnsOfTeam.Count)].gameObject.transform;

            newObj = Instantiate(obj, tx.position, tx.rotation);
        }

        // prevent an extra (clone) suffix
        newObj.name = obj.name;
        newObj.GetComponent<Entity>().team = team;

        return newObj;
    }
}
