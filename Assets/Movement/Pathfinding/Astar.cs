using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
    The Astar class provides an implementation of the A* pathfinding algorithm,
    enabling characters or entities in the game to navigate through a grid or 
    tile-based map. By analyzing the walkability of tiles and using a heuristic 
    approach, this class efficiently finds the shortest path between two points.
    Additionally, utilities for checking line of sight, reachable tiles, and more 
    are included to enhance movement and targeting capabilities. This functionality
    is also used by several other classes in the project, such as the Item range,
    the Archer's circle spell, and several other components.
*/

public class Astar : MonoBehaviour
{

    // ----- Section: Singleton Implementation -----
    public static Astar Instance { get; private set; }

    // ----- Section: Tilemap & Walkability -----
    public Tilemap tilemap;
    public LayerMask unwalkableLayerMask;

    // Singleton's Awake method
    private void Awake()
    {
        // Singleton instance management
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, we destroy the current one:
            Destroy(gameObject);
        }
    }

    // ----- Section: Pathfinding Methods -----
    // Finds a path or returns null if one isn't found.
    public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        // Initializes the containers we'll use during the search.
        // openSet contains nodes that are yet to be evaluated
        // closedSet contains nodes that have been evaluated
        List<Vector3Int> openSet = new List<Vector3Int>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        // We'll store the path here, node by node.
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        //This keeps a record of how far(cost) each step in the path is.
        Dictionary<Vector3Int, int> gCost = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, int> hCost = new Dictionary<Vector3Int, int>();

        // Start position is added to openSet, gCost and hCost are initialized
        openSet.Add(startPos);
        gCost[startPos] = 0;
        hCost[startPos] = GetDistance(startPos, targetPos);

        // Main loop
        while (openSet.Count > 0)
        {
            Vector3Int current = openSet[0];

            // We pick the next step that's closest to the goal
            for (int i = 1; i < openSet.Count; i++)
            {
                if (gCost[openSet[i]] + hCost[openSet[i]] < gCost[current] + hCost[current] ||
                    gCost[openSet[i]] + hCost[openSet[i]] == gCost[current] + hCost[current] &&
                    hCost[openSet[i]] < hCost[current])
                {
                    current = openSet[i];
                }
            }

            // Current position is removed from openSet and added to closedSet
            openSet.Remove(current);
            closedSet.Add(current);

            // If target position was reached, retrace path and return it
            if (current == targetPos)
            {
                return RetracePath(cameFrom, startPos, targetPos);
            }

            // We look at all the possible next steps from here.
            foreach (Vector3Int neighbour in GetNeighbors(current))
            {
                if (!IsWalkable(neighbour) || closedSet.Contains(neighbour))
                {
                    continue;
                }

                // We check if this step brings us closer to the goal.
                int tentativeGCost = gCost[current] + GetDistance(current, neighbour);

                if (!openSet.Contains(neighbour) || tentativeGCost < gCost[neighbour])
                {
                    // If it's a shorter path, update path and cost information, and add neighbor to openSet
                    cameFrom[neighbour] = current;
                    gCost[neighbour] = tentativeGCost;
                    hCost[neighbour] = GetDistance(neighbour, targetPos);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        // If no path was found, return null
        return null;
    }



    // Retraces a path by following cameFrom links
    private List<Vector3Int> RetracePath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int startPos, Vector3Int targetPos)
    {
        // Initialize path
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = targetPos;

        //If we haven't reached the start position
        while (current != startPos)
        {
            // Add the current position to the path
            path.Add(current);
            // Move to the next position along the path
            current = cameFrom[current];
        }

        // Reverse the path to have it go from start to target, and return it
        path.Reverse();
        return path;
    }

    // Calculate and return the distance between two positions (posA and posB)
    private int GetDistance(Vector3Int posA, Vector3Int posB)
    {
        // Calculating the differences in x and y coordinates
        int dX = Mathf.Abs(posA.x - posB.x);
        int dY = Mathf.Abs(posA.y - posB.y);

        // Return the sum of the absolute differences
        return dX + dY;
    }

    // Generate and return an array of neighboring positions around a given tile position (tilePos)
    public Vector3Int[] GetNeighbors(Vector3Int tilePos)
    {
        // Creating a list of neighboring positions
        List<Vector3Int> neighbours = new List<Vector3Int>();

        neighbours.Add(tilePos + new Vector3Int(1, 0, 0));
        neighbours.Add(tilePos + new Vector3Int(-1, 0, 0));
        neighbours.Add(tilePos + new Vector3Int(0, 1, 0));
        neighbours.Add(tilePos + new Vector3Int(0, -1, 0));

        // Returning the array of neighbors
        return neighbours.ToArray();
    }

    // Check if a given position is walkable or not
    public bool IsWalkable(Vector3Int position)
    {
        // A position is walkable if it doesn't have a tile or if its collider type is not Grid
        return !tilemap.HasTile(position) || (tilemap.GetColliderType(position) != Tile.ColliderType.Grid && !(Physics2D.OverlapBox(tilemap.CellToWorld(position), new Vector2(1, 1), 0, unwalkableLayerMask)));
    }


    // ----- Section: Tile Accessibility Methods -----

    // Get all tiles that can be reached from a given start position (startTilePos) within a given range
    public HashSet<Vector3Int> GetReachableTiles(Vector3Int startTilePos, int range)
    {
        // Initializing set of reachable tiles and queue 
        HashSet<Vector3Int> reachableTiles = new HashSet<Vector3Int>();
        Queue<(Vector3Int, int)> queue = new Queue<(Vector3Int, int)>();

        // Adding the start position to the queue
        queue.Enqueue((startTilePos, 0));

        // While there are positions to explore
        while (queue.Count > 0)
        {
            // Dequeue a position and its distance from start
            (Vector3Int currentTilePos, int currentDistance) = queue.Dequeue();

            // If the current distance is less than the range
            if (currentDistance < range)
            {
                // Get all neighbors of the current position
                Vector3Int[] neighbors = GetNeighbors(currentTilePos);

                // For each neighbor
                foreach (Vector3Int neighbor in neighbors)
                {
                    // If it's a walkable tile that we haven't added to reachableTiles yet
                    if (!reachableTiles.Contains(neighbor) && IsWalkable(neighbor))
                    {
                        reachableTiles.Add(neighbor);
                        // Enqueue the tile with currentDistance + 1
                        queue.Enqueue((neighbor, currentDistance + 1));
                    }
                }
            }
        }

        // After exploring all possible tiles within the range, return the set of reachable tiles
        return reachableTiles;
    }
  
    public bool HasLineOfSight(Vector3Int fromPos, Vector3Int toPos)
    {
        Vector3 fromWorldPos = tilemap.GetCellCenterWorld(fromPos);
        Vector3 toWorldPos = tilemap.GetCellCenterWorld(toPos);

        RaycastHit2D hit = Physics2D.Linecast(fromWorldPos, toWorldPos, unwalkableLayerMask);

        // If the raycast hits nothing, return true
        if (hit.collider == null)
        {
            return true;
        }

        // If the raycast hits a target that is at toPos, also return true
        if (hit.collider.gameObject.transform.position == toWorldPos)
        {
            return true;
        }

        // Otherwise, return false
        return false;
    }


}