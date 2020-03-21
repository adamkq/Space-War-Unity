﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]

public class PlayerController : MonoBehaviour
{
    // Get Player Input and find dynamics/weapons commands. Component for agent. Will override AIController if both are present.
    private Agent agent;
    private Agent.Dynamics dyn;
    
    void Start()
    {
        agent = GetComponent<Agent>();
        dyn = agent.dynamics;
    }

    void Update()
    {
        GetPlayerInput();
        PlayerFireWeapons();
    }

    void GetPlayerInput()
    {
        // Apply forces to Agent rb2D
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
    }

    void PlayerFireWeapons()
    {
        // sends command to fire a weapon. If the weapon has an ROF/inventory limit, it is applied within the Agent class.
        if (Input.GetKey(KeyCode.Space))
        {
            agent.FireBullet();
        }
    }
}
