using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]

public class SpawnPoint : MonoBehaviour
{
    // script for a single spawn point

    private bool occupied = false;
    private LineRenderer line;

    // set to NoTeam to allow any team to access
    public Team team = Team.NoTeam;
    public float radius = 1f;
    public bool ReValidate;

    // Start is called before the first frame update
    void Start()
    {
        radius = GetComponent<CircleCollider2D>().radius;
    }

    private void OnValidate()
    {
        gameObject.name = "spawn_" + team.ToString();
        GetComponent<CircleCollider2D>().radius = radius;

        line = GetComponent<LineRenderer>();
        GetComponent<Highlighter>().DrawPolygon(line, radius, Teams.teamColors[team], 18);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        occupied = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        occupied = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        occupied = false;
    }

    public bool IsOccupied()
    {
        return occupied;
    }
}
