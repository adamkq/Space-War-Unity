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

    // Start is called before the first frame update
    void Start()
    {
        OnValidate();
        line.enabled = false;
        radius = GetComponent<CircleCollider2D>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        //occupied = false;
    }

    private void OnValidate()
    {
        gameObject.name = "spawn_" + team.ToString();
        GetComponent<CircleCollider2D>().radius = radius;

        line = GetComponent<LineRenderer>();
        line.colorGradient.mode = GradientMode.Fixed;
        line.startColor = Teams.teamColors[team];
        line.endColor = line.startColor;

        DrawCircle(GetComponent<CircleCollider2D>().radius);
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
    
    void DrawCircle(float radius)
    {
        int angleIncrement = 10; // degrees; higher = lower res circle
        line.positionCount = 360/angleIncrement;

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = (float)i * angleIncrement * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(transform.position.x + radius * Mathf.Cos(angle), transform.position.y + radius * Mathf.Sin(angle), transform.position.z);
            line.SetPosition(i, pos);
        }
        
    }
}
