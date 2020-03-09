using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxCam : MonoBehaviour
{
    Vector3 initialCamPos;
    MeshRenderer meshRend;
    MeshFilter meshFilter;
    float speedScaleX, speedScaleY;

    public Camera mainCamera;
    public bool anchorToCamera;
    public float scrollSpeed;
    public float alphaValue = 1f;

    void Awake()
    {
        initialCamPos = mainCamera.transform.position;
        speedScaleX = scrollSpeed / transform.localScale.x;
        speedScaleY = scrollSpeed / transform.localScale.y;

        meshRend = GetComponent<MeshRenderer>();
        if (alphaValue >= 0f)
        {
            meshRend.material.color = new Color(1f, 1f, 1f, alphaValue);
        }
        
        meshFilter = GetComponent<MeshFilter>();
        // https://answers.unity.com/questions/523289/change-size-of-mesh-at-runtime.html
        foreach (var v in meshFilter.mesh.vertices)
        {
            continue;
        }
        
    }

    void Update()
    {
        if (!mainCamera)
        {
            return;
        }
        float offsetX, offsetY;
        // texture offsets should follow the camera to give the impression of faraway parallax
        // offsets are scaled to the size of the mesh (quad)

        // instead of scaling the mesh to the entire level, just fill the camera
        // at speed = 0, the image appears infinitely far
        // at speed = 1, the image appears to be at the same distance as the player being tracked
        if (anchorToCamera)
        {
            offsetX = speedScaleX * (mainCamera.transform.position.x);
            offsetY = speedScaleY * (mainCamera.transform.position.y);
            transform.Translate(new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y) - (Vector2)transform.position);
        }
        else
        {
            offsetX = speedScaleX * (initialCamPos.x - mainCamera.transform.position.x);
            offsetY = speedScaleY * (initialCamPos.y - mainCamera.transform.position.y);
        }
        meshRend.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}
