using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Agent))]

public class AIController : MonoBehaviour
{
    // Get AI target and find dynamics/weapons commands. Component for agent.
    // Debugging Lines color convention: blue = target, green = guidance/autopilot, red = weapons
    private Agent agent;
    private Agent.Dynamics dyn;

    [System.Serializable]
    // Mission types:
    // Attack (pick an Agent and try to kill it, or if None are found, go for the game objective or powerups)
    // Defend (pick a score object and stay within a certain radius)
    // Ambush (pick a wp that has lots of hostile Agents either facing it or near it, and camp there)
    // Heal (if low health, find a health powerup. If not, pick a friendly Agent and stay near it)
    //
    // Baseline rules
    // 1. Kill nearby hostiles
    // 2. If no hostiles nearby and no powerups in possession, go for powerups
    // 3. Shoot at, or avoid, hazards
    public enum Mission { Attack, Defend, Ambush, Heal };

    [System.Serializable]
    public class Targeting
    {
        // Holds all params related to target search and follow
        // These are used by other subsystems to actually move the agent
        internal Rigidbody2D rb2D; // of target; used by guidance to get closing vel

        public GameObject target; // Agent will home in on this
        public Mission mission;

    }

    [System.Serializable]
    public class Guidance
    {
        // Finds point (or sequence of points) to go to based on relative target location
        internal List<GameObject> wpsToTarget = new List<GameObject>();
        // sibling index of wp
        internal int closestWpToTarget;
        internal int closestWPToSelf;

        internal Vector2 LOS = Vector3.zero;
        internal float angleLOS = 0f;
        internal float setPointSpeed = 0f;

        public float speedLimit = 10f;
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
        // will take mission from a "Team Manager" class (e.g. Hunt, Defend, Find Health, etc, see above)
        // will also keep track of those Agents which are detectable once TODO stealth mechanics are implemented

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
        // if no target, clear the wpList and set speed to min
        // if no LOS to target, get the closest wpLOS to target (or just check this once per second)
        //      if that node has changed:
        //          get the closest wpLOS to self
        //          get the wp path and save it
        //      if not, prune the wpList until only one node has LOS
        // if LOS to target, use the pseudo-Pro Nav code already written
        //
        // Set Point Speed:
        // if no LOS to target, set max
        // scale based on turn angle and distance to next wp (e.g. the Agent should slow down in advance of a sharp turn)
        // if LOS to target, then set speed based on whatever "radius" from the target that the Agent has chosen

        if (targeting.target)
        {
            if (!WaypointManager.HasLOS(gameObject, targeting.target))
            {
                // check if the wpList is empty or if self or target do not have LOS to their respective nodes.
                // this may not necessarily work off of the closest wp with LOS to target but it is faster to
                // check than getting the closest node on every update.
                if (!guidance.wpsToTarget.Any() || !WaypointManager.HasLOS(guidance.wpsToTarget.Last(), targeting.target)
                    || !WaypointManager.HasLOS(guidance.wpsToTarget.First(), gameObject))
                {
                    guidance.closestWpToTarget = WaypointManager.GetClosestWaypointWithLOS(targeting.target);
                    guidance.closestWPToSelf = WaypointManager.GetClosestWaypointWithLOS(gameObject);

                    guidance.wpsToTarget = WaypointManager.GetPath(guidance.closestWPToSelf, guidance.closestWpToTarget);
                }
                // prune; if agent can see the next wp, no need for the current wp
                else
                {
                    // could do binary search here
                    while (guidance.wpsToTarget.Count > 1 && WaypointManager.HasLOS(gameObject, guidance.wpsToTarget[1]))
                    {
                        guidance.wpsToTarget.RemoveAt(0);
                    }
                }
                GameObject wpNext = guidance.wpsToTarget[0];
                guidance.LOS = wpNext.transform.position - transform.position;

                Debug.DrawLine(transform.position, wpNext.transform.position, Color.green);
                for (int i = 0; i < guidance.wpsToTarget.Count - 1; i++)
                {
                    Debug.DrawLine(guidance.wpsToTarget[i].transform.position, guidance.wpsToTarget[i + 1].transform.position, Color.green);
                }

                // TODO: set speed based on distance and velocity to the target
                
                
            }
            else
            {
                // aim directly at target
                guidance.wpsToTarget.Clear();
                guidance.LOS = targeting.target.transform.position - transform.position;
            }
            // cross product
            guidance.angleLOS = Vector3.Angle(guidance.LOS, transform.up) * Mathf.Sign(-guidance.LOS.x * transform.up.y + guidance.LOS.y * transform.up.x);
            guidance.setPointSpeed = guidance.speedLimit;

        }
        else
        {
            guidance.wpsToTarget.Clear();
            guidance.LOS = transform.up;
            guidance.angleLOS = 0f;
            guidance.setPointSpeed = 0.1f; // idling
        }
        
    }

    void UpdateAutopilot()
    {
        // low-level, actuates the Agent

        // TODO: Use state-space control

        // is agent facing the waypoint?
        float setPointSpeed = Mathf.Cos(guidance.angleLOS * Mathf.Deg2Rad) > 0 ? guidance.setPointSpeed : 0;

        // use Proportional controller to find force
        dyn.accLinear = transform.up * (setPointSpeed - dyn.fwdSpeed) * autopilot.gainThrottle;
        
        // use PID controller to try to dampen oscillations (D) and zero in on target (PI)
        // anti-windup and reset
        if (Mathf.Abs(dyn.accAngular) < dyn.accTurnLimit && Mathf.Abs(guidance.angleLOS) < 45f)
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
        if (hit.collider == targeting.target.GetComponent<Collider2D>())
        {
            agent.FireBullet();
        }
    }
}
