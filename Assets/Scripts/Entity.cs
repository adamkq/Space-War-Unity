using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class Entity : MonoBehaviour
{
    // https://stackoverflow.com/questions/295104/what-is-the-difference-between-a-field-and-a-property
    private SpawnPointManager spm;
    private SpriteRenderer spriteRend;
    private Collider2D c2D;

    public byte team;
    public Sprite sprite;
    public Color spriteColor;

    // Start is called before the first frame update
    void Start()
    {
        spm = GameObject.Find("Spawn Point Mgr").GetComponent<SpawnPointManager>();
        spriteRend = GetComponent<SpriteRenderer>();
        c2D = GetComponent<Collider2D>();
        
        if (sprite)
        {
            spriteRend.sprite = sprite;
            spriteRend.color = spriteColor;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void Spawn()
    {
        spm.SpawnEntity(gameObject, team);
    }
}
