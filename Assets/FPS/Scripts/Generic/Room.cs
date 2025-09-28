using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Transform> Doors = new List<Transform>();

    public List<Transform> PatrolPoints = new List<Transform>();

    public Transform PlayerSpawn;
}
