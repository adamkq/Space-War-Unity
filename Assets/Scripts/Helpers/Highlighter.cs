using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]

public class Highlighter : MonoBehaviour
{
    // may be added to other gameObjects to draw shapes
    // pass one LR as arg per shape. Get location(s) from transform.
    // only one LR per object (why...) so this will be turned into a mgr class
    // OnValidate, redraw all the highlighters of the associated object (track w/ dictionary)
    private LineRenderer line;

    public bool drawLineAtRuntime;
    public static Color defaultColor = new Color(0.8f, 1f, 0f);
    public static float defaultLineWidth = 0.1f;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = drawLineAtRuntime;
    }

    private void OnValidate()
    {

    }
    // may swap out linerenderer for: gameobj, linewidth, and renderonruntime
    public void DrawPolygon(LineRenderer line, float radius, Color color, int points=3)
    {
        line.colorGradient.mode = GradientMode.Fixed;
        line.startColor = color;
        line.endColor = line.startColor;

        line.positionCount = Mathf.Max(2, points);
        float angleIncrement = 360f / line.positionCount; // degrees; higher = lower res circle

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(transform.position.x + radius * Mathf.Cos(angle), transform.position.y + radius * Mathf.Sin(angle), transform.position.z);
            line.SetPosition(i, pos);
        }
    }

    void DrawArrow(float length, float width, Color color)
    {
        // arrowhead at gameobj position
    }

    void DrawStar(float radius, Color color, int points=5)
    {
        // if pts < 4, draw a cross
    }

    void DrawCircle(float radius)
    {

    }
}
