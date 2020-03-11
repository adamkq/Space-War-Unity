using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Summoner : MonoBehaviour
{
    internal class Dynamics
    {
        internal Rigidbody2D rb2D;

        internal Vector2 accLinear = Vector2.zero;
    }

    internal Dynamics dyn;

    public GameObject target;

    void Awake()
    {
        dyn = new Dynamics
        {
            rb2D = GetComponent<Rigidbody2D>()
        };
    }

    void Update()
    {
        if (!target)
        {
            target = GameObject.Find("spaceship");
        }
        if (Input.GetKey(KeyCode.S))
        {
            Summon();
        }
    }

    void FixedUpdate()
    {
        Actuate();
    }

    private void Summon()
    {
        dyn.accLinear = (target.transform.position - transform.position);
        dyn.accLinear.Normalize();
        dyn.accLinear *= dyn.rb2D.mass * 5;
    }

    private void Actuate()
    {
        dyn.rb2D.AddForce(dyn.accLinear);
        dyn.accLinear = Vector2.zero;
    }
}
