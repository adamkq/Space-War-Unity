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
    private static GameObject spawnPoint;

    private int enemyIndex; // uniquely name enemies

    public GameObject asteroid;
    public GameObject enemy;

    void Start()
    {
        // sort all spawns by team
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    void Update()
    {
        ManualSpawn();
    }

    
    void ManualSpawn()
    {
        
        // manually instantiate entities
        for (int i = 0; i < alphaKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(alphaKeyCodes[i]))
            {
                Team team = (Team)i;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Agent[] enemies = GameObject.Find("Agents").GetComponentsInChildren<Agent>();
                    foreach (Agent enemy in enemies)
                    {
                        if (enemy.team == team)
                        {
                            Destroy(enemy.gameObject);
                            break;
                        }
                    }
                }
                else
                {
                    spawnPoint = GetSpawnPoint(team);
                    GameObject _enemy = Instantiate(enemy, spawnPoint.transform.position, spawnPoint.transform.rotation);
                    _enemy.name = "enemy" + enemyIndex.ToString();
                    
                    _enemy.GetComponent<Entity>().team = team;
                    Debug.LogFormat("Spawning {0} (Team {1})", _enemy.name, team);

                    ++enemyIndex;
                }
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            spawnPoint = GetSpawnPoint();
            Instantiate(asteroid, spawnPoint.transform.position, spawnPoint.transform.rotation);
        }

        else if (Input.GetKeyUp(KeyCode.A)) Debug.LogFormat("Total Asteroids: {0}", Asteroid.NumberInstantiated);
    }

    public static GameObject GetSpawnPoint(Team team= Team.NoTeam)
    {
        // returns a spawnpoint of the specified team (if available)
        // select a suitable spawner at random (if none, just choose an unsuitable one)
        // Order by priority: 1) Team, 2) Unoccupied, 3) Spawn Delay
        // team 0 is the universal team

        // if no spawns, instantiate at the parent transform
        // this requires an instance of the SPM
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available.");
            return null; 
        }
 
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
        return spawnsOfTeam[Random.Range(0, spawnsOfTeam.Count)].gameObject;
    }
}
