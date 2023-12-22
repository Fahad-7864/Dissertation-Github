using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The WheelGameManager class manages the lifecycle of the wheel game 
    within the application. It handles the initialization, scoring, and 
    ending of the wheel game. Additionally, this class provides mechanisms 
    to interact with other game elements and UI components, enabling and 
    disabling them as necessary during the wheel game's execution.
*/
public class WheelGameManager : MonoBehaviour
{
    // ----- Section: References and Configuration -----
    public GameObject wheelGamePrefab;
    private GameObject wheelGameInstance;
    public GameObject[] gameObjectsToDeactivate;
    public List<GameObject> objectsToKeepDeactivated;

    // ----- Section: Delegates and Events -----
    // These events and delegates provide mechanisms for other components to respond to the wheel game's events.

    // Event to notify when the wheel game scores
    public delegate void WheelGameScoreDelegate(int score);
    public event WheelGameScoreDelegate OnWheelGameScored;

    // Event to notify when the wheel game ends
    public delegate void WheelGameEndDelegate();
    public event WheelGameEndDelegate OnWheelGameEnded;

    // ----- Section: Wheel Game Lifecycle -----
    // These methods control the start and end of the wheel game.
    public IEnumerator StartWheelGame()
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
        Pointer pointerScript = wheelTextController.GetPointerScript();

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

    // Handle the end collision event of the pointer in the wheel game.
    void HandleEndCollision()
    {
        Debug.Log("HandleEndCollision method has been called.");
        Debug.Log("Persistent Data: " + PersistentData.WheelGameScore.ToString());

        OnWheelGameScored?.Invoke(PersistentData.WheelGameScore);

        WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
        Pointer pointerScript = wheelTextController.GetPointerScript();
        pointerScript.onEndCollision.RemoveListener(HandleEndCollision);

        DestroyWheelAndCanvas();

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

        OnWheelGameEnded?.Invoke(); // Notify listeners that the wheel game has ended
    }

    // Destroy wheel and canvas game objects.

    public void DestroyWheelAndCanvas()
    {
        if (wheelGameInstance != null)
        {
            WheelTextController wheelTextController = wheelGameInstance.GetComponent<WheelTextController>();
            if (wheelTextController != null)
            {
                wheelTextController.DestroyInstances();
            }
        }
    }
}

