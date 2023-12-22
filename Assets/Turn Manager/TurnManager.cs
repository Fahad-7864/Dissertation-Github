using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/*
   The TurnManager class displays the turn-based mechanics in the game. 
   It ensures that characters take turns based on their speed and energy levels, 
   while also providing utilities for actions like movement, utility actions, 
   and handling special states like 'frozen' or 'dead'. Additionally, this class 
   provides visualization for the turn order, facilitating player strategy. 
   As a singleton, this class is the central hub for turn-based logic.
*/
public enum ActionPhase
{
    UtilityBeforeMovement,
    MovementIfUtilityExecuted,
    MovementIfNoUtilityExecuted,
    UtilityAfterMovement
}

public class TurnManager : MonoBehaviour
{

    // ----- Section: Configuration Variables -----
    public float tickTime = 1f;
    public int energyThreshold = 100;
    public GameObject turnOrderPanel;
    public EveryonesStats everyonesStats;
    Character activeCharacter;
    Queue<Character> characters;
    public GameManager gameManager; 
    public DisplayStats displayStats;
    [SerializeField]
    private RetreatAction retreatAction;
    [SerializeField]
    private ChatboxController chatbox;


    // Initializes the game with character stats and their order for taking turns.
    private void Start()
    {
        // Initialize characters from EveryonesStats
        var characterStatsList = everyonesStats.allCharacterStats;

        // Randomize the order
        System.Random rng = new System.Random();
        characterStatsList = characterStatsList.OrderBy(a => rng.Next()).ToList();

        var charactersList = new List<Character>();
        foreach (var stats in characterStatsList)
        {
            charactersList.Add(new Character(stats));
        }

        // Sort characters by speed, from highest to lowest
        charactersList.Sort((a, b) => b.stats.speed.CompareTo(a.stats.speed));

        // Enqueue characters to queue
        characters = new Queue<Character>(charactersList);

        // Initialize panel at start
        foreach (var character in characters)
        {
            UpdateOrderPanel(character);
        }

        StartCoroutine(Tick());
    }

