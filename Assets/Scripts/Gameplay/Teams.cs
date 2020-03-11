using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// teams
[System.Serializable]
public enum Team
{
    NoTeam,
    One,
    Two,
    Three,
    Four,
    Five,
    Six
};

public static class Teams
{
    public static Dictionary<Team, Color> teamColors = new Dictionary<Team, Color>
    {
        // first 4 colors selected based on 'tetradic' scheme
        { Team.NoTeam, Color.white },
        { Team.One, new Color(0.15f, 0.15f, 1f) }, // blueish
        { Team.Two, new Color(1f, 0.15f, 0.15f) }, // reddish
        { Team.Three, new Color(0.15f, 1f, 0.15f) }, // greenish
        { Team.Four, new Color(1f, 0.63f, 0.25f) }, // orangish
        { Team.Five, Color.cyan },
        { Team.Six, Color.magenta }
    };
}
