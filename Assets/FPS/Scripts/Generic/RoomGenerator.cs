using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.AI;         
using Unity.AI.Navigation;  

public class RoomGenerator : MonoBehaviour
{
    [Header("Префабы комнат")]
    public Room[] AllRooms;
    public Room[] RequiredRooms;
    [Min(1)] public int TotalRooms = 10;

    [Header("NavMesh (опц.)")]
    public NavMeshSurface Surface;

    [Header("Overlap check (опц.)")]
    public LayerMask RoomLayer = ~0;
    public Vector3 BoundsPadding = new Vector3(0.2f, 0.2f, 0.2f);

    readonly List<Transform> _freeDoors = new();
    readonly List<Transform> _allPatrolNodes = new();
    readonly List<Bounds> _placedBounds = new();

    void Start() => GenerateLevel();

    void GenerateLevel()
    {
        
        if (RequiredRooms == null || RequiredRooms.Length == 0)
        {
            return;
        }
        
        Room startRoom = Instantiate(RequiredRooms[0], transform.position, Quaternion.identity);
        if (!ValidateRoom(startRoom, "стартовой")) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player && startRoom.PlayerSpawn)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.transform.position = startRoom.PlayerSpawn.position;
            player.transform.rotation = startRoom.PlayerSpawn.rotation;

            if (cc) cc.enabled = true;
        }

        CollectPatrolPoints(startRoom);
        _freeDoors.AddRange(startRoom.Doors);
        _placedBounds.Add(GetRoomWorldBounds(startRoom));

        int count = 1;

        while (count < TotalRooms && _freeDoors.Count > 0)
        {
            Transform attachDoor = _freeDoors[Random.Range(0, _freeDoors.Count)];
            if (!attachDoor) { _freeDoors.Remove(attachDoor); continue; }

            Room prefab = (count < RequiredRooms.Length)
                ? RequiredRooms[count]
                : (AllRooms != null && AllRooms.Length > 0
                    ? AllRooms[Random.Range(0, AllRooms.Length)]
                    : RequiredRooms[Random.Range(0, RequiredRooms.Length)]);

            if (!prefab) { Debug.LogWarning("[RoomGenerator] null префаб"); continue; }

            Room newRoom = Instantiate(prefab);
            if (!ValidateRoom(newRoom, prefab.name)) { Destroy(newRoom.gameObject); continue; }
            Transform newEntry = newRoom.Doors[Random.Range(0, newRoom.Doors.Count)];

            Quaternion rotDelta = Quaternion.FromToRotation(newEntry.forward, -attachDoor.forward);
            newRoom.transform.rotation = rotDelta * newRoom.transform.rotation;
            Vector3 offset = attachDoor.position - newEntry.position;
            newRoom.transform.position += offset;

            CollectPatrolPoints(newRoom);

            newRoom.Doors.Remove(newEntry);
            _freeDoors.AddRange(newRoom.Doors);

            count++;
        }

        if (Surface) Surface.BuildNavMesh();
        CreateAndAssignGlobalPatrolPath();
    }

    // ===== helpers =====
    bool ValidateRoom(Room room, string tag)
    {
        if (room.Doors == null || room.Doors.Count == 0)
        {
            return false;
        }
        return true;
    }

    void CollectPatrolPoints(Room room)
    {
        if (room.PatrolPoints == null) return;
        foreach (var p in room.PatrolPoints)
            if (p) _allPatrolNodes.Add(p);
    }

    Bounds GetRoomWorldBounds(Room room, Vector3 extraPadding = default)
    {
        bool has = false;
        Bounds b = default;

        var rends = room.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }

        var cols = room.GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            if (!has) { b = c.bounds; has = true; }
            else b.Encapsulate(c.bounds);
        }

        if (!has)
            b = new Bounds(room.transform.position, Vector3.one * 2f);

        b.Expand(extraPadding);
        return b;
    }

    void CreateAndAssignGlobalPatrolPath()
    {
        if (_allPatrolNodes.Count == 0)
        {
            return;
        }

        var go = new GameObject("GeneratedPatrolPath");
        var path = go.AddComponent<PatrolPath>();
        path.PathNodes = new List<Transform>(_allPatrolNodes);

        foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
        {
            enemy.PatrolPath = path;
            enemy.ResetPathDestination();
            enemy.SetPathDestinationToClosestNode();
        }
    }
}
