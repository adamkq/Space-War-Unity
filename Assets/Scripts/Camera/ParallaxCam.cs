using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxCam : MonoBehaviour
{
    Vector3 initialCamPos;
    MeshRenderer meshRend;
    Vector2 speedScale;
    float orthoSize;

    public Camera mainCamera;
    public bool anchorToCamera;
    public float scrollSpeed;
    public float alphaValue = 1f;

    void Awake()
    {
        initialCamPos = mainCamera.transform.position;
        speedScale = new Vector2(scrollSpeed / transform.localScale.x, scrollSpeed / transform.localScale.y); // valid for 'anchor to camera' only

        orthoSize = mainCamera.orthographicSize;

        meshRend = GetComponent<MeshRenderer>();
        meshRend.sortingLayerName = "Background";

        if (alphaValue >= 0f) meshRend.material.color = new Color(1f, 1f, 1f, alphaValue);
        
    }

    void Update()
    {
        if (!mainCamera)
        {
            return;
        }
        Offset();

        Rescale();
        
    }

    void Offset()
    {
        // texture offsets should follow the camera to give the impression of faraway parallax
        // offsets are scaled to the size of the mesh (quad)

        // instead of scaling the mesh to the entire level, just fill the camera
        // at speed = 0, the image appears infinitely far
        // at speed = 1, the image appears to be at the same distance as the player being tracked
        Vector2 camLocation;
        if (anchorToCamera)
        {
            camLocation = mainCamera.transform.position;
            transform.Translate(camLocation - (Vector2)transform.position);
        }
        else
        {
            camLocation = new Vector2(initialCamPos.x - mainCamera.transform.position.x, initialCamPos.y - mainCamera.transform.position.y);
        }
        meshRend.material.mainTextureOffset = Vector3.Scale(speedScale, camLocation);
    }

    void Rescale()
    {
        // if the camera position changes, then the tiling and scale of the mesh should also change
        // rescaling the mesh during runtime causes the shader to derender (or something) so I will change the tiling

        // camera size is proportional to how much scenery it takes in
        // at speed = 0, the scale should change so that the image size remains the same
        // at speed = 1, the scale should not change at all (i.e. what happens by default) so that the image gets smaller as the cam moves out & vice versa

        
        if (mainCamera.orthographicSize == orthoSize)
        {
            return;
        }

        // at speed = 1, new tiling = old tiling
        meshRend.material.mainTextureScale *= 1/(1 + (1 - scrollSpeed) * (mainCamera.orthographicSize / orthoSize - 1));
        orthoSize = mainCamera.orthographicSize;
    }
}
