using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CentreCharacter : MonoBehaviour
{
    public Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
        }
    }
    public void CenterCharacterOnTile(Transform characterTransform)
    {
        // Convert the character's world position to cell position
        Vector3Int currentCell = tilemap.WorldToCell(characterTransform.position);

        // Get the center position of the tile in world coordinates
        Vector3 tileCenterWorldPos = tilemap.GetCellCenterWorld(currentCell);

        // Create a Vector3 for the offset
        Vector3 offset = new Vector3(0, 0.2f, 0);

        // Add the offset to the position
        characterTransform.position = tileCenterWorldPos + offset;

        Debug.Log("Character Position After Centering and Adding Offset: " + characterTransform.position);
    }

}
