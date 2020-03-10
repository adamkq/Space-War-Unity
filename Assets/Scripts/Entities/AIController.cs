using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]

public class AIController : MonoBehaviour
{
    // Get AI target and find dynamics/weapons commands. Component for agent.
    // Debugging Lines color convention: blue = target, green = guidance/autopilot, red = weapons
    private Agent agent;
    private Agent.Dynamics dyn;

    [System.Serializable]
    // TODO: Flesh this out. Some overall descriptions:
    // Aggressive: Default, heads directly to target at max speed
    // Defensive: Stays near score objects, or if there are none, get to just within LOS of target
    // Ambush: Attempts to place itself in the likely path of its target, then wait until target is in range to attack
    // Patrol: Wanders to waypoints in a looping sequence
    public enum Modes { Aggressive, Defensive, Ambush, Patrol };

    [System.Serializable]
    public class Targeting
    {
        // Holds all params related to target search and follow
        // These are used by other subsystems to actually move the agent
        internal Rigidbody2D rb2D; // used by guidance to get closing vel

        public GameObject target; // enemy will home in on this
        public Modes mode;

    }

    [System.Serializable]
    public class Guidance
    {
        // Finds point (or sequence of points) to go to based on relative target location, raycasts, etc. Ignores dynamics.
        // May replace with navmesh

        // target pos in relation to enemy pos
        internal Vector2 LOS = Vector3.zero;
        internal Vector2 relVelocity = Vector2.zero;
        internal Vector2 accCommand = Vector2.zero;
        internal float angleLOS;

        // gains
        public float setPointSpeed;
    }

    [System.Serializable]
    public class Autopilot
    {
        // Acts as a go-between for the Guidance and Dynamics subsystems. Ignores target info.

        // Use PID control to help the enemy maneuver. May replace with state-space model.
        internal float angleErrorIntegral = 0f;

        // forces and autopilot params
        public float gainThrottle, gainTurnP, gainTurnI, gainTurnD;
    }

    public Targeting targeting;
    public Guidance guidance;
    public Autopilot autopilot;

    void Start()
    {
        agent = GetComponent<Agent>();
        dyn = agent.dynamics;
    }

    // since these fcns involves physics measurements (e.g. rb2D velocities), they are called in FixedUpdate
    void FixedUpdate()
    {
        UpdateTargeting();
        UpdateGuidance();
        UpdateAutopilot();
        AIFireWeapons();
    }

    void UpdateTargeting()
    {
        // high-level, takes into account other Agent locations, health, objectives, etc
        if (targeting.target)
        {
            // presently, only change targets if the previous one is destroyed
            Debug.DrawLine(transform.position, targeting.target.transform.position, Color.blue);
            return;
        }
        Entity[] agents = GameObject.Find("Agents").GetComponentsInChildren<Entity>();

        foreach (var agent in agents)
        {
            if (agent.team != GetComponent<Entity>().team)
            {
                targeting.target = agent.gameObject;
            }
        }
        if (targeting.target)
        {
            targeting.rb2D = targeting.target.GetComponent<Rigidbody2D>();
        }
    }

    void UpdateGuidance()
    {
        // mid-level, plots a set of waypoints to the objective target. Also sets speed for autopilot.
        // https://www.youtube.com/watch?v=jvtFUfJ6CP8


        if (targeting.target)
        {
            // lead the target
            guidance.LOS = targeting.target.transform.position - transform.position;
            guidance.setPointSpeed = dyn.speedLimit;
        }
        else
        {
            // TODO: use default point
            guidance.LOS = transform.up;
            guidance.setPointSpeed = 0f;
        }

        guidance.angleLOS = Vector3.Angle(guidance.LOS, transform.up) * Mathf.Sign(-guidance.LOS.x * transform.up.y + guidance.LOS.y * transform.up.x);
        
    }

    void UpdateAutopilot()
    {
        // low-level, actuates the Agent

        // TODO: Use state-space control

        // use Proportional controller to find force
        dyn.accLinear = transform.up * (guidance.setPointSpeed - Vector2.Dot(dyn.rb2D.velocity, transform.up)) * autopilot.gainThrottle;

        // use PID controller to try to dampen oscillations (D) and zero in on target (PI)
        // anti-windup and reset
        if (Mathf.Abs(dyn.accAngular) < dyn.accTurnLimit && Mathf.Abs(guidance.angleLOS) < 20f)
        {
            autopilot.angleErrorIntegral += guidance.angleLOS * Time.fixedDeltaTime;
        }
        else
        {
            autopilot.angleErrorIntegral = 0;
        }
        dyn.accAngular = (autopilot.gainTurnP * guidance.angleLOS) + (autopilot.gainTurnD * -dyn.rb2D.angularVelocity) + (autopilot.gainTurnI * autopilot.angleErrorIntegral);
    }

    void AIFireWeapons()
    {
        // Used by AI-controlled Agent to determine when to fire
        
        if (!targeting.target || Mathf.Abs(guidance.angleLOS) > 3f)
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up);
        Debug.DrawLine(transform.position, hit.point, Color.red);
        if (hit.collider == null || Vector2.Distance(transform.position, hit.point) > Vector2.Distance(transform.position, targeting.target.transform.position) - 1f)
        {
            agent.FireBullet();
        }
    }
}
