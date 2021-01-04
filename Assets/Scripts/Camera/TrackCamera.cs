using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class TrackCamera : MonoBehaviour
{
    public GameObject target; // the player object

    private Camera mainCamera;
    private Rigidbody2D rb2D;
    private float depth;

    private void Awake()
    {
        // keep the camera Z value; update x and y
        mainCamera = GetComponent<Camera>();
        depth = transform.position.z;
    }

    void Start()
    {
        rb2D = target.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // allow user to set camera zoom
        if (Input.GetKeyDown(KeyCode.Equals)) ChangeOrthographicSize(3f);
        
        if (Input.GetKeyDown(KeyCode.Minus)) ChangeOrthographicSize(-3f);

        if (Input.GetMouseButtonDown(0))
        {
            //ChangeTarget(target);
        }

        if (target) FollowTarget();
    }

    void FollowTarget()
    {
        // lead the player a bit
        transform.position = rb2D ? Vector2.Lerp(transform.position, rb2D.position + (rb2D.velocity * 0.3f), 0.1f) : Vector2.Lerp(transform.position, target.transform.position, 0.1f);
        // keep the Z value
        transform.position += Vector3.forward * depth;
    }

    void ChangeOrthographicSize(float inc)
    {
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + inc, 5.0f, 20.0f);
    }

    public void ChangeTarget(GameObject newTarget)
    {
        // switch camera to the agent that was clicked on
        target = newTarget;
        rb2D = target.GetComponent<Rigidbody2D>();
    }
}
