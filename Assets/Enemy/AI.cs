using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The AI class is responsible for managing the Artificial Intelligence decisions 
    of the game entities. It evaluates and determines the best action based on the 
    current situation for both movement and utility actions.
*/
public class AI : MonoBehaviour
{
    // ----- Section: AI Actions and Decision Variables -----
    public AIAction[] actions;
    public AIAction bestAction;


    // Separate lists for movement and utility actions
    [SerializeField]
    private List<AIAction> movementActions;
    [SerializeField]
    public List<AIAction> utilityActions;

    public ChatboxController chatbox;

    // ----- Section: Initialization -----
    // Initialize and classify AI actions into movement and utility categories.
    private void Start()
    {
        // Initialize the lists
        movementActions = new List<AIAction>();
        utilityActions = new List<AIAction>();

        // Fill the lists based on action type
        foreach (AIAction action in actions)
        {
           // Debug.Log("Action: " + action.name + ", Type: " + action.actionType);

            if (action.actionType == ActionType.Movement)
            {
                movementActions.Add(action);
            }
            else if (action.actionType == ActionType.Utility)
            {
                utilityActions.Add(action);
            }
        }
    }

    // ----- Section: AI Decision Mechanics -----

    // Determines the best movement action for the AI based on the current situation.
    public void ChooseBestMovementAction(AISituationGrabber situationGrabber, GameObject gameObject)
    {
        CharacterStats stats = gameObject.GetComponent<CharacterStats>();

        List<AIAction> priority1Movements = new List<AIAction>();
        List<AIAction> priority2Movements = new List<AIAction>();
        List<AIAction> priority3Movements = new List<AIAction>();

        foreach (AIAction action in movementActions)  
        {
            int priority = action.GetPriorityLevel(gameObject);
            switch (priority)
            {
                case 1:
                    priority1Movements.Add(action);
                    break;
                case 2:
                    priority2Movements.Add(action);
                    break;
                case 3:
                    priority3Movements.Add(action);
                    break;
                default:
                    Debug.Log("Unknown priority level: " + priority);
                    break;
            }
        }

        // Choose the best movement action
        AIAction bestAction = null;
        bestAction = ChooseBestMovementActionFromBucket(priority1Movements, situationGrabber, gameObject);
        Debug.Log(GetBucketDebugInfo(priority1Movements, situationGrabber, gameObject));

        if (bestAction == null || bestAction.EvaluateUtility(gameObject, situationGrabber) <= 0)
        {
            bestAction = ChooseBestMovementActionFromBucket(priority2Movements, situationGrabber, gameObject);
            Debug.Log(GetBucketDebugInfo(priority2Movements, situationGrabber, gameObject));

        }
        if (bestAction == null || bestAction.EvaluateUtility(gameObject, situationGrabber) <= 0)
        {
            bestAction = ChooseBestMovementActionFromBucket(priority3Movements, situationGrabber, gameObject);
            Debug.Log(GetBucketDebugInfo(priority2Movements, situationGrabber, gameObject));

        }

        if (bestAction != null)
        {
            Debug.Log("Best movement action: " + bestAction.GetType().Name);
            bestAction.Execute(gameObject, situationGrabber);
            chatbox.AddMessage($"AI chose movement action: {bestAction.GetType().Name}");
        }


    }


    // Determines the best utility action for the AI based on the current situation.
    public void ChooseBestUtilityAction(AISituationGrabber situationGrabber, GameObject gameObject)
    {
        CharacterStats stats = gameObject.GetComponent<CharacterStats>();

        List<AIAction> priority1Actions = new List<AIAction>();
        List<AIAction> priority2Actions = new List<AIAction>();
        List<AIAction> priority3Actions = new List<AIAction>();

        foreach (AIAction action in utilityActions)  
        {
            int priority = action.GetPriorityLevel(gameObject);
            switch (priority)
            {
                case 1:
                    priority1Actions.Add(action);
                    break;
                case 2:
                    priority2Actions.Add(action);
                    break;
                case 3:
                    priority3Actions.Add(action);
                    break;
                default:
                    Debug.Log("Unknown priority level: " + priority);
                    break;
            }
        }

        // Choose the best utility action
        if (priority1Actions.Count > 0)
        {
            bestAction = ChooseBestUtilityActionFromBucket(priority1Actions, situationGrabber, gameObject);
            Debug.Log(GetBucketDebugInfo(priority1Actions, situationGrabber, gameObject));
        }
        else if (priority2Actions.Count > 0)
        {
            bestAction = ChooseBestUtilityActionFromBucket(priority2Actions, situationGrabber, gameObject);
            Debug.Log(GetBucketDebugInfo(priority2Actions, situationGrabber, gameObject));
        }
        else if (priority3Actions.Count > 0)
        {
            bestAction = ChooseBestUtilityActionFromBucket(priority3Actions, situationGrabber, gameObject);
            Debug.Log(GetBucketDebugInfo(priority3Actions, situationGrabber, gameObject));
        }


        if (bestAction != null)
        {
            Debug.Log("Best utility action: " + bestAction.GetType().Name);
            bestAction.Execute(gameObject, situationGrabber);
            chatbox.AddMessage($"AI chose utility action: {bestAction.GetType().Name}");

        }
    }


    // Selects the best movement action from a given list based on its utility value.
    private AIAction ChooseBestMovementActionFromBucket(List<AIAction> actionBucket, AISituationGrabber situationGrabber, GameObject gameObject)
    {
        float bestUtility = float.NegativeInfinity;
        AIAction bestAction = null;

        foreach (AIAction action in actionBucket)
        {
            // Ensure that the action is of type Movement
            if (action.actionType == ActionType.Movement)
            {
                float utility = action.EvaluateUtility(gameObject, situationGrabber);
                Debug.Log(action.GetType().Name + " utility: " + utility);

                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    bestAction = action;
                }
            }
        }

        return bestAction;
    }


    // Selects the best utility action from a given list based on its utility value.
    private AIAction ChooseBestUtilityActionFromBucket(List<AIAction> actionBucket, AISituationGrabber situationGrabber, GameObject gameObject)
    {
        float bestUtility = float.NegativeInfinity;
        AIAction bestAction = null;

        foreach (AIAction action in actionBucket)
        {
            // Ensure that the action is of type Utility
            if (action.actionType == ActionType.Utility)
            {
                float utility = action.EvaluateUtility(gameObject, situationGrabber);
                Debug.Log(action.GetType().Name + " utility: " + utility);

                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    bestAction = action;
                }
            }
        }

        return bestAction;
    }

    // Provides debugging information for each action in the bucket, detailing their utility values.
    private string GetBucketDebugInfo(List<AIAction> actionBucket, AISituationGrabber situationGrabber, GameObject gameObject)
    {
        string debugInfo = "";
        foreach (AIAction action in actionBucket)
        {
            float utility = action.EvaluateUtility(gameObject, situationGrabber);
            debugInfo += "\nAction: " + action.GetType().Name + ", Utility: " + utility;
        }
        return debugInfo;
    }
}




