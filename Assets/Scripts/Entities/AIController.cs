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
        internal Entity entity; // other info
        internal Vector2 lineToTgt;

        public Mission mission;
        public GameObject target; // Agent will home in on this
        public float radius = 0f; // Determines acceptable distance to target
    }

    [System.Serializable]
    public class Guidance
    {
        // Finds point (or sequence of points) to go to based on relative target location
        internal List<GameObject> wpsToTarget = new List<GameObject>();
        internal bool HasLOSToTarget; // can the target be seen (ignoring other entities)?
        internal Vector2 LOS = Vector2.zero; // position vector to next wp, or to target
        internal float angleLOS, setPointSpeed;

        public float speedLimit = 10f;
    }

    [System.Serializable]
    public class Autopilot
    {
        // Acts as a go-between for the Guidance and Dynamics subsystems. Ignores target info.

        // Use PID control to help the enemy maneuver. May replace with state-space model.
        internal float angleErrorIntegral = 0f;

        // forces and autopilot params
        public float gainThrottle = 10f, gainTurnP = 2f, gainTurnI = 1f, gainTurnD = 0.5f;
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
        if (!agent.alive)
        {
            targeting.target = null; // reset on death
            return;
        }
        UpdateTargeting();
        UpdateGuidance();
        UpdateAutopilot(); // works even without target
        AIFireWeapons();
    }

    void UpdateTargeting()
    {
        // high-level, takes into account other Agent locations, health, objectives, etc
        // will take mission from a "Team Manager" class (e.g. Hunt, Defend, Find Health, etc, see above)
        // will also keep track of those Agents which are detectable once TODO stealth mechanics are implemented

        // selection
        // bug: agent waits until its 'dead' target respawns to select a new target
        if (!targeting.target || !targeting.entity.alive)
        {
            // choose at random. Could choose based on distance, ship type, etc
            Entity[] targets = GameObject.Find("Agents").GetComponentsInChildren<Entity>();
            List<GameObject> potentialTargets = new List<GameObject>();

            foreach (var t in targets)
            {
                // target must a) be alive, b) not be self, c) be on a different team
                if (t.alive && t.gameObject != gameObject && (t.team != agent.team || t.team == Team.NoTeam))
                {
                    potentialTargets.Add(t.gameObject);
                }
            }
            if (potentialTargets.Any())
            {
                targeting.target = potentialTargets[Random.Range(0, potentialTargets.Count)];
                targeting.rb2D = targeting.target.GetComponent<Rigidbody2D>();
                targeting.entity = targeting.target.GetComponent<Entity>();
            }
        }

        // draw line
        if (targeting.target)
        {
            Debug.DrawLine(transform.position, targeting.target.transform.position, Color.blue);
            targeting.lineToTgt = targeting.target.transform.position - transform.position;
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
        // Set Point Speed: If no target, set min. If no LOS to target, set max. If LOS to target and close to target, match speed.

        if (!targeting.target)
        {
            guidance.HasLOSToTarget = false;
            guidance.wpsToTarget.Clear();
            guidance.LOS = transform.up;
            guidance.angleLOS = 0f;
            guidance.setPointSpeed = Mathf.Min(0.1f, guidance.speedLimit); // idling
            return;
        }
        guidance.HasLOSToTarget = WaypointManager.HasLOS(gameObject, targeting.target);
        guidance.setPointSpeed = guidance.speedLimit;

        if (!guidance.HasLOSToTarget)
        {
            // check if the wpList is empty or if self or target do not have LOS to their respective nodes.
            // this may not necessarily work off of the closest wp with LOS to target but it is faster to
            // check than getting the closest node on every update.
            if (!guidance.wpsToTarget.Any() || !WaypointManager.HasLOS(guidance.wpsToTarget.Last(), targeting.target)
                || !WaypointManager.HasLOS(guidance.wpsToTarget.First(), gameObject))
            {
                int closestWpToTarget = WaypointManager.GetClosestWaypointWithLOS(targeting.target);
                int closestWPToSelf = WaypointManager.GetClosestWaypointWithLOS(gameObject);

                // if either of the above are -1, this will return an empty list
                guidance.wpsToTarget = WaypointManager.GetPath(closestWPToSelf, closestWpToTarget);
            }
            // prune; if agent can see the next wp, no need for the current wp
            else if (guidance.wpsToTarget.Count > 1 && WaypointManager.HasLOS(gameObject, guidance.wpsToTarget[1]))
            {
                guidance.wpsToTarget.RemoveAt(0);
            }
            if (guidance.wpsToTarget.Any())
            {
                guidance.LOS = guidance.wpsToTarget[0].transform.position - transform.position;

                Debug.DrawLine(transform.position, guidance.wpsToTarget[0].transform.position, Color.green);
                for (int i = 0; i < guidance.wpsToTarget.Count - 1; i++)
                {
                    Debug.DrawLine(guidance.wpsToTarget[i].transform.position, guidance.wpsToTarget[i + 1].transform.position, Color.green);
                }
            }
        }
        else
        {
            // aim directly at target
            guidance.LOS = targeting.lineToTgt;
            // match speed
            if (targeting.rb2D && guidance.LOS.magnitude < targeting.radius)
            {
                guidance.setPointSpeed = Mathf.Min(guidance.speedLimit, targeting.rb2D.velocity.magnitude);
            }
        }
        // cross product
        guidance.angleLOS = Vector2.Angle(guidance.LOS, transform.up) * Mathf.Sign(-guidance.LOS.x * transform.up.y + guidance.LOS.y * transform.up.x);
    }

    void UpdateAutopilot()
    {
        // low-level, actuates the Agent

        // process error
        Vector2 velE = (Vector2)Vector3.ClampMagnitude(guidance.LOS, guidance.setPointSpeed) - agent.rb2D.velocity;
        // edge cases
        float angleE = (guidance.HasLOSToTarget || velE.magnitude < 1f) ? guidance.angleLOS : Vector2.Angle(transform.up, velE) * Mathf.Sign(-velE.x * transform.up.y + velE.y * transform.up.x);

        // For throttle, use Proportional controller to find force
        // project velE onto transform.up to get the actual pv
        dyn.accLinear = Vector2.Dot(transform.up, velE) * autopilot.gainThrottle;
        
        // For turns, use PID controller to try to dampen oscillations (D) and zero in on target (PI)
        // anti-windup and reset
        if (Mathf.Abs(dyn.accAngular) < dyn.accTurnLimit && Mathf.Abs(angleE) < 45f)
        {
            autopilot.angleErrorIntegral += angleE * Time.fixedDeltaTime;
        }
        else
        {
            autopilot.angleErrorIntegral = 0;
        }

        dyn.accAngular = (autopilot.gainTurnP * angleE)
            + (autopilot.gainTurnD * -agent.rb2D.angularVelocity)
            + (autopilot.gainTurnI * autopilot.angleErrorIntegral);
    }

    void AIFireWeapons()
    {
        // Used by AI-controlled Agent to determine when to fire
        if (!targeting.target || !targeting.entity.alive) return;

        if (Mathf.Abs(guidance.angleLOS) < Mathf.Max(agent.weapons.bulletSpread, 5f) && WaypointManager.HasLOS(gameObject, targeting.target, false))
        {
            Debug.DrawLine(transform.position, transform.position + transform.up * guidance.LOS.magnitude, Color.red);
            agent.FireBullet();
        }
    }
}
