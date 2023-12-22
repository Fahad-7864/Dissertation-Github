using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
    This script primarily controls the behavior of the in-game cursor, and it is attached to the cursor GameObject.
    It provides functionalities such as toggling cursor size, casting spells, and handling mouse click interactions.
*/

public class CursorController : MonoBehaviour
{
    // ----- Section: Cursor Variables -----
    public Tilemap cursorTilemap;   
    public Tile cursorTile;           
    private GameObject cursor;        
    private Vector3Int previousTilePos; 
    private Tile previousTileType;    
    private bool isLargeCursor = false; 
    public Tile confirmedTile;
    private List<Vector3Int> confirmedTilesPos = new List<Vector3Int>();


    void Start()
    {
        // Initialize the previous tile position to a value that won't be a real tile position
        previousTilePos = Vector3Int.one * int.MaxValue;

        // Create the cursor GameObject and set its properties
        cursor = new GameObject("Cursor");
        SpriteRenderer sr = cursor.AddComponent<SpriteRenderer>();
        sr.sprite = cursorTile.sprite;
        sr.sortingLayerName = "Highlight";

    }

    // Update() function runs once per frame
    void Update()
    {
        // Figure out where the mouse is in the game world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = -ray.origin.z / ray.direction.z;
        Vector3 mousePosition = ray.GetPoint(distance);

        // Convert the mouse position to tile coordinates
        Vector3Int tilePos = cursorTilemap.WorldToCell(mousePosition);
        // Update the cursor's world position
        cursor.transform.position = cursorTilemap.GetCellCenterWorld(tilePos);


        // If the 'S' key is pressed, toggle between large and small cursors
        if (Input.GetKeyDown(KeyCode.S))
        {
            isLargeCursor = !isLargeCursor;
        }

        // If the tile position has changed, update the cursor and the tilemap
        if (tilePos != previousTilePos || Input.GetKeyDown(KeyCode.S))
        {
            ClearCursorTiles();
            SetCursorTiles(tilePos);
        }

        // If 'C' key is pressed, cast the circle spell at cursor's position
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3Int cursorTilePos = cursorTilemap.WorldToCell(GetCursorWorldPosition());
        }

        // If the right mouse button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 clickScreenPosition = Input.mousePosition;
            Vector2 clickRay = Camera.main.ScreenToWorldPoint(clickScreenPosition);

            // Convert the mouse position to tile coordinates
            Vector3Int clickTilePos = cursorTilemap.WorldToCell(clickRay);

            // Set the clicked tile to the confirmed tile type
            cursorTilemap.SetTile(clickTilePos, confirmedTile);

            // Add the confirmed tile position to the list
            confirmedTilesPos.Add(clickTilePos);
        }

        // If the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Calculate the mouse position in world coordinates
            Vector3 clickScreenPosition = Input.mousePosition;
            Vector2 clickRay = Camera.main.ScreenToWorldPoint(clickScreenPosition);
 
            // Convert the mouse position to tile coordinates
            Vector3Int clickTilePos = cursorTilemap.WorldToCell(clickRay);

            // Update the cursor's world position
            cursor.transform.position = cursorTilemap.GetCellCenterWorld(clickTilePos);


            Vector3 cursorWorldPos = GetCursorWorldPosition();

        }
    }

    public void ClearConfirmedTiles()
    {
        foreach (Vector3Int tilePos in confirmedTilesPos)
        {
            cursorTilemap.SetTile(tilePos, null);
        }
        confirmedTilesPos.Clear();
    }

    // ClearCursorTiles function removes the cursor's current tiles from the tilemap
    void ClearCursorTiles()
    {
        // Removes the tile at the previous cursor position
        cursorTilemap.SetTile(previousTilePos, null);
        // If the cursor size is set to large, clear surrounding tiles as well
        if (isLargeCursor)
        {
            // Iterate through neighboring tiles and clear them
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    cursorTilemap.SetTile(previousTilePos + new Vector3Int(x, y, 0), null);
                }
            }
        }
    }

    // SetCursorTiles function sets the cursor's tiles on the tilemap
    void SetCursorTiles(Vector3Int tilePos)
    {
        // Set the tile at the current cursor position
        cursorTilemap.SetTile(tilePos, cursorTile);
        // Remember the current cursor position for later
        previousTilePos = tilePos;
        // If the cursor size is set to large, set surrounding tiles as well
        if (isLargeCursor)
        {
            // Iterate through neighboring tiles and set them
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    cursorTilemap.SetTile(tilePos + new Vector3Int(x, y, 0), cursorTile);
                }
            }
        }
    }

    // Getter method for the cursor GameObject
    public GameObject GetCursor()
    {
        return cursor;
    }

    // Getter method for the cursor's world position
    public Vector3 GetCursorWorldPosition()
    {

        return cursor.transform.position;
    }

    //public GameObject shadowBoltPrefab;

    //public void CastShadowBolt(GameObject target)
    //{
    //    Debug.Log("CastShadowBolt called");

    //    // Instantiate the Shadow Bolt at the character's position
    //    GameObject shadowBoltInstance = Instantiate(shadowBoltPrefab, character.transform.position, Quaternion.identity);

    //    // Set the sorting order for the Shadow Bolt sprite
    //    SpriteRenderer sr = shadowBoltInstance.GetComponent<SpriteRenderer>();
    //    sr.sortingOrder = 1;

    //    // Launch the Shadow Bolt towards the target
    //    StartCoroutine(MoveShadowBoltTowardsTarget(shadowBoltInstance, target));

    //    // Log a debug message
    //    Debug.Log("Shadow Bolt fired");
    //}


    //IEnumerator MoveShadowBoltTowardsTarget(GameObject shadowBolt, GameObject target)
    //{
    //    // Get the ShadowBoltProjectile component from the instantiated Shadow Bolt
    //    ShadowBoltProjectile shadowBoltProjectile = shadowBolt.GetComponent<ShadowBoltProjectile>();

    //    // Initialize the Shadow Bolt with the target
    //    shadowBoltProjectile.Initialize(target);

    //    // Wait for the Shadow Bolt to reach the target or the target to be destroyed
    //    while (shadowBoltProjectile.enabled && target != null)
    //    {
    //        yield return null;
    //    }

    //    // Destroy the Shadow Bolt after it reaches the target or the target is destroyed
    //    Destroy(shadowBolt);
    //}

}

