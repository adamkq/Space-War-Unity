using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// teams
[System.Serializable]
public enum WallType
{
    Default, // moderate friction and bounce. Terminates projectiles.
    Bouncy, // low friction, high bounce. Bounces projectiles.
    EntityPass, // only allows entities through (terminates projectiles).
    ProjectilePass, // only allows projectiles through.
    HotWall, // high friction and bounce. Hurts entities, adds damage to projectiles 
};

public static class Walls
{
    public static Dictionary<WallType, Color> wallColors = new Dictionary<WallType, Color>
    {
        { WallType.Default, Color.white },
        { WallType.Bouncy, new Color(1f, 0.5f, 1f) }, // purple
        { WallType.EntityPass, new Color(0.25f, 1f, 0.25f) }, // green
        { WallType.ProjectilePass, new Color(0.25f, 0.25f, 1f) }, // blue
        { WallType.HotWall, new Color(1f, 0.25f, 0.25f) }, // red
    };
}