    // This coroutine handles the turn logic for each character in the game.
    IEnumerator Tick()
    {
        while (true)
        {
            // Dequeue the character from the front of the queue
            activeCharacter = characters.Dequeue();
            activeCharacter.stats.energy += activeCharacter.stats.speed;
            activeCharacter.stats.actionPoints = 1;

            // Check if the character is frozen.
            if (activeCharacter.stats.IsFrozen)
            {
                if (activeCharacter.stats.energy >= energyThreshold)
                {
                    // If frozen character's energy is over the threshold, show their turn in the panel but don't let them act.
                    string frozenMessage = $"{activeCharacter.stats.characterName} is frozen but has full energy!";
                    Debug.Log(frozenMessage);

                    // Send to Chatbox
                    chatbox.AddMessage(frozenMessage + " Their turn is skipped.");

                    UpdateOrderPanel(activeCharacter);

                    // Indicate that it's their turn, even though they won't act.
                    activeCharacter.stats.SetCharacterTurn(true);

                    // Wait for 2 seconds to let the player see the updated panel
                    yield return new WaitForSeconds(2.0f);

                    // Reset energy
                    activeCharacter.stats.energy = 0;

                    // Increment the number of turns taken
                    activeCharacter.stats.turnsTaken++;
                    activeCharacter.stats.turnsFrozen--;

                    // End their turn
                    activeCharacter.stats.SetCharacterTurn(false);
                }

                characters.Enqueue(activeCharacter);
                continue;
            }


            if (activeCharacter.stats.IsDead)
            {
                // Remove sprite from game
                //activeCharacter.stats.characterGameObject.SetActive(false);

                // Make the corresponding character image in the turnOrderPanel inactive
                Transform child = turnOrderPanel.transform.Find(activeCharacter.stats.characterName);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }

                // Remove the dead character from the queue
                RemoveCharacterFromQueue(activeCharacter);

                // Continue to next character's turn
                continue;
            }
           
 
            if (activeCharacter.stats.energy >= energyThreshold)
            {

                LineOfArrows lineOfArrows = activeCharacter.stats.characterGameObject.GetComponent<LineOfArrows>();
                if (lineOfArrows != null)
                {
                    lineOfArrows.ReduceCooldown();
                }
                //Subtract energyThreshold from character's energy 

                //activeCharacter.stats.energy -= energyThreshold;
                Debug.Log(activeCharacter.stats.characterName + "'s turn!");
                chatbox.AddMessage(activeCharacter.stats.characterName + "'s turn!"); 
                displayStats.ShowStatsByGameObject(activeCharacter.stats.characterGameObject);

                // Use SetCharacterTurn 
                activeCharacter.stats.SetCharacterTurn(true);

                // Subscribe to the event
                activeCharacter.stats.CharacterTurnChanged += HandleCharacterTurnChanged;

                // Update the order panel
                UpdateOrderPanel(activeCharacter);
                if (activeCharacter.stats.type == CharacterType.Enemy)
                {
          

                    var enemyMover = activeCharacter.stats.characterGameObject.GetComponent<EnemyMover>();
                    var enemyAI = activeCharacter.stats.characterGameObject.GetComponent<AI>();
                    var situationGrabber = activeCharacter.stats.characterGameObject.GetComponent<AISituationGrabber>();
                    // New variable to track whether a utility action was executed
                    bool utilityActionExecuted = false;
                    // Define a flag outside the loop to track if there is a need to break
                    bool breakLoop = false;

                    if (enemyMover != null && enemyAI != null && situationGrabber != null)
                    {
                        // Iterate through the four phases defined in the ActionPhase enum
                        foreach (ActionPhase phase in Enum.GetValues(typeof(ActionPhase)))
                        {
                            if (breakLoop) 
                                break;
                            CharacterStats stats = activeCharacter.stats; 
                            switch (phase)
                            {
                                case ActionPhase.UtilityBeforeMovement:

                                    if (stats.characterClass == CharacterClass.Warrior && situationGrabber.CanMoveToPreferredTile())
                                    {
                                        // If the boolean is true then skip this case
                                        break;

                                    }

                                    // Attempt to execute a utility action before movement
                                    utilityActionExecuted = ExecuteUtilityPhase(activeCharacter, enemyAI, situationGrabber);
                                    // If a utility action was executed, wait for 2 seconds before moving on

                                    if (utilityActionExecuted)
                                        Debug.Log("Executing Utility Before Movement Phase");
                                    yield return new WaitForSeconds(1.5f);
                                    break;
                                case ActionPhase.MovementIfUtilityExecuted:
                                    // Execute the movement phase only if a utility action was previously executed
                                    if (utilityActionExecuted)
                                    {
                                        Debug.Log("Executing Movement If Utility Executed Phase");
                                        if (stats.characterClass == CharacterClass.Archer)
                                        {
                                            Debug.Log("Archer has executed a utility action, altering movement strategy.");
                                            retreatAction.Execute(activeCharacter.stats.characterGameObject, situationGrabber);

                                            //situationGrabber.StartCalculateRetreatMovement(unit);

                                        }
                                        else
                                        {
                                            ExecuteMovementPhase(enemyAI, situationGrabber, activeCharacter.stats.characterGameObject);
                                        }
                                        yield return new WaitForSeconds(1.5f);
                                        // Set the flag to true to break the loop
                                        breakLoop = true; 
                                    }
                                    break;
                                case ActionPhase.MovementIfNoUtilityExecuted:
                                    // Execute the movement phase only if no utility action was previously executed
                                    if (!utilityActionExecuted)
                                    {
                                        Debug.Log("Executing Movement If No Utility Executed Phase");
                                        ExecuteMovementPhase(enemyAI, situationGrabber, activeCharacter.stats.characterGameObject);
                                        yield return new WaitForSeconds(1.5f);
                                    }
                                    break;
                                case ActionPhase.UtilityAfterMovement:
                                    Debug.Log("Executing Utility After Movement Phase");
                                    // Execute another utility phase after the movement phase (regardless of whether a utility action was executed before)
                                    if (ExecuteUtilityPhase(activeCharacter, enemyAI, situationGrabber))
                                        Debug.Log("Utility After Movement Phase Executed");

                                    yield return new WaitForSeconds(1.5f);
                                    break;
                            }
                        }

                        // Reset energy and end AI's turn
                        activeCharacter.stats.energy = 0;
                        Debug.Log("Ending AI's turn.");
                        activeCharacter.stats.isCharacterTurn = false;
                        activeCharacter.stats.turnsTaken++;
                    }
                }
                // Wait until it's no longer this character's turn
                yield return new WaitUntil(() => !activeCharacter.stats.isCharacterTurn);

                // Insert the character at the start of the queue with their remaining energy
                List<Character> characterList = new List<Character>(characters);
                characterList.Insert(0, activeCharacter);
                characters = new Queue<Character>(characterList);
            }
            else
            {
                // If the character doesn't have enough energy, put it back in the queue
                characters.Enqueue(activeCharacter);
            }

            // Log the energy of each character after the tick
            //Debug.Log("Energy levels after tick:");
            foreach (Character character in characters)
            {
               Debug.Log(character.stats.characterName + " energy: " + character.stats.energy);
            }

            yield return new WaitForSeconds(tickTime);
        }
    }


    // Evaluates and executes utility actions during a character's turn.
    private bool ExecuteUtilityPhase(Character activeCharacter, AI enemyAI, AISituationGrabber situationGrabber)
    {
        foreach (AIAction action in enemyAI.utilityActions)
        {
            float utility = action.EvaluateUtility(activeCharacter.stats.characterGameObject, situationGrabber);
            if (utility >= 0.1)
            {
                Debug.Log($"Utility action found with utility {utility}: {action.GetType().Name}");
                enemyAI.ChooseBestUtilityAction(situationGrabber, activeCharacter.stats.characterGameObject);
                return true;
            }
        }
        Debug.Log("No utility action found with utility greater than 0.1.");
        return false;
    }


    // Executes movement actions during a character's turn.
    private void ExecuteMovementPhase(AI enemyAI, AISituationGrabber situationGrabber, GameObject characterGameObject)
    {

        Debug.Log("Executing movement phase.");
        enemyAI.ChooseBestMovementAction(situationGrabber, characterGameObject);
    }




    // Handles events when a character's turn changes.
    void HandleCharacterTurnChanged(bool isTurn)
    {
        if (!isTurn)
        {
            // End the current character's turn
            Debug.Log(activeCharacter.stats.characterName + "'s turn ended!");

            // Update the GameManager's occupiedTiles dictionary
            gameManager.PopulateOccupiedTiles();

            // Unsubscribe from the event to avoid multiple subscriptions
            activeCharacter.stats.CharacterTurnChanged -= HandleCharacterTurnChanged;

            // Continue the tick
            activeCharacter.stats.SetCharacterTurn(false); 
        }
    }

    // Ends the current character's turn when the end turn button is clicked.
    public void OnEndTurnButtonClick()
    {
        if (activeCharacter.stats.isCharacterTurn)
        {
            Debug.Log(activeCharacter.stats.characterName + "'s turn ended!");

            // Update the GameManager's occupiedTiles dictionary
            gameManager.PopulateOccupiedTiles(); 


            activeCharacter.stats.energy = 0;

            // Continue the tick
            activeCharacter.stats.isCharacterTurn = false;
        }
    }


    // Removes a character from the turn queue, typically used when a character dies.
    private void RemoveCharacterFromQueue(Character characterToRemove)
    {
        // Create a new list without the dead character
        List<Character> charactersList = new List<Character>(characters.Where(character => character != characterToRemove));

        // Convert list back to queue
        characters = new Queue<Character>(charactersList);
    }


    void UpdateOrderPanel(Character activeCharacter)
    {
        // Ensure character images are correctly set
        foreach (Character character in characters)
        {
            Transform child = turnOrderPanel.transform.Find(character.stats.characterName);
            if (child != null)
            {
                if (character.stats.IsDead)  
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
        // Move the active character's image to the front
        Transform activeChild = turnOrderPanel.transform.Find(activeCharacter.stats.characterName);
        if (activeChild != null)
        {
            activeChild.SetAsFirstSibling();
        }
    }



    // Get/Set method to see whos characters turn it is.
    public CharacterStats GetActiveCharacterStats()
    {
        return activeCharacter.stats;
    }
    public Character GetActiveCharacter()
    {
        return activeCharacter;
    }


}
public class Character
{
    public CharacterStats stats;

    public Character(CharacterStats stats)
    {
        this.stats = stats;
        this.stats.energy = 0;
        this.stats.actionPoints = 0;


    }
}



