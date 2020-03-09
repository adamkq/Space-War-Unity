using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // script for a single spawn point

    private bool occupied = false;

    // set to 0 to allow any team to access
    public byte team = 0;
    public float spawn_time = 2.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //occupied = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        occupied = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        occupied = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        occupied = false;
    }

    public bool IsOccupied()
    {
        return occupied;
    }
}
