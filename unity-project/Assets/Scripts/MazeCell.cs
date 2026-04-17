using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    // Each cell tracks which of its 4 walls are active
    public bool wallNorth = true;
    public bool wallSouth = true;
    public bool wallEast  = true;
    public bool wallWest  = true;

    // Flag used by the maze generation algorithm
    public bool visited = false;

    // References to the actual wall GameObjects in the scene
    [HideInInspector] public GameObject wallNorthObj;
    [HideInInspector] public GameObject wallSouthObj;
    [HideInInspector] public GameObject wallEastObj;
    [HideInInspector] public GameObject wallWestObj;
    [HideInInspector] public GameObject floorObj;
}