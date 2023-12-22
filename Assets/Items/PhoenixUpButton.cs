using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
/*
    The PhoenixUpButton class manages the behavior of the PhoenixUp item button in the game.
    It provides functions to determine when and how the PhoenixUp item can be used, displays
    a confirmation dialog to the user, and integrates a mini-game to modify the effect of the item.
*/
public class PhoenixUpButton : MonoBehaviour
{
    // ----- Section: Phoenix up Variables -----
    public Item phoenixUp;
    public EveryonesStats everyonesStats;
    public DisplayStats displayStats;
    public Tilemap phoenixUpUseTilemap;
    public Tile highlightTile;
    public Astar pathfinding;
    private CharacterStats characterStats;
    private List<Vector3Int> highlightedTiles = new List<Vector3Int>();
    public TurnManager turnManager;
    public int phoenixUpRange = 1;
    public GameObject wheelGamePrefab;
    private GameObject wheelGameInstance;
    public GameObject[] gameObjectsToDeactivate;
    public List<GameObject> objectsToKeepDeactivated;
    public GameObject confirmationDialog;
    public Canvas Canvas;
    private GameObject activeConfirmationDialog;
    private Button yesButton;
    private Button noButton;
    private CharacterStats _targetCharacterForConfirmation;



    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickScreenPosition = Input.mousePosition;
            Vector2 clickRay = Camera.main.ScreenToWorldPoint(clickScreenPosition);
            Vector3Int clickTilePos = phoenixUpUseTilemap.WorldToCell(clickRay);

            Debug.Log("Mouse Position: " + clickScreenPosition);
            Debug.Log("Click Ray Position: " + clickRay);
            Debug.Log("Click Tile Position: " + clickTilePos);

