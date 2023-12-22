using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System;

/*
    The PotionButton class manages the behavior of the PotionButton item in the game.
    It provides functions to determine when and how the PotionButton item can be used, displays
    a confirmation dialog to the user, and integrates a mini-game to modify the effect of the item.
*/

public class PotionButton : MonoBehaviour
{
    // ----- Section: Potion Variables -----
    public Item potion;
    public EveryonesStats everyonesStats;
    public DisplayStats displayStats;
    public Tilemap potionUseTilemap;
    public Tile highlightTile;
    public Astar pathfinding;
    private CharacterStats characterStats;
    private List<Vector3Int> highlightedTiles = new List<Vector3Int>();
    public TurnManager turnManager;
    public int potionRange = 1;
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

    public DiceOutcomeForWheelGame diceOutcomeForWheelGame;
    public ChatboxController chatboxController;


    // Handles the action when the Potion button is clicked.
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
            // Instantiate confirmation dialog and get its components
            activeConfirmationDialog = Instantiate(confirmationDialog);
            activeConfirmationDialog.transform.SetParent(Canvas.transform, false);

            Text dialogText = activeConfirmationDialog.GetComponentInChildren<Text>();
            yesButton = activeConfirmationDialog.transform.Find("YesButton").GetComponent<Button>();
            noButton = activeConfirmationDialog.transform.Find("NoButton").GetComponent<Button>();
            // Add a new button for the dice rolling option
            Button rollDiceButton = activeConfirmationDialog.transform.Find("RollDiceButton").GetComponent<Button>();
            rollDiceButton.onClick.AddListener(RollDiceButtonClicked);

            // Set up button actions and dialog text
            yesButton.onClick.AddListener(ConfirmationYes);
            noButton.onClick.AddListener(ConfirmationNo);
            dialogText.text = $"Are you sure you want to use {potion.name}?, Pressing Yes will start the wheel game";

           // dialogText.text = $"Are you sure you want to use {potion.name} on {characterStats.name}?";
        }

        displayStats.UpdateDisplay();
    }
    private void Start()
    {
        // Get the button component and assign the click event
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }


    public void RollDiceButtonClicked()
    {
        diceOutcomeForWheelGame.RollDiceButtonClicked(OnDiceRollingCompletedAndShowPotionRange);
    }


    private void OnDiceRollingCompletedAndShowPotionRange()
    {
        SetPotionEffectMultiplier(PersistentData.WheelGameScore);

        Destroy(activeConfirmationDialog); 
        StartCoroutine(ShowPotionUseRange());
    }


    // Shows tiles within a specific range where the potion can be used.
    IEnumerator ShowPotionUseRange()
    {

        yield return new WaitForSeconds(2f);

        Debug.Log("Coroutine has finished.");

        // Clear any previously highlighted tiles
        ClearHighlightedTiles();

        Character activeCharacter = turnManager.GetActiveCharacter();

        // Get the current tile position of the character
        Vector3Int currentTilePos = potionUseTilemap.WorldToCell(activeCharacter.stats.characterGameObject.transform.position);
        Debug.Log("Current tile position: " + currentTilePos);

        // Add current character's tile to the highlighted tiles list and set the tile at this position to the highlight tile
        highlightedTiles.Add(currentTilePos);
        potionUseTilemap.SetTile(currentTilePos, highlightTile);

        HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(currentTilePos, potionRange);
        Debug.Log("Number of reachable tiles: " + reachableTiles.Count);

        foreach (Vector3Int tilePos in reachableTiles)
        {
            // Exclude the current tile position
            if (tilePos != currentTilePos) 
            {
                // Set the tile at the reachable position to the highlight tile
                potionUseTilemap.SetTile(tilePos, highlightTile);
                // Add the tile position to the list of highlighted tiles
                highlightedTiles.Add(tilePos); 
            }
        }
    }


    // Removes the highlighted effect from tiles where the potion can be used.
    public void ClearHighlightedTiles()
    {
        foreach (Vector3Int tilePos in highlightedTiles)
        {
            potionUseTilemap.SetTile(tilePos, null); 
        }

        highlightedTiles.Clear(); 
    }

    private void Update()
    {
        // If the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Calculate the mouse position in world coordinates
            Vector3 clickScreenPosition = Input.mousePosition;
            Vector2 clickRay = Camera.main.ScreenToWorldPoint(clickScreenPosition);

            // Convert the mouse position to tile coordinates
            Vector3Int clickTilePos = potionUseTilemap.WorldToCell(clickRay);

            // Log mouse click positions for debugging
            Debug.Log("Mouse Position: " + clickScreenPosition);
            Debug.Log("Click Ray Position: " + clickRay);
            Debug.Log("Click Tile Position: " + clickTilePos);

            // Call the OnTileClick function
            OnTileClick(clickTilePos);
        }
    }

    // Confirms the intention to use the potion and starts the mini-game.
    private void ConfirmationYes()
    {
        // Load the wheel game scene
        StartCoroutine(StartWheelGame());
        Destroy(activeConfirmationDialog);

    }

    // Starts the wheel mini-game, deactivating certain UI elements during its play.
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

        // Assuming some delay before Wheel Game starts
        yield return new WaitForSeconds(2f);

        // Instantiate wheel game and set it active
        wheelGameInstance = Instantiate(wheelGamePrefab);
        WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
        Pointer pointerScript = null;
        if (wheelTextController != null)
        {
            pointerScript = wheelTextController.GetPointerScript();
        }

        // Make sure pointerScript is not null before subscribing to its event
        if (pointerScript != null)
        {
            // Subscribe to the event
            pointerScript.onEndCollision.AddListener(HandleEndCollision);
        }
        else
        {
            Debug.LogError("Pointer script is null!");
        }

        wheelGameInstance.SetActive(true);
    }


    // Handles the end of the wheel mini-game and sets the potion effect multiplier.
    void HandleEndCollision()
    {
        Debug.Log("HandleEndCollision method has been called.");

        // Log the Persistent Data
        Debug.Log("Persistent Data: " + PersistentData.WheelGameScore.ToString());


        if (PersistentData.WheelGameScore >= 0)
        {
            Debug.Log("Potion effect multiplier after applying wheel game score: " + potion.effectMultiplier);

            // apply multiplier to potion
            SetPotionEffectMultiplier(PersistentData.WheelGameScore);
        }


        // Unsubscribe from the event to avoid memory leaks
        WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
        Pointer pointerScript = null;
        if (wheelTextController != null)
        {
            pointerScript = wheelTextController.GetPointerScript();
            pointerScript.onEndCollision.RemoveListener(HandleEndCollision);
            wheelTextController.DestroyInstances();
        }

        // Destroy the wheel game instance
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


        StartCoroutine(ShowPotionUseRange());

    }


    // Cancels the use of the potion after the confirmation dialog.
    private void ConfirmationNo()
    {
        Destroy(activeConfirmationDialog);
    }

    // Uses the potion on a specified character.
    private void UsePotionOnCharacter(CharacterStats targetCharacter)
    {
        float beforeHealth = targetCharacter.hp;

        potion.Use(targetCharacter);

        // After the potion use, get the updated health
        float afterHealth = targetCharacter.hp;

        // Calculate the difference to find out how much the potion has healed
        float healedAmount = afterHealth - beforeHealth;

        chatboxController.AddMessage("Used potion on character " + targetCharacter.characterName + ". Before health: " + beforeHealth + ", After health: " + afterHealth
            + ", Healed amount: " + healedAmount + ". Effect multiplier: " + potion.effectMultiplier);

        displayStats.UpdateDisplay();

        ClearHighlightedTiles();
    }

    // Sets the effect multiplier of the potion based on the wheel game score.
    public void SetPotionEffectMultiplier(int wheelGameScore)
    {
        if (wheelGameScore >= 30)
        {
            potion.effectMultiplier = 3.0f;
        }
        else if (wheelGameScore >= 20)
        {
            potion.effectMultiplier = 2.0f;
        }
        else if (wheelGameScore >= 10)
        {
            potion.effectMultiplier = 1.5f;
        }
        else
        {
            potion.effectMultiplier = 1.0f;
        }

        Debug.Log("Wheel game score: " + wheelGameScore + ", potion effect multiplier: " + potion.effectMultiplier);

    }

    // Checks if a tile was clicked and takes appropriate actions based on the character on that tile.
    public void OnTileClick(Vector3Int tilePos)
    {
        //   Debug.Log("Tile clicked at: " + tilePos); 

        // Check if the clicked tile is within the highlighted tiles

        if (highlightedTiles.Contains(tilePos)) 
        {
            //Debug.Log("Clicked tile is in highlighted tiles."); 

            CharacterStats targetCharacter;
            // if there's a character on the tile
            if (GameManager.Instance.occupiedTiles.TryGetValue(tilePos, out targetCharacter)) 
            {
                // Debug.Log("Character found on tile."); // Debugging if a character is found on the clicked tile

                // Instantiate confirmation dialog and get its components
                activeConfirmationDialog = Instantiate(confirmationDialog);
                activeConfirmationDialog.transform.SetParent(Canvas.transform, false); 

                Text dialogText = activeConfirmationDialog.GetComponentInChildren<Text>();
                yesButton = activeConfirmationDialog.transform.Find("YesButton").GetComponent<Button>();
                noButton = activeConfirmationDialog.transform.Find("NoButton").GetComponent<Button>();

                // Check if dialogText, yesButton and noButton were correctly retrieved
                if (dialogText == null) Debug.Log("dialogText is null.");
                if (yesButton == null) Debug.Log("yesButton is null.");
                if (noButton == null) Debug.Log("noButton is null.");

                // Set up button actions and dialog text
                yesButton.onClick.AddListener(() => ConfirmationYesWithCharacter(targetCharacter));
                noButton.onClick.AddListener(ConfirmationNo);
                dialogText.text = $"Are you sure you want to use {potion.name} on {targetCharacter.name}?";
            }
            else
            {
                // Debugging if no character is found on the clicked tile
                Debug.Log("No character on the tile."); 
            }
        }
        else
        {
            // Debugging if the clicked tile is not within the highlighted tiles
            Debug.Log("Clicked tile is not in highlighted tiles."); 

        }
    }


    // Uses the potion on a character after confirmation and updates the game state.
    private void ConfirmationYesWithCharacter(CharacterStats targetCharacter)
    {
         UsePotionOnCharacter(targetCharacter);
         Destroy(activeConfirmationDialog);

    }

}

