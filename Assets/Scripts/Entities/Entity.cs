﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class Entity : MonoBehaviour
{

    internal SpriteRenderer sRend;

    internal Rigidbody2D rb2D;
    internal Collider2D c2D;
    // basic
    public bool alive { get; internal set; }
    internal int health;

    // used for respawns and teleports
    internal bool wasKinematic;
    
    public Sprite sprite;
    public Color spriteColor;

    public Team team;

    public bool invuln;
    public int startHealth = 1;

    protected virtual void OnValidate()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Awake()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        c2D = GetComponent<Collider2D>();

        if (sprite) sRend.sprite = sprite;

        health = startHealth;
        alive = true;
    }

    protected virtual void Start()
    {
        startHealth = Mathf.Max(startHealth, 1);
        ChangeTeamColor();
    }

    protected virtual void Update()
    {
        
    }

    private void ChangeTeamColor()
    {
        // fixes odd bug where Asteroid keeps calling the OnValidate fcn
        if (sprite && !gameObject.name.Contains("asteroid"))
        {
            sRend.color = (team == Team.NoTeam) ? spriteColor : sRend.color = Teams.teamColors[team];
        }
    }

    internal void Deactivate()
    {
        sRend.enabled = false;
        c2D.enabled = false;
        wasKinematic = rb2D.isKinematic;
        rb2D.isKinematic = true;
        rb2D.velocity = Vector2.zero;
    }

    internal void Activate(Vector2 initialVel)
    {
        sRend.enabled = true;
        c2D.enabled = true;
        rb2D.isKinematic = wasKinematic;
        rb2D.velocity = initialVel;
    }

    internal IEnumerator RespawnWithDelay(float delay = 0f, GameObject killedBy = null)
    {
        if (killedBy)
        {
            Debug.LogFormat("{0} (Team {1}) killed by {2} (Team {3}).", name, team, killedBy.name, killedBy.GetComponent<Entity>().team);
        }
        else
        {
            Debug.LogFormat("{0} (Team {1}) killed.", name, team);
        }

        alive = false;
        Deactivate();

        yield return new WaitForSeconds(delay);

        Debug.LogFormat("Respawning {0} (Team {1}).", name, team);

        GameObject spawnPoint = SpawnPointManager.GetSpawnPoint(team);
        rb2D.position = spawnPoint.transform.position;
        rb2D.rotation = spawnPoint.transform.rotation.z;

        yield return new WaitForFixedUpdate(); // keeps entity from 'warping' in one frame

        alive = true;
        health = startHealth;
        Activate(Vector2.zero);
    }

    internal void Teleport(Vector3 position, bool preserveVelocity = true)
    {
        Vector2 initialVel = preserveVelocity ? rb2D.velocity : Vector2.zero;
        Deactivate();
        rb2D.position = position;
        Activate(initialVel);
    }

    internal void IncrementHealth(int dHealth)
    {
        if (invuln && dHealth < 0) return;

        health += dHealth;
    }
}
