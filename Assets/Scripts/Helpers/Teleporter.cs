using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private Camera cam;
    private Vector2 mousePos = Vector2.zero;
    private Rigidbody2D rb2D;
    
    private void Start()
    {
        cam = Camera.main;
        rb2D = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            bool wasKinematic = rb2D.isKinematic;
            rb2D.isKinematic = true;
            rb2D.position = mousePos;
            rb2D.isKinematic = wasKinematic;
        }
    }
}
