using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
    The GameManager class manages the core game functionalities like tile occupation,
    character placement, and tile highlighting. It acts as a central hub for game-related
    operations and offers utility methods for tile and character management.
*/

public class GameManager : MonoBehaviour
{
    // ----- Section: Variables and References -----
    public static GameManager Instance;
    public Tilemap moveRangeTilemap;
    public Dictionary<Vector3Int, CharacterStats> occupiedTiles = new Dictionary<Vector3Int, CharacterStats>();
    private AISituationGrabber situationGrabber; 
    public TileBase highlightoccupiedTile; 
    public List<CharacterStats> characters;


    // Sets up the game manager's core components and initial state.
    void Awake()
    {
        // Set the Instance field to this instance of GameManager
        Instance = this;
        characters = EveryonesStats.Instance.allCharacterStats;
    }

    void Update()
    {
        PopulateOccupiedTiles();
       // HighlightCharacterTiles();
    }


    // ----- Section: Tile Management -----

    // Checks if the specified tile is occupied by a character.
    public bool IsTileOccupied(Vector3Int tile)
    {
        return occupiedTiles.ContainsKey(tile);
    }


    // Populates the dictionary of tiles that are currently occupied by characters.
    public void PopulateOccupiedTiles()
    {
        // Clear the dictionary first
        occupiedTiles.Clear();

        foreach (CharacterStats character in characters)
        {
            // Get the tile position
            Vector3Int tilePosition = moveRangeTilemap.WorldToCell(character.characterGameObject.transform.position);
            occupiedTiles[tilePosition] = character;
            // Center the character on their current tile
            CenterCharacterOnTile(character);

        }
    }

    public void CenterCharacterOnTile(CharacterStats character)
    {
        Vector3Int tilePosition = moveRangeTilemap.WorldToCell(character.characterGameObject.transform.position);

        // Find the world position of the tile's center
        Vector3 centeredPosition = moveRangeTilemap.GetCellCenterWorld(tilePosition);

        // Offset the centered position if necessary
        centeredPosition.y += 0.2f;

        character.characterGameObject.transform.position = centeredPosition;
    }


    public void PrintOccupiedTiles()
    {
        Debug.Log("PrintOccupiedTiles method called.");

        if (occupiedTiles.Count == 0)
        {
            Debug.Log("No occupied tiles found.");
            return;
        }

        foreach (KeyValuePair<Vector3Int, CharacterStats> entry in occupiedTiles)
        {
            Debug.Log("Tile " + entry.Key + " is occupied by " + entry.Value.characterName);
        }
    }

    public void HighlightCharacterTiles()
    {
        // Print the total number of tiles to be highlighted
        Debug.Log("Highlighting " + occupiedTiles.Count + " character tiles.");

        foreach (var tile in occupiedTiles.Keys)
        {
            moveRangeTilemap.SetTile(tile, highlightoccupiedTile);
        }
    }
}
