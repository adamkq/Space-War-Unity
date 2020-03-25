using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]

public class Laser : Projectile
{
    // Implement a laser: Draw a line from the emitter (agent) outwards
    private LineRenderer lRend;
    private EdgeCollider2D ec2D;
    private List<Vector2> beam = new List<Vector2>();
    private Transform hazards;
    private bool draw = true;

    public int maxSegments = 100;
    public float maxLength = 500f;

    protected override void Start()
    {
        base.Start();
        lRend = GetComponent<LineRenderer>();
        lRend.positionCount = maxSegments;

        ec2D = GetComponent<EdgeCollider2D>();
        ec2D.edgeRadius = Mathf.Max(0, lRend.startWidth / 2);

        hazards = GameObject.Find("Hazards").transform;

        GetBeam();
        UpdateLineRenderer();
    }

    protected override void Update()
    {
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        draw = false;
    }

    public void UpdateLineRenderer()
    {
        if (beam.Count() < 2 || !draw)
        {
            lRend.enabled = false;
            lRend.positionCount = 2; // minimum
            return;
        }

        lRend.enabled = true;
        lRend.positionCount = beam.Count();
        for (int pt = 0; pt < beam.Count(); pt++)
        {
            lRend.SetPosition(pt, beam[pt]);
        }
    }

    public Collider2D GetBeam()
    {
        // ray
        GameObject goHit;
        Vector2 hitPoint = transform.position;
        Vector2 direction = transform.up;
        float dist = float.PositiveInfinity;
        float offset = 0.0001f;

        RaycastHit2D hit = Physics2D.Raycast(hitPoint, direction, dist); //default

        // black hole edge case. Works for multiple overlapping BHs:
        // when beam collides w/ BH, add it to list. Update the minimum singRadius if necessary. Apply optics & check singularity collision
        // when beam leaves BH, remove it from list.
        List<BlackHole> bhs = new List<BlackHole>();
        List<BlackHole> toRemove = new List<BlackHole>();
        Vector2 pseudoForce = Vector2.zero, relPos = Vector2.zero;

        // check if ray starts in BH
        foreach (Transform child in hazards)
        {
            GameObject go = child.gameObject;
            BlackHole bh = go.GetComponent<BlackHole>();
            if (bh && bh.areaEffectRadius > Vector2.Distance(transform.position, child.position))
            {
                bhs.Add(bh);
                dist = Mathf.Min(dist, bh.singularityRadius);
            }
        }

        // if so, find initial direction of beam
        foreach (BlackHole bh in bhs)
        {
            relPos = (Vector2)bh.gameObject.transform.position - hitPoint;
            float distToHorizon = relPos.magnitude - bh.singularityRadius;
            pseudoForce += relPos.normalized / distToHorizon;
        }
        if (bhs.Any()) direction *= Mathf.Max(1, Mathf.Pow(pseudoForce.magnitude, 3));

        while (beam.Count < lRend.positionCount)
        {
            AddSegment(hitPoint);
            hit = Physics2D.Raycast(hitPoint + direction * offset, direction, dist);

            if (!hit.collider)
            {
                // not in a BH
                if (float.IsPositiveInfinity(dist))
                {
                    hitPoint += direction * maxLength;
                    break;
                }

                // prune BHs
                if (toRemove.Any())
                {
                    bhs.RemoveAll(x => toRemove.Contains(x));
                    toRemove.Clear();

                    // reset ray
                    if (bhs.Any())
                    {
                        dist = bhs.Min(x => x.singularityRadius);
                    }
                    else
                    {
                        dist = float.PositiveInfinity;
                        direction.Normalize();
                        continue;
                    }
                }

                pseudoForce = Vector2.zero;
                bool hitSing = false;

                foreach (BlackHole bh in bhs)
                {
                    relPos = (Vector2)bh.gameObject.transform.position - hitPoint;
                    
                    // in the black hole area
                    if (relPos.magnitude < bh.areaEffectRadius + offset)
                    {
                        float distToHorizon = relPos.magnitude - bh.singularityRadius;
                        if (distToHorizon < 0)
                        {
                            hitSing = true; // hit singularity
                            break;
                        }
                        // light should bend near a BH regardless of how 'massive' it is
                        // pseudoForce += relPos.normalized * -bh.forceMagnitude * Time.fixedDeltaTime / distToHorizon;
                        pseudoForce += relPos.normalized / distToHorizon;
                    }
                    // exit BH; remove from list
                    else
                    {
                        toRemove.Add(bh);
                    }
                }

                if (hitSing) break;

                direction += pseudoForce;
                hitPoint += direction.normalized * dist;

                if (float.IsInfinity(dist))
                {
                    Debug.LogFormat("NaN in Black Hole, {0}, {1}", direction, dist);
                    break;
                }
            }
            else
            {
                goHit = hit.collider.gameObject;
                hitPoint = hit.point;

                // lasers most likely to hit walls first
                if (goHit.CompareTag("Wall"))
                {
                    WallType wallType = goHit.GetComponent<LineWall>().wallType;

                    if (!exemptWalls.Contains(wallType)) break;

                    // reflect laser
                    if (wallType == WallType.Bouncy || wallType == WallType.HotWall)
                    {
                        if (wallType == WallType.HotWall) damage += 1; // the LineWall script does not detect laser collisions

                        Vector2 offCollider = hitPoint - 1f * direction;
                        Vector2 inNormal = offCollider - hit.collider.ClosestPoint(offCollider);

                        direction = Vector3.Reflect(direction, inNormal.normalized);
                        continue;
                    }
                }

                // base case; terminate laser
                if (!hit.collider.isTrigger) break;

                // black hole entry. Rotate beam towards the singularity
                if (goHit.CompareTag("AreaEffect") && goHit.transform.parent.gameObject.name.ToLower().Contains("black hole"))
                {
                    BlackHole bh = goHit.transform.parent.gameObject.GetComponent<BlackHole>();
                    bhs.Add(bh);

                    dist = Mathf.Min(dist, bh.singularityRadius); // facilitate curve; the smaller this is, the smoother the curve will be
                }
            }
        }
        // terminate
        AddSegment(hitPoint);
        return hit.collider;
    }

    // adds points (in world space) to the lRend
    public void AddSegment(Vector2 pt)
    {
        beam.Add(transform.InverseTransformPoint(pt));
    }
}
