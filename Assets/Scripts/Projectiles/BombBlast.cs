using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBlast : MonoBehaviour
{
    CircleCollider2D cc2D;
    PointEffector2D pe2D;

    GameObject FiredBy;
    Team team;
    int maxBlastDamage;
    bool noDamageFallOff;

    private void Start()
    {
        // Duplicate info. I want to avoid having params in this script.
        Bomb _bomb = gameObject.GetComponentInParent<Bomb>();

        FiredBy = _bomb.FiredBy;
        maxBlastDamage = _bomb.damage;
        team = _bomb.team;
        noDamageFallOff = _bomb.noDamageFallOff;

        // adjust blast characteristics
        cc2D = gameObject.GetComponent<CircleCollider2D>();
        cc2D.radius = _bomb.blastRadius;

        pe2D = gameObject.GetComponent<PointEffector2D>();
        pe2D.forceMagnitude = _bomb.blastForce;
    }

    // upon detonation, called once per object in the blast zone
    // "detonation" involves enabling this collider for at least one timestep, then destroying the GO.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject go = collision.gameObject;

        // apply damage based on distance, for which we need a rigidbody
        // ignore kinematic rb2Ds
        Rigidbody2D rb2DBlast = go.GetComponent<Rigidbody2D>();
        if (!rb2DBlast || rb2DBlast.isKinematic)
        {
            return;
        }

        // chain reaction. Bombs don't inherit from Entity, so they don't have a team.
        if (go.CompareTag("Projectile") && go.name.ToLower().Contains("bomb"))
        {
            go.GetComponent<Bomb>().IncrementHealth(-999);
            return;
        }

        // damage; agents track who they were last hit by, so they need a special function
        // no self-damage
        Vector2 relPos = rb2DBlast.position - (Vector2)transform.position;
        Team otherTeam = go.GetComponent<Entity>().team;
        int blastDamage;

        if (otherTeam != team || otherTeam == Team.NoTeam)
        {
            blastDamage = noDamageFallOff ? maxBlastDamage : Mathf.FloorToInt(maxBlastDamage / relPos.magnitude);
            if (go.CompareTag("Hazard")) go.GetComponent<Entity>().IncrementHealth(-blastDamage);
            else if (go.CompareTag("Agent")) go.GetComponent<Agent>().ApplyBombBlastDamage(-blastDamage, FiredBy);
        }
    }
}
