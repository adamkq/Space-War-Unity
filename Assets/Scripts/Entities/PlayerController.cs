using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]

public class PlayerController : MonoBehaviour
{
    // Get Player Input and find dynamics/weapons commands. Component for agent.
    private Agent agent;
    private Agent.Dynamics dyn;
    private Rigidbody2D rb2D;
    
    void Start()
    {
        agent = GetComponent<Agent>();
        dyn = agent.dynamics;
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GetPlayerInput();
        PlayerFireWeapons();
    }

    // Apply forces to Agent rb2D
    void GetPlayerInput()
    {
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
            dyn.accLinear = dyn.accFwdLimit;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            dyn.accLinear = dyn.accRevLimit;
        }

        // e-brake: rotates agent in direction opposite to velocity
        if (Input.GetKey(KeyCode.J))
        {
            float angle = Vector2.Angle(-rb2D.velocity, transform.up) * Mathf.Sign(Vector3.Cross(-rb2D.velocity, transform.up).z);
            dyn.accAngular = -angle * 0.1f - rb2D.angularVelocity * 0.01f;
        }
    }

    // sends command to fire a weapon. If the weapon has an ROF/inventory limit, it is applied within the Agent class.
    void PlayerFireWeapons()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            agent.FireBullet();
        }
    }
}
