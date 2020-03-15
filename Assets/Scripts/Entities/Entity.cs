using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class Entity : MonoBehaviour
{
    // https://github.com/JetBrains/resharper-unity/wiki/Performance-critical-context-and-costly-methods
    // https://stackoverflow.com/questions/295104/what-is-the-difference-between-a-field-and-a-property
    private SpriteRenderer spriteRend;

    internal Rigidbody2D rb2D;
    internal int health;

    public Sprite sprite;
    public Color spriteColor;

    
    public bool invuln;
    public int startHealth;

    public Team team;

    protected virtual void OnValidate()
    {
        startHealth = Mathf.Max(startHealth, 1);
    }

    protected virtual void Awake()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        if (sprite) spriteRend.sprite = sprite;

        health = startHealth;
    }

    protected virtual void Start()
    {
        ChangeTeamColor();
    }

    protected virtual void Update()
    {
        ChangeTeamColor();
    }

    private void ChangeTeamColor()
    {
        spriteRend.color = (team == Team.NoTeam) ? spriteColor : spriteRend.color = Teams.teamColors[team];
    }

    internal void IncrementHealth(int dHealth)
    {
        if (invuln && dHealth < 0) return;

        health += dHealth;
    }

    public void Respawn()
    {
        // to do: add to spawn queue
        Color foo = Color.gray;
        GameObject newObj = SpawnPointManager.SpawnEntity(gameObject, team);
        TrackCamera camera = GameObject.Find("Main Camera").GetComponent<TrackCamera>();

        if (camera.target == gameObject) camera.ChangeTarget(newObj);
    }
}
