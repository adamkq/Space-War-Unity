using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main_camera_script : MonoBehaviour
{
    public GameObject target; // the player sprite

    private Camera mainCamera;
    private Vector2 mousePos;
    private Rigidbody2D rb2D;
    private float depth;

    // Start is called before the first frame update
    void Start()
    {
        // keep the camera Z value; update x and y
        mainCamera = GetComponent<Camera>();
        depth = transform.position.z;

        rb2D = target.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        // allow user to set camera zoom
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            ChangeOrthographicSize(3f);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            ChangeOrthographicSize(-3f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            ChangeTarget();
        }

        if (target)
        {
            FollowTarget();
            Debug.DrawLine(Vector3.zero, mousePos);
        }
    }

    void FollowTarget()
    {
        // lead the player a bit
        transform.position = Vector2.Lerp(transform.position, rb2D.position + rb2D.velocity * 0.3f, 0.1f);
        // keep the Z value
        transform.position += Vector3.forward * depth;
    }

    void ChangeOrthographicSize(float inc)
    {
        mainCamera.orthographicSize += inc;
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, 5.0f, 20.0f);
    }

    void ChangeTarget()
    {
        // switch camera to the agent that was clicked on
    }
}
