
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]

public class Starfield : MonoBehaviour
{
	// http://guidohenkel.com/2018/05/endless_starfield_unity/
	public Camera mainCamera;
	
	public int MaxStars = 100;
	public float parallaxScale = 0.5f; // (world sim space) 1 = infinitely far, 0 = at same distance as player
	public float minStarSize = 0.1f;
	public float maxStarSize = 0.5f;
	public float FieldWidth = 70f;
	public float FieldHeight = 50f;
	public bool Colorize = false;

	float xOffset;
	float yOffset;

	ParticleSystem Particles;
	ParticleSystem.Particle[] Stars;
    
    void Awake()
	{
        // unity bug: when the block hole PS is active, this PS cannot correctly render stars; they 'flicker' between their intended shape and a 'square' shape

		Stars = new ParticleSystem.Particle[MaxStars];
		Particles = GetComponent<ParticleSystem>();

		// Offset the coordinates to distribute the spread around the object's center
		xOffset = FieldWidth * 0.5f; 
		yOffset = FieldHeight * 0.5f;

		for (int i = 0; i < MaxStars; i++)
		{
			float randSize = Random.Range(minStarSize, maxStarSize); // Randomize star size within parameters
			float scaledColor = Colorize ? 1f - 2 * (maxStarSize - randSize) : 1f; // Red tint

			Stars[i].position = GetRandomInRectangle(FieldWidth, FieldHeight) + transform.position;
			Stars[i].startSize = randSize;
			Stars[i].startColor = new Color(1f, scaledColor, scaledColor, 1f);
		}
		Particles.SetParticles(Stars, Stars.Length); // Write data to the particle system
	}

    void Update()
    {
		// follow camera
		Vector2 dLoc = mainCamera.transform.position - transform.position; // change in location since last update
		transform.Translate(dLoc);

		// track stars
		if (dLoc.sqrMagnitude < 0.00001)
		{
			return;
		}

		for (int i = 0; i < MaxStars; i++)
		{
			// in local sim space: dLoc should be negative, 0 parallax = far, 1 = close
			// in world sim space: dLoc should be positive, 0 parallax = close, 1 = far
			Stars[i].position += new Vector3(dLoc.x * parallaxScale, dLoc.y * parallaxScale, 0f);

            // wrapping currently only works for world sim space
            // wrap horiz
            if (Stars[i].position.x < mainCamera.transform.position.x - xOffset)
            {
                Stars[i].position += new Vector3(FieldWidth, 0f, 0f);
            }
			else if (Stars[i].position.x > mainCamera.transform.position.x + xOffset)
			{
				Stars[i].position += new Vector3(-FieldWidth, 0f, 0f);
			}
            // wrap vertical
			if (Stars[i].position.y < mainCamera.transform.position.y - yOffset)
			{
				Stars[i].position += new Vector3(0f, FieldHeight, 0f);
			}
			else if (Stars[i].position.y > mainCamera.transform.position.y + yOffset)
			{
				Stars[i].position += new Vector3(0f, -FieldHeight, 0f);
			}
		}
        Particles.SetParticles(Stars);
	}

    // GetRandomInRectangle
    //----------------------------------------------------------
    // Get a random value within a certain rectangle area
    //
    Vector3 GetRandomInRectangle(float width, float height)
	{
        return new Vector3(Random.Range(0, width) - xOffset, Random.Range(0, height) - yOffset, 0);
	}
}