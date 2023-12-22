using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    The IceSpellAoeIndivSkillButton class is responsible for managing the individual skill button for the 
    Water Spell Area of Effect (AOE). This button, when clicked, presents the player with a confirmation dialog. 
    On confirmation, it triggers a mini-game (Wheel Game) that determines the multiplier for the spell effect. 
    Based on the Wheel Game's score, the potency of the spell can be increased. The class also handles 
    activating and deactivating relevant UI elements and manages the spell's effect application.
*/


public class IceSpellAoeIndivSkillButton : MonoBehaviour
{
    // ----- Section: References & UI Components -----

    public Skill skill;
    public TurnManager turnManager;
    public WheelGameManager wheelGameManager;
    public GameObject skillsPanel;
    [SerializeField]
    private ChatboxController chatbox;
    public GameObject[] gameObjectsToDeactivate; 
    public List<GameObject> objectsToKeepDeactivated;
    public GameObject wheelGamePrefab;  
    private GameObject wheelGameInstance; 
    public GameObject confirmationDialog; 
    public Canvas Canvas; 
    private GameObject activeConfirmationDialog; 
    private Button yesButton;
    private Button noButton;
    public DisplayStats displayStats;

    // ----- Section: Button Interactions -----
    public void OnButtonClick()
    {
        Character activeCharacter = turnManager.GetActiveCharacter();
        GameObject activeCharacterGameObject = activeCharacter.stats.characterGameObject;
        var WaterspellAOE = activeCharacterGameObject.GetComponent<IceSpellAoe>();

        if (activeCharacterGameObject == null)
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

            // Set up button actions and dialog text
            yesButton.onClick.AddListener(ConfirmationYes);
            noButton.onClick.AddListener(ConfirmationNo);
            dialogText.text = $"Are you sure you want to use {skill.name}?"; 

        }

        displayStats.UpdateDisplay();
    }


    private void ConfirmationYes()
    {
        // Store target character for potion use
        // Load the wheel game scene
        StartCoroutine(StartWheelGame());
        Destroy(activeConfirmationDialog);

    }

    private void ConfirmationNo()
    {
        // Destroy confirmation dialog
        Destroy(activeConfirmationDialog);
    }

    // ----- Section: Wheel Game Mechanics -----
    IEnumerator StartWheelGame()
    {
        objectsToKeepDeactivated = new List<GameObject>();

        foreach (GameObject parent in gameObjectsToDeactivate)
        {
            Debug.Log($"Attempting to Deactivate Children of GameObject: {parent.name}");
            foreach (Transform child in parent.transform)
            {
                if (child.name == "Skills Panel" || child.name == "inventory" || child.name == "Example Textbox") // or any other condition
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
    void HandleEndCollision()
    {
        Debug.Log("HandleEndCollision method has been called.");

        // Log the Persistent Data
        Debug.Log("Persistent Data: " + PersistentData.WheelGameScore.ToString());



        if (PersistentData.WheelGameScore >= 0)
        {
           // Debug.Log("Potion effect multiplier after applying wheel game score: " + potion.effectMultiplier);

            // apply multiplier to potion
            SetSpellEffectMultiplier(PersistentData.WheelGameScore);
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


        StartCoroutine(ShowSpellUseRange());

    }


    private void Start()
    {
        // Get the button component and assign the click event
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    public void SetSpellEffectMultiplier(int wheelGameScore)
    {
        Debug.Log("Setting spell effect multiplier with score: " + wheelGameScore);

        Character activeCharacter = turnManager.GetActiveCharacter();
        GameObject activeCharacterGameObject = activeCharacter.stats.characterGameObject;
        var WaterspellAOE = activeCharacterGameObject.GetComponent<IceSpellAoe>();

        if (wheelGameScore > 20)
        {
            WaterspellAOE.turnsFrozen += 1;
        }
    }

    public void StartSpellRangeCoroutine()
    {
        Debug.Log("Starting ShowSpellUseRange coroutine.");
        StartCoroutine(ShowSpellUseRange());
    }

    IEnumerator ShowSpellUseRange()
    {
        Debug.Log("Entered ShowSpellUseRange coroutine.");

        yield return new WaitForSeconds(2f);

        Debug.Log("Coroutine has finished.");

        Character activeCharacter = turnManager.GetActiveCharacter();
        GameObject activeCharacterGameObject = activeCharacter.stats.characterGameObject;
        var WaterspellAOE = activeCharacterGameObject.GetComponent<IceSpellAoe>();

        WaterspellAOE.ToggleSpellCasting();
    }




}



//  Debug.Log("Wheel game score: " + wheelGameScore + ", Damage: " + WaterspellAOE.damage + ", Freeze Duration: " + WaterspellAOE.freezeDuration);


