using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(Rigidbody2D))]

public class Agent : MonoBehaviour
{
    // A general class for player and enemy characters
    // at the player's whim, an Agent should be able to switch between AI and manual control

    [System.Serializable]
    public class Basic
    {
        // basic info for a character
        public bool playerControlled;
        public int health;
        public int score;
    }

    [System.Serializable]
    public class Dynamics
    {
        // state
        internal Rigidbody2D rb2D;
        internal Vector2 accLinear;
        internal float accAngular;

        // limits
        public float speedLimit, accFwdLimit, accRevLimit, accTurnLimit;
    }

    [System.Serializable]
    public enum Modes { Aggressive, Defensive, Ambush, Nomadic };

    [System.Serializable]
    public class Targeting
    {
        // Holds all params related to target search and follow
        // These are used by other subsystems to actually move the agent
        

        internal Rigidbody2D rb2D; // used by guidance to get closing vel

        public GameObject target; // enemy will home in on this
        public Modes targetingMode;

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
        public float leadFactor;
    }

    [System.Serializable]
    public class Autopilot
    {
        // Acts as a go-between for the Guidance and Dynamics subsystems. Ignores target info.

        // Use PID control to help the enemy maneuver. May replace with state-space model.
        internal float angleErrorIntegral = 0f;

        // forces and autopilot params
        public float setPointSpeed, gainThrottle, gainTurnP, gainTurnI, gainTurnD;
    }

    [System.Serializable]
    public class Weapons
    {
        // contains and spawns weapon prefabs
        // TODO: Add projectile queue system
        internal float timeLastFired = float.NegativeInfinity;

        public bool pacifist;
        public GameObject bullet;
        public float bulletROF;
        public GameObject bomb;
        public uint numberOfBombs;
        public GameObject laser;

    }

    public Basic basic;
    public Dynamics dyn;
    public Targeting tgt;
    public Guidance guidance;
    public Autopilot autopilot;
    public Weapons weapons;

    void Start()
    {
        transform.parent = GameObject.Find("Agents").transform;
        dyn.rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (basic.playerControlled)
        {
            GetPlayerInput();
            // get a camera
            PlayerFireWeapons();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TogglePlayerControl();
        }
    }

    private void FixedUpdate()
    {
        if (!basic.playerControlled)
        {
            UpdateTargeting();
            UpdateGuidance();
            UpdateAutopilot();
            AIFireWeapons();
        }
        Actuate();
    }

    void TogglePlayerControl()
    {
        basic.playerControlled = !basic.playerControlled;
        // track/untrack camera
        if (!basic.playerControlled)
        {
            tgt.target = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.name == "bullet(Clone)" && other.GetComponent<Entity>().team != GetComponent<Entity>().team)
        {
            basic.health -= 1;
            if (basic.health <= 0)
            {
                Kill();
            }
        }
    }

    void Kill()
    {
        GetComponent<Entity>().Spawn();
        Destroy(gameObject);
    }

    void GetPlayerInput()
    {
        // updates the same Dynamics values as the Autopilot function below
        // Move the spaceship when an arrow key is pressed
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            dyn.accAngular = dyn.accTurnLimit;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            dyn.accAngular = -dyn.accTurnLimit;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            dyn.accLinear = transform.up * dyn.accFwdLimit;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            dyn.accLinear = transform.up * dyn.accRevLimit;
        }

        // hard brake
        if (Input.GetKey(KeyCode.B))
        {
            dyn.accLinear = dyn.rb2D.velocity * dyn.accRevLimit;
        }

    }

    void UpdateTargeting()
    {
        // high-level, takes into account other Agent locations, health, objectives, etc
        if (tgt.target)
        {
            // presently, only change targets if the previous one is destoryed
            return;
        }
        Entity[] agents = GameObject.Find("Agents").GetComponentsInChildren<Entity>();

        foreach (var agent in agents)
        {
            if (agent.team != GetComponent<Entity>().team)
            {
                tgt.target = agent.gameObject;
            }
        }
        if (tgt.target)
        {
            tgt.rb2D = tgt.target.GetComponent<Rigidbody2D>();
        }
    }

    void UpdateGuidance()
    {
        // mid-level, plots a set of waypoints to the objective target
        // https://www.youtube.com/watch?v=jvtFUfJ6CP8

        
        if (tgt.target && tgt.rb2D)
        {
            // lead the target
            guidance.LOS = tgt.target.transform.position - transform.position;
        }
        else
        {
            // TODO: use default point
            guidance.LOS = Vector3.zero - transform.position;
        }

        // get target info
        guidance.angleLOS = Vector3.Angle(guidance.LOS, transform.up) * Mathf.Sign(-guidance.LOS.x * transform.up.y + guidance.LOS.y * transform.up.x);
        Debug.DrawLine(transform.position, guidance.LOS);
    }

    void UpdateAutopilot()
    {
        // low-level, actuates the Agent
        
        // TODO: Use state-space control

        // use Proportional controller to find force
        dyn.accLinear = transform.up * (autopilot.setPointSpeed - dyn.rb2D.velocity.magnitude) * autopilot.gainThrottle;

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

    void Actuate()
    {
        // forward
        if (Vector2.Angle(transform.up, dyn.accLinear) == 0)
        {
            dyn.accLinear = Vector3.ClampMagnitude(dyn.accLinear, dyn.accFwdLimit);
        }
        // reverse
        else
        {
            dyn.accLinear = Vector3.ClampMagnitude(dyn.accLinear, -dyn.accRevLimit);
        }

        dyn.accAngular = Mathf.Clamp(dyn.accAngular, -dyn.accTurnLimit, dyn.accTurnLimit);

        // apply
        dyn.rb2D.AddForce(dyn.accLinear);
        dyn.rb2D.AddTorque(dyn.accAngular);

        // reset
        dyn.accAngular = 0f;
        dyn.accLinear = Vector3.zero;
    }

    void PlayerFireWeapons()
    {
        if (weapons.pacifist)
        {
            return;
        }
        // Get key input
        if (Input.GetKey(KeyCode.Space))
        {
            FireBullet();
        }
    }

    void AIFireWeapons()
    {
        if (weapons.pacifist || !tgt.target)
        {
            return;
        }
        // Used by AI-controlled Agent to determine when to fire
        if (Mathf.Abs(guidance.angleLOS) < 3f)
        {
            FireBullet();
        }
    }

    void FireBullet()
    {
        // TODO: take from Queue
        if (Time.time - weapons.timeLastFired < Mathf.Max(Time.fixedDeltaTime, 1/weapons.bulletROF))
        {
            return;
        }
        weapons.timeLastFired = Time.time;

        // offset
        GameObject bullet = Instantiate(weapons.bullet, transform.position + (transform.up * 1f), transform.rotation);

        // speed vector add: cross(A, B) = AB cos(angle(A,B))
        Vector2 va = dyn.rb2D.velocity;
        bullet.GetComponent<Projectile>().speed += Vector2.Dot(transform.up, va);
        bullet.GetComponent<Projectile>().speed = Mathf.Max(bullet.GetComponent<Projectile>().speed, 1f);

        float bulletSpread = 3f; // degrees, either side
        bullet.transform.Rotate(new Vector3(0f, 0f, Random.Range(-bulletSpread, bulletSpread)));
        bullet.transform.parent = GameObject.Find("Projectiles").transform;

        bullet.GetComponent<Entity>().team = GetComponent<Entity>().team;
    }
}
