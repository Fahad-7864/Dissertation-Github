using TMPro;
using UnityEngine;

/*
    The WheelTextController class is responsible for managing the instantiation and 
    setup of the wheel game's UI elements and the wheel itself. It ensures that all 
    necessary components, such as the pointer and associated text displays, are properly 
    connected. This class provides seamless integration between the wheel game's visual 
    representation and its logic.
*/

public class WheelTextController : MonoBehaviour
{
    // ----- Section: Configuration Variables -----
    public GameObject canvasPrefab;
    public GameObject[] wheelPrefabs;

    private GameObject wheelInstance;
    private GameObject canvasInstance;

    // ----- Section: Initialization -----
    // This section deals with the initialization and setup of the wheel and its UI elements.
   public void Awake()
    {
        Vector3 centerPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));

        if (canvasPrefab != null)
        {
            if (wheelPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, wheelPrefabs.Length);
                wheelInstance = Instantiate(wheelPrefabs[randomIndex], centerPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogError("No wheel prefabs available!");
            }

            canvasInstance = Instantiate(canvasPrefab, Vector3.zero, Quaternion.identity);
            Pointer pointerScript = wheelInstance.GetComponentInChildren<Pointer>();
            if (pointerScript != null)
            {
                pointerScript.resultText = canvasInstance.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();
                if (pointerScript.resultText == null)
                {
                    Debug.LogError("Could not find ResultText on the canvas!");
                }

                pointerScript.phase1ScoreText = canvasInstance.transform.Find("Phase1ScoreText").GetComponent<TextMeshProUGUI>();
                pointerScript.phase2ScoreText = canvasInstance.transform.Find("Phase2ScoreText").GetComponent<TextMeshProUGUI>();
                pointerScript.phase3ScoreText = canvasInstance.transform.Find("Phase3ScoreText").GetComponent<TextMeshProUGUI>();
                if (pointerScript.phase1ScoreText == null || pointerScript.phase2ScoreText == null || pointerScript.phase3ScoreText == null)
                {
                    Debug.LogError("Could not find phase score text on the canvas!");
                }

                pointerScript.totalScoreText = canvasInstance.transform.Find("TotalScoreText").GetComponent<TextMeshProUGUI>();
                if (pointerScript.totalScoreText == null)
                {
                    Debug.LogError("Could not find TotalScoreText on the canvas!");
                }

                pointerScript.popupText = canvasInstance.transform.Find("PopupText").GetComponent<TextMeshProUGUI>();
                if (pointerScript.popupText == null)
                {
                    Debug.LogError("Could not find PopupText on the canvas!");
                }
            }
            else
            {
                Debug.LogError("The wheel instance does not have a Pointer script attached!");
            }
        }
        else
        {
            Debug.LogError("WheelPrefab and/or CanvasPrefab is not assigned!");
        }
    }

    // ----- Section: Utility Methods -----


    // Get the Pointer script from the instantiated wheel instance.
    public Pointer GetPointerScript()
    {
        if (wheelInstance != null)
        {
            Pointer pointer = wheelInstance.GetComponentInChildren<Pointer>();

            if (pointer != null)
            {
                Debug.Log("Pointer instance found in wheelInstance!");
                return pointer;
            }
            else
            {
                Debug.LogError("Could not find Pointer instance in wheelInstance!");
            }
        }
        else
        {
            Debug.LogError("WheelInstance is null!");
        }

        return null;
    }

    // Destroy the instantiated wheel and canvas game objects.
    public void DestroyInstances()
    {
        if (wheelInstance != null)
        {
            Destroy(wheelInstance);
            wheelInstance = null;
        }
        if (canvasInstance != null)
        {
            Destroy(canvasInstance);
            canvasInstance = null;
        }
    }

    public string GetCurrentWheelPrefabName()
    {
        if (wheelInstance != null)
        {
            return wheelInstance.name.Replace("(Clone)", "").Trim(); // Remove the "(Clone)" part from the name
        }
        else
        {
            Debug.LogError("WheelInstance is null!");
            return "Unknown";
        }
    }


}