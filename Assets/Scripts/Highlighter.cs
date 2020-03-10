using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    // may be added to other gameObjects to draw shapes
    // pass one LR as arg per shape. Get location(s) from transform.
    // only one LR per object (why...) so this will be turned into a mgr class
    // OnValidate, redraw all the highlighters of the associated object (track w/ dictionary)

    public static Color defaultColor = new Color(0.8f, 1f, 0f);
    public static float defaultLineWidth = 0.1f;

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    // may swap out linerenderer for: gameobj, linewidth, and renderonruntime
    void DrawPolygon(LineRenderer lr, float radius, Color color, int points=3)
    {
        if (color == null)
        {
            color = defaultColor;
        }
    }

    void DrawArrow(LineRenderer lr, float length, float width, Color color)
    {
        // arrowhead at gameobj position
    }

    void DrawStar(LineRenderer lr, float radius, Color color, int points=5)
    {
        // if pts < 4, draw a cross
    }
}
