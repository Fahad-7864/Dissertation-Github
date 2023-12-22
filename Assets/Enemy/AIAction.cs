using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    The AIAction is an abstract class that defines the structure for individual AI actions.
    Each AI action has a type (Movement or Utility), a priority level, a utility evaluation, 
    and an execution method.
*/

public abstract class AIAction : ScriptableObject
{

    public ActionType actionType;  


    public abstract int GetPriorityLevel(GameObject unit);

    public abstract float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber);

    public abstract void Execute(GameObject unit, AISituationGrabber situationGrabber);
}


public enum ActionType
{
    Movement,
    Utility
}