            OnTileClick(clickTilePos);
        }
    }

    // ----- Section: Button Actions -----
    // Handles the action when the PhoenixUp button is clicked.
    public void OnButtonClick()
    {
        characterStats = turnManager.GetActiveCharacterStats();
        if (characterStats == null)
        {
            Debug.Log("No character stats found.");
        }
        else
        {
            Debug.Log("Found character stats.");
            activeConfirmationDialog = Instantiate(confirmationDialog);
            activeConfirmationDialog.transform.SetParent(Canvas.transform, false);

            Text dialogText = activeConfirmationDialog.GetComponentInChildren<Text>();
            yesButton = activeConfirmationDialog.transform.Find("YesButton").GetComponent<Button>();
            noButton = activeConfirmationDialog.transform.Find("NoButton").GetComponent<Button>();

            yesButton.onClick.AddListener(ConfirmationYes);
            noButton.onClick.AddListener(ConfirmationNo);
            dialogText.text = $"Are you sure you want to use {phoenixUp.name} on {characterStats.name}?";
        }

        displayStats.UpdateDisplay();
    }

    // Highlights the tiles where PhoenixUp can be used.
    IEnumerator ShowPhoenixUpUseRange()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Coroutine has finished.");
        ClearHighlightedTiles();

        Character activeCharacter = turnManager.GetActiveCharacter();
        Vector3Int currentTilePos = phoenixUpUseTilemap.WorldToCell(activeCharacter.stats.characterGameObject.transform.position);
        Debug.Log("Current tile position: " + currentTilePos);

        highlightedTiles.Add(currentTilePos);
        phoenixUpUseTilemap.SetTile(currentTilePos, highlightTile);

        HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(currentTilePos, phoenixUpRange);
        Debug.Log("Number of reachable tiles: " + reachableTiles.Count);

        foreach (Vector3Int tilePos in reachableTiles)
        {
            if (tilePos != currentTilePos)
            {
                phoenixUpUseTilemap.SetTile(tilePos, highlightTile);
                highlightedTiles.Add(tilePos);
            }
        }
    }


    // Removes the highlight from previously highlighted tiles.
    public void ClearHighlightedTiles()
    {
        foreach (Vector3Int tilePos in highlightedTiles)
        {
            phoenixUpUseTilemap.SetTile(tilePos, null);
        }

        highlightedTiles.Clear();
    }


    // Begins the wheel mini-game sequence after confirming the use of PhoenixUp.
    private void ConfirmationYes()
    {
        StartCoroutine(StartWheelGame());
        Destroy(activeConfirmationDialog);
    }

    // Starts the wheel mini-game and deactivates certain UI elements for its duration.
    IEnumerator StartWheelGame()
    {
        objectsToKeepDeactivated = new List<GameObject>();

        foreach (GameObject parent in gameObjectsToDeactivate)
        {
            Debug.Log($"Attempting to Deactivate Children of GameObject: {parent.name}");
            foreach (Transform child in parent.transform)
            {
                if (child.name == "Skills Panel" || child.name == "inventory" || child.name == "Example Textbox")
                {
                    objectsToKeepDeactivated.Add(child.gameObject);
                }
                child.gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(2f);
        wheelGameInstance = Instantiate(wheelGamePrefab);
        WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
        Pointer pointerScript = null;
        if (wheelTextController != null)
        {
            pointerScript = wheelTextController.GetPointerScript();
        }

        if (pointerScript != null)
        {
            pointerScript.onEndCollision.AddListener(HandleEndCollision);
        }
        else
        {
            Debug.LogError("Pointer script is null!");
        }

        wheelGameInstance.SetActive(true);
    }


    // Handles the conclusion of the wheel mini-game and applies the PhoenixUp effect multiplier.
    void HandleEndCollision()
    {
        Debug.Log("HandleEndCollision method has been called.");
        Debug.Log("Persistent Data: " + PersistentData.WheelGameScore.ToString());

        if (PersistentData.WheelGameScore >= 0)
        {
            Debug.Log("PhoenixUp effect multiplier after applying wheel game score: " + phoenixUp.effectMultiplier);
            SetPhoenixUpEffectMultiplier(PersistentData.WheelGameScore);
        }

        WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
        Pointer pointerScript = null;
        if (wheelTextController != null)
        {
            pointerScript = wheelTextController.GetPointerScript();
            pointerScript.onEndCollision.RemoveListener(HandleEndCollision);
            wheelTextController.DestroyInstances();
        }

        if (wheelGameInstance != null)
        {
            Destroy(wheelGameInstance);
            wheelGameInstance = null;
        }

        foreach (GameObject parent in gameObjectsToDeactivate)
        {
            Debug.Log($"Attempting to Reactivate Children of GameObject: {parent.name}");
            foreach (Transform child in parent.transform)
            {
                if (!objectsToKeepDeactivated.Contains(child.gameObject))
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        StartCoroutine(ShowPhoenixUpUseRange());
    }

    // Cancels the use of PhoenixUp after the confirmation dialog.
    private void ConfirmationNo()
    {
        Destroy(activeConfirmationDialog);
    }

    // Uses the PhoenixUp item on a specified character.
    private void UsePhoenixUpOnCharacter(CharacterStats targetCharacter)
    {
        // Ensure the target character is dead
        if (targetCharacter.IsDead)  
        {
            phoenixUp.Use(targetCharacter);
            Debug.Log("Used PhoenixUp on character " + targetCharacter.characterName + ". Effect multiplier: " + phoenixUp.effectMultiplier);
            displayStats.UpdateDisplay();
            ClearHighlightedTiles();

            // Check if the character was successfully revived
            if (targetCharacter.hp > 0) 
            {
                SpriteRenderer spriteRenderer = targetCharacter.characterGameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && targetCharacter.originalSprite != null)
                {
                    // Revert to the original sprite
                    spriteRenderer.sprite = targetCharacter.originalSprite;  
                }
            }
        }
        else
        {
            Debug.Log(targetCharacter.characterName + " is already alive. Cannot use PhoenixUp.");
        }
    }

    // Modifies the effect of PhoenixUp based on the wheel game score.
    public void SetPhoenixUpEffectMultiplier(int wheelGameScore)
    {
        if (wheelGameScore >= 10)
        {
         phoenixUp.effectMultiplier = 3.0f;
        }
        else if (wheelGameScore >= 5)
        {
            phoenixUp.effectMultiplier = 2.0f;
        }
        else if (wheelGameScore >= 2)
        {
            phoenixUp.effectMultiplier = 1.5f;
        }
        else
        {
            phoenixUp.effectMultiplier = 1.0f;
        }
        Debug.Log("Wheel game score: " + wheelGameScore + ", PhoenixUp effect multiplier: " + phoenixUp.effectMultiplier);
    }

    // Checks if a tile was clicked and takes appropriate actions based on the character on that tile.
    public void OnTileClick(Vector3Int tilePos)
    {
        if (highlightedTiles.Contains(tilePos))
        {
            CharacterStats targetCharacter;
            if (GameManager.Instance.occupiedTiles.TryGetValue(tilePos, out targetCharacter))
            {
                activeConfirmationDialog = Instantiate(confirmationDialog);
                activeConfirmationDialog.transform.SetParent(Canvas.transform, false);

                Text dialogText = activeConfirmationDialog.GetComponentInChildren<Text>();
                yesButton = activeConfirmationDialog.transform.Find("YesButton").GetComponent<Button>();
                noButton = activeConfirmationDialog.transform.Find("NoButton").GetComponent<Button>();

                yesButton.onClick.AddListener(() => ConfirmationYesWithCharacter(targetCharacter));
                noButton.onClick.AddListener(ConfirmationNo);
                dialogText.text = $"Are you sure you want to use {phoenixUp.name} on {targetCharacter.characterName}?";
            }
            else
            {
                Debug.Log("No character on the tile.");
            }
        }
        else
        {
            Debug.Log("Clicked tile is not in highlighted tiles.");
        }
    }

    // Uses PhoenixUp on a character and updates the game state accordingly.
    private void ConfirmationYesWithCharacter(CharacterStats targetCharacter)
    {
        UsePhoenixUpOnCharacter(targetCharacter);
        Destroy(activeConfirmationDialog);
    }
}