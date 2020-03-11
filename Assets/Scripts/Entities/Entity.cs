using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class Entity : MonoBehaviour
{
    // https://github.com/JetBrains/resharper-unity/wiki/Performance-critical-context-and-costly-methods


    // https://stackoverflow.com/questions/295104/what-is-the-difference-between-a-field-and-a-property
    private SpawnPointManager spm;
    private SpriteRenderer spriteRend;
    private Collider2D c2D;
    // dynamic rigidbody?

    public Team team;

    public Sprite sprite;
    public Color spriteColor;

    protected virtual void Start()
    {
        spm = GameObject.Find("Spawn Point Mgr").GetComponent<SpawnPointManager>();
        spriteRend = GetComponent<SpriteRenderer>();
        c2D = GetComponent<Collider2D>();
        
        if (sprite)
        {
            spriteRend.sprite = sprite;
            if (team != Team.NoTeam)
            {
                spriteRend.color = Teams.teamColors[team];
            }
        }
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void Respawn()
    {
        GameObject newObj = spm.SpawnEntity(gameObject, team);
        TrackCamera camera = GameObject.Find("Main Camera").GetComponent<TrackCamera>();

        if (camera.target == gameObject)
        {
            camera.ChangeTarget(newObj);
        }
    }
}
