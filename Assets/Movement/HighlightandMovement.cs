using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
/*
    The HighlightandMovement class is primarily responsible for 
    handling a character's movement and the highlighting of the movement range on a tilemap.
*/

public class HighlightandMovement : MonoBehaviour
{

    // ----- Section: Setup Variables -----
    public Tilemap moveRangeTilemap;
    public Tile highlightTile;
    public Astar pathfinding;
    public GameObject character;
    public float pathSpeed = 2.0f;
    public int moveRange = 3;

    // ----- Section: Tracking Variables -----
    public List<Vector3Int> highlightedTiles = new List<Vector3Int>();
    private bool isShowingMoveRange = false;
    private Vector3Int previousTilePos;
    private Animator characterAnimator;
    public EveryonesStats everyonesStats;
    private CharacterStats characterStats;
    public Tile confirmedTile;
    private Vector3Int? currentlySelectedTile = null;


    public bool showMoveRange = false;


    [SerializeField]
    private Vector3Int selectedTile;

    // ----- Section: Initialization Methods -----
    void Start()
    {
        characterAnimator = character.GetComponent<Animator>();
        characterStats = character.GetComponent<CharacterStats>();
        previousTilePos = Vector3Int.one * int.MaxValue;
    }




    // ----- Section: Movement Methods -----
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = -ray.origin.z / ray.direction.z;
        Vector3 mousePosition = ray.GetPoint(distance);
        selectedTile = moveRangeTilemap.WorldToCell(mousePosition);  
        
        if (Input.GetMouseButtonDown(0) && highlightedTiles.Contains(selectedTile))
        {
            HandleTileSelection();
        }

        if (Input.GetMouseButtonDown(1) && isShowingMoveRange) 
        {
            ClearHighlightedTiles();
            // Since tiles are cleared, movement range is not showing
            isShowingMoveRange = false; 
        }


        if (Input.GetKeyDown(KeyCode.D))
        {
            if (isShowingMoveRange)
            {
                ClearHighlightedTiles();
            }
            else
            {
                ShowMoveRange();
              //  Center the character on their current tile
               Vector3Int currentTilePos = moveRangeTilemap.WorldToCell(character.transform.position);
                Vector3 centeredPosition = moveRangeTilemap.GetCellCenterWorld(currentTilePos);

                // Offset the centered position
                centeredPosition.y += 0.2f;

                character.transform.position = centeredPosition;
            }

            isShowingMoveRange = !isShowingMoveRange;
        }




    }
    

    // this method is called when the player clicks on a tile
    private void HandleTileSelection()
    {
        // If the clicked tile is already the selected tile, move the character
        if (currentlySelectedTile.HasValue && currentlySelectedTile.Value == selectedTile)
        {
            Vector3Int startTilePos = moveRangeTilemap.WorldToCell(character.transform.position);
            List<Vector3Int> path = pathfinding.FindPath(startTilePos, selectedTile);

            if (path != null && path.Count <= moveRange + 1)
            {
                StartCoroutine(MoveCharacterAlongPath(character, path, moveRangeTilemap, pathSpeed));
                ClearHighlightedTiles();
                isShowingMoveRange = false;

                // Reset the currently selected tile
                currentlySelectedTile = null;
            }
        }
        else
        {
            // If the clicked tile is not the selected tile, change its color and set it as the selected tile

            // First, if there is already a selected tile, change its color back to the highlight tile
            if (currentlySelectedTile.HasValue)
            {
                moveRangeTilemap.SetTile(currentlySelectedTile.Value, highlightTile);
            }

            // Then, change the color of the clicked tile to the confirmed tile and set it as the selected tile
            moveRangeTilemap.SetTile(selectedTile, confirmedTile);
            currentlySelectedTile = selectedTile;
        }
    }

    // This coroutine moves a character along the path determined by the Astar class.

    IEnumerator MoveCharacterAlongPath(GameObject character, List<Vector3Int> path, Tilemap tilemap, float speed)
    {
        // Iterate through each position in the path
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 startPosition = character.transform.position;
            Vector3 targetPosition = tilemap.GetCellCenterWorld(path[i]);
            targetPosition.z = character.transform.position.z;

            float journeyTime = Vector3.Distance(startPosition, targetPosition) / speed;
            float elapsedTime = 0;

            while (elapsedTime < journeyTime)
            {
                float fractionOfJourney = (elapsedTime / journeyTime);
                character.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Snap character to target position, ensuring the character ends up at the exact target position.
            character.transform.position = targetPosition;
        }
        characterStats.actionPoints--;

    }

    private List<Vector3Int> highlightedMoveTiles = new List<Vector3Int>();




    // ----- Section: Tile Highlighting Functions -----


    // This method will clear the move range from the tilemap
    public void ClearHighlightedTiles()
    {
        foreach (Vector3Int tilePos in highlightedTiles)
        {
            moveRangeTilemap.SetTile(tilePos, null);
        }

        highlightedTiles.Clear(); 
    }




    // This method will show the move range for the current character
    public void ShowMoveRange()
    {
        Debug.Log("ShowMoveRange called");
        ClearHighlightedTiles(); 

        // Center the character on their current tile
        Vector3Int currentTilePos = moveRangeTilemap.WorldToCell(character.transform.position);
        character.transform.position = moveRangeTilemap.GetCellCenterWorld(currentTilePos);

        HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(currentTilePos, characterStats.moveRange);

        foreach (Vector3Int tilePos in reachableTiles)
        {
            // Exclude the current tile position and occupied tiles
            if (tilePos != currentTilePos && !GameManager.Instance.IsTileOccupied(tilePos)) 
            {
                // Add the tile to the list of highlighted tiles
                moveRangeTilemap.SetTile(tilePos, highlightTile); 
                highlightedTiles.Add(tilePos); 
            }
        }
    }

    // This method will show the move range for the current character, it is for the move range button.
    public void ShowMoveRangeButton()
    {
        if (isShowingMoveRange)
        {
            ClearHighlightedTiles();
        }
        else
        {
            ShowMoveRange();
            // Center the character on their current tile
            Vector3Int currentTilePos = moveRangeTilemap.WorldToCell(character.transform.position);
            Vector3 centeredPosition = moveRangeTilemap.GetCellCenterWorld(currentTilePos);

            // Offset the centered position
            centeredPosition.y += 0.2f;

            character.transform.position = centeredPosition;
        }

        isShowingMoveRange = !isShowingMoveRange;
    }

}
