using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // triggers do not terminate projectiles
    internal static readonly HashSet<string> exemptTriggers = new HashSet<string>() { "Projectile", "Respawn", "Waypoint", "AreaEffect" };
    // these walls do not terminate projectiles
    internal static readonly HashSet<WallType> exemptWalls = new HashSet<WallType>() { WallType.Bouncy, WallType.ProjectilePass, WallType.HotWall };

    internal Rigidbody2D rb2D;
    internal float timeFired;

    public float speed = 10.0f;
    public int damage = 1;
    public GameObject FiredBy { get; set; }

    protected virtual void Start()
    {
        transform.parent = GameObject.Find("Projectiles").transform;
        timeFired = Time.time;
        if (name.Contains("(Clone)")) name = name.Substring(0, name.Length - "(Clone)".Length);
        name += "_";
        if (FiredBy) name += FiredBy.name;
    }

    protected virtual void Update()
    {
        
    }

    public void FireProjectile(GameObject projectile, Transform tform, float speed, float lifetime=30f, int damage=1)
    {
        // Take bullet from queue
        // Set its tform (This is needed to make sure that the bullet doesn't collide with the agent firing it)
        // set other params
    }
}
