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
        public int health = 8;
        [Tooltip("Scales self-damage due to relative velocity.")]
        public float impactDamageMult = 0.5f;
    }
    

    [System.Serializable]
    public class Dynamics
    {
        internal Rigidbody2D rb2D;

        public float maxLinearVel = 5f;
        public float maxAngularVel = 200f;
    }

    [System.Serializable]
    public class Fragmentation
    {
        // how should the fragments be generated
        // fragments are scaled based on inverse of number of frags
        internal bool makeFrags; // if true, the asteroid will self-destruct

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

    private void Awake()
    {
        NumberInstantiated += 1;
        dynamics.rb2D = GetComponent<Rigidbody2D>();
        InitializeVelocity();
    }

    protected override void Start()
    {
        transform.parent = GameObject.Find("Asteroids").transform;
        base.Start();
        if (NumberInstantiated > absoluteLimit)
        {
            Debug.LogWarningFormat("Max number of asteroids ({0}) instantiated. Destroying asteroid.", absoluteLimit);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // used to synchronize object creation/destruction with the physics engine
        if (frag.makeFrags)
        {
            frag.makeFrags = false;
            Fragment();
        }

    }

    private void OnValidate()
    {
        // upper bound on number of frags = pow(maxFrags, maxGenerations)
        // should be under absolute limit
        // QED: maxFrags = pow(limit, 1/maxGen)
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
        // TODO: Get damage function based on velocity
        if (other.tag == "Agent")
        {
            int dHealth = (int)Mathf.Min(-collision.relativeVelocity.magnitude * basic.impactDamageMult, 0);
            IncrementHealth(dHealth);
        }

        if (other.name.Contains("Black Hole"))
        {
            frag.makeFrags = true;
        }

        // keep large numbers of small asteroids from piling up and turning the physics engine to molasses
        if (other.name.Contains("asteroid"))
        {
            // the higher the coefficient is, the more asteroids get eliminated. in theory the coeff could be as low as 1.25 (for frags = 2)
            if (transform.localScale.x < 1.5 * frag.minScale)
            {
                frag.makeFrags = true;
            }
        }
    }

    private void OnDestroy()
    {
        NumberInstantiated -= 1;
    }

    public void InitializeVelocity()
    {
        // Random velocities
        dynamics.rb2D.velocity += Random.insideUnitCircle * dynamics.maxLinearVel;
        dynamics.rb2D.angularVelocity = Random.Range(-dynamics.maxAngularVel, dynamics.maxAngularVel);
    }

    void IncrementHealth(int dHealth)
    {
        basic.health += dHealth;
        if (basic.health < 1)
        {
            frag.makeFrags = true;
        }
    }

    public void Fragment()
    {
        // generate fragments
        // scale is based on appearance: asteroid mass / "volume" is conserved but apparent area scales with volume at power of 2/3.
        if (frag.maxGenerations > 0)
        {
            int frags = Random.Range(2, frag.maxFrags);
            float scaleFactor = Mathf.Max(-0.1f * frags + 1f, 0.2f); // approximates inverse radical fcn for frags > 2

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
                    asteroid.basic.health = (int)Mathf.Max(basic.health * scaleFactor, 1);

                    if (NumberInstantiated >= absoluteLimit)
                    {
                        break;
                    }
                }
            }
        }

        // destroy this asteroid
        Destroy(gameObject);
    }
}
