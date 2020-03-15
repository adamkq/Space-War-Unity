using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Entity
{
    // Class for asteroid hazards.
    // Note: Asteroids can recursively self-spawn, so a hard limit has been put
    // in place to keep too many from being instantiated during runtime.

    public static int NumberInstantiated { get; private set; }
    private const int absoluteLimit = 512; // convenient bound for many common pow(x, y) values

    [System.Serializable]
    public class Basic
    {
        [Tooltip("Scales self-damage due to relative velocity.")]
        public float impactDamageMult = 0.5f;
        [Tooltip("Relative velocity for any damage.")]
        public float impactVelThreshold = 0.5f;
    }
    

    [System.Serializable]
    public class Dynamics
    {
        public float maxLinearVel = 5f;
        public float maxAngularVel = 200f;
    }

    [System.Serializable]
    public class Fragmentation
    {
        // how should the fragments be generated
        // fragments are scaled based on inverse of number of frags

        [Tooltip("Max number of times a parent asteroid can fragment.")]
        public int maxGenerations = 3;
        [Tooltip("Max number of frags generated. Limited based on max generations.")]
        public int maxFrags = 4;
        [Tooltip("Min scale below which no asteroid fragments will be made.")]
        public float minScale = 0.2f;
    }

    public Basic basic;
    public Dynamics dynamics;
    public Fragmentation frag;

    protected override void Awake()
    {
        base.Awake();
        NumberInstantiated += 1;
        InitializeVelocity();
    }

    protected override void Start()
    {
        base.Start();
        transform.parent = GameObject.Find("Asteroids").transform;
        if (NumberInstantiated > absoluteLimit)
        {
            Debug.LogWarningFormat("Max number of asteroids ({0}) instantiated. Destroying asteroid.", absoluteLimit);
            Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        // synchronize object creation/destruction with the physics engine
        if (health < 1) Fragment();
    }

    protected override void OnValidate()
    {
        // upper bound on number of frags = pow(maxFrags, maxGenerations)
        // should be under absolute limit
        // QED: maxFrags = pow(limit, 1/maxGen)
        base.OnValidate();
        frag.maxGenerations = Mathf.Clamp(frag.maxGenerations, 0, (int)Mathf.Log(absoluteLimit, 2));
        frag.maxFrags = Mathf.Clamp(frag.maxFrags, 2, (int)Mathf.Pow(absoluteLimit, 1f/Mathf.Max(frag.maxGenerations, 1)));
        frag.minScale = Mathf.Clamp01(frag.minScale);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Projectile")
        {
            IncrementHealth(-other.GetComponent<Projectile>().damage);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Agent") ApplyImpactDamage(collision.relativeVelocity.magnitude);

        if (!(other.tag == "Hazard")) return;

        // inter-asteroid collisions
        if (other.name.Contains("asteroid"))
        {
            // keep large numbers of small asteroids from piling up and turning the physics engine to molasses
            // coeff corresponds to minimum scalingFactor of about 3 frags
            if (0.6f * transform.localScale.x < frag.minScale) IncrementHealth(-health);
        }
        else // black holes and neutron stars
        {
            IncrementHealth(-health);
        }
    }

    private void OnDestroy()
    {
        NumberInstantiated -= 1;
    }

    public void InitializeVelocity()
    {
        // Random velocities
        rb2D.velocity += Random.insideUnitCircle * dynamics.maxLinearVel;
        rb2D.angularVelocity = Random.Range(-dynamics.maxAngularVel, dynamics.maxAngularVel);
    }

    void ApplyImpactDamage(float relVelocity)
    {
        
        // arg should be greater than 0 in case of a collision, so negate it to get the damage
        // TODO: Generalize this for all entities (since they all have rigidbodies) and make it a function of mass, velocity, and coefficients
        int dHealth = (int)Mathf.Min(-(relVelocity - basic.impactVelThreshold) * basic.impactDamageMult, 0);
        IncrementHealth(dHealth);
    }

    public void Fragment()
    {
        // generate fragments
        // scale is based on appearance: asteroid mass / "volume" is conserved but apparent area scales with volume at power of 2/3.
        // this means that scaleFactor relates to number of frags with: (1/frags)**(1/3)
        if (frag.maxGenerations > 0)
        {
            int frags = Random.Range(2, frag.maxFrags);
            float scaleFactor = Mathf.Max(-0.08f * frags + 0.8f, 0.3f); // approximates inverse radical fcn for frags > 2

            // do not spawn frags below a certain size
            if (transform.localScale.x * scaleFactor > frag.minScale)
            {
                for (int i = 0; i < frags; i++)
                {
                    GameObject fragment = Instantiate(gameObject, transform.position, Quaternion.identity);
                    // update
                    fragment.name = name;
                    fragment.transform.localScale = transform.localScale * scaleFactor;

                    Rigidbody2D frag_rb2D = fragment.GetComponent<Rigidbody2D>();
                    Asteroid asteroid = fragment.GetComponent<Asteroid>();

                    // approximate
                    frag_rb2D.mass *= scaleFactor;

                    // recursive
                    asteroid.frag.maxGenerations -= 1;
                    asteroid.startHealth = (int)Mathf.Max(startHealth * scaleFactor, 1);

                    if (NumberInstantiated >= absoluteLimit) break;
                }
            }
        }

        // destroy this asteroid
        Destroy(gameObject);
    }
}
