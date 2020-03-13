using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repulsor : MonoBehaviour
{
    private PointEffector2D pe2D;
    void Awake()
    {
        pe2D = GetComponent<PointEffector2D>();
    }

    void Update()
    {
        // toggle force field
        if (Input.GetKeyDown(KeyCode.R))
        {
            pe2D.enabled = !pe2D.enabled;
        }
    }
}
