using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


/*
    The Pointer class is responsible for controlling the behavior of the pointer 
    in a wheel game. The class governs the movement, collision detection, scoring,
    and UI updates of the pointer. It also handles game phases and transitions
    between them, ensuring the gameplay progresses smoothly and intuitively.
*/

public class Pointer : MonoBehaviour
{
    // ----- Section: Essential Variables for Pointer Movement -----
    public Transform wheelCenter;
    public float speed = -30f; // Make it negative for clockwise rotation
    private bool isMoving = false;
    private float radius;

    // ----- Section: UI and Text Components -----
    public Transform pointTransform;
    public GameObject pointer; 
    [SerializeField]
    public TextMeshProUGUI phase1ScoreText, phase2ScoreText, phase3ScoreText; private int phase1Score = 0, phase2Score = 0, phase3Score = 0;
    [SerializeField]
    public TextMeshProUGUI popupText;
    [SerializeField]
    public TextMeshProUGUI totalScoreText;
    [SerializeField]
    public TextMeshProUGUI resultText;

    // ----- Section: Game Logic & Phases -----

    [SerializeField]
    private bool hasUsedTurn = false; 
    private bool scoreAddedThisTurn = false;
    private bool hasCollided = false;
    private bool phase1ScoreSet = false, phase2ScoreSet = false, phase3ScoreSet = false;
    public WheelType wheelType;
    private GamePhase currentPhase = GamePhase.Phase1;



    public UnityEvent onEndCollision;

    public enum GamePhase { Phase1, Phase2, Phase3, Finished }
    public enum WheelType
    {
      ForwardWheel,
      BackwardWheel,
    }

    // Initializes variables and sets up initial configurations.
    void Start()
    {
        //Set the speed according to the wheel type
        speed = GetSpeedBasedOnWheelType();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("Missing Rigidbody2D component!");
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        radius = Vector3.Distance(transform.position, wheelCenter.position);
        // isMoving = true;

    }

    // Updates the pointer's behavior every frame.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !hasUsedTurn) // Check if space was pressed and if the turn has not been used
        {
            isMoving = !isMoving;
            if (!isMoving)
            {
                Debug.Log("Pointer stopped. Checking for score...");

                hasUsedTurn = true; // Set the turn as used
                scoreAddedThisTurn = false; // Reset the score added flag
                CheckAndAddScore();
                //CheckPosition();
                Invoke("ResumeMoving", 2f);

            }
        }

        if (isMoving)
        {
            Debug.DrawLine(wheelCenter.position, pointTransform.position, Color.red);

            float angle = speed * Time.deltaTime;
            transform.RotateAround(wheelCenter.position, Vector3.forward, angle);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

            // This line sets the position of the object to always be `radius` units away from `wheelCenter`
            transform.position = wheelCenter.position + (transform.position - wheelCenter.position).normalized * radius;

        }
    }


    // Resumes the pointer's movement.
    void ResumeMoving()
    {
        isMoving = true;
    }

    // Checks for collisions and updates the score.
    void CheckAndAddScore()
    {
        Vector2 boxSize = new Vector2(0.08f, 0.1f); 
        Vector2 offset = 0.5f * pointTransform.up; 
        Vector2 boxPosition = (Vector2)pointTransform.position + offset;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f);

        // First, check for any "Perfect" regions
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Perfect"))
            {
                resultText.text = "The pointer stopped on a Perfect region!";
                AddScore(10); // Add score for the Perfect region
                return;
            }
        }

        // If no "Perfect" region was found, check for "Good" and "Bad" regions
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Good"))
            {
                resultText.text = "The pointer stopped on a Good region!";
                AddScore(5); 
                return;
            }

            if (collider.CompareTag("Bad"))
            {
                resultText.text = "The pointer stopped on a Bad region!";
                AddScore(2); 
                return;
            }
        }
        

    }

    // Draws debug visualizations in the Unity editor.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector2 boxSize = new Vector2(0.08f, 0.1f);
        Vector2 offset = 0.5f * pointTransform.up; 
        Vector2 boxPosition = (Vector2)pointTransform.position + offset;
        // Draw a wireframe cube at the position of pointTransform with the size of boxSize
        Gizmos.DrawWireCube(boxPosition, boxSize); 
    }

    // Handles the pointer's collision with game regions.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Phase1End") && !hasCollided)
        {
            hasCollided = true;

            StartCoroutine(Phase1EndCollision());
        }
        if (collision.CompareTag("Phase2End") && !hasCollided)
        {
            hasCollided = true;

            StartCoroutine(Phase2EndCollision());
        }

        if (collision.CompareTag("Phase3End") && !hasCollided)
        {
            hasCollided = true;

            StartCoroutine(Phase3EndCollision());
        }
    }


    // Determines the pointer's speed based on the type of wheel.
    public float GetSpeedBasedOnWheelType()
    {
        switch (wheelType)
        {
            case WheelType.ForwardWheel:
                return -60f;
            case WheelType.BackwardWheel:
                return 60f;
            default:
                return 0f;
        }
    }

    // Handles the game's behavior at the end of Phase 1.
    IEnumerator Phase1EndCollision()
    {
        speed = 0f;

        // Wait for 2 seconds
        yield return new WaitForSeconds(0.5f);

        // Restart the pointer's movement
        isMoving = true;
        speed = GetSpeedBasedOnWheelType();
        hasCollided = false;  
        hasUsedTurn = false; // Reset the turn use flag so the user can press space again
        phase1ScoreText.text = "Phase 1 Score: " + phase1Score.ToString(); // Display phase 1 score

        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); // Update total score text
        currentPhase = GamePhase.Phase2;


    }

    // Handles the game's behavior at the end of Phase 2.
    IEnumerator Phase2EndCollision()
    {
        speed = 0f;

        yield return new WaitForSeconds(0.5f);

        isMoving = true;
        speed = GetSpeedBasedOnWheelType();
        hasCollided = false;  
        hasUsedTurn = false; 

        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); 
        phase2ScoreText.text = "Phase 2 Score: " + phase2Score.ToString();
        currentPhase = GamePhase.Phase3;

    }

    // Handles the game's behavior at the end of Phase 3.
    IEnumerator Phase3EndCollision()
    {
        speed = 0f;
        Debug.Log("Pointer has collided with the end!");

        yield return new WaitForSeconds(0.5f);

        isMoving = true;
        speed = GetSpeedBasedOnWheelType();
        hasCollided = false;  
        hasUsedTurn = false; 

        phase3ScoreText.text = "Phase 3 Score: " + phase3Score.ToString(); 
        currentPhase = GamePhase.Finished;
        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); 

        // Display the popup text and wait for 2 seconds
        popupText.text = "Loading main scene...";
        yield return new WaitForSeconds(2f);
        Debug.Log("Invoking onEndCollision event.");
        onEndCollision.Invoke();

        //SceneManager.LoadScene("ItemScene");
    }




    // Adds points to the player's score.
    void AddScore(int points)
    {
        if (!isMoving && hasUsedTurn && !scoreAddedThisTurn)
        {
            Debug.Log("AddScore called with: " + points);
            scoreAddedThisTurn = true; // Indicate that score has been added for this turn

            switch (currentPhase)
            {
                case GamePhase.Phase1:
                    if (!phase1ScoreSet)
                    {
                        phase1Score = points; // Set the score directly
                        phase1ScoreSet = true; // Indicate that the score for phase 1 has been set
                        Debug.Log("Phase 1 Score: " + phase1Score);
                        PersistentData.WheelGameScore = phase1Score; // Update global game score
                        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); // Update total score text


                    }
                    break;
                case GamePhase.Phase2:
                    if (!phase2ScoreSet)
                    {
                        phase2Score = points; // Set the score directly
                        phase2ScoreSet = true; // Indicate that the score for phase 2 has been set
                        Debug.Log("Phase 2 Score: " + phase2Score);
                        PersistentData.WheelGameScore = phase1Score + phase2Score; // Update global game score
                        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); // Update total score text


                    }
                    break;
                case GamePhase.Phase3:
                    if (!phase3ScoreSet)
                    {
                        phase3Score = points; // Set the score directly
                        phase3ScoreSet = true; // Indicate that the score for phase 3 has been set
                        Debug.Log("Phase 3 Score: " + phase3Score);
                        PersistentData.WheelGameScore = phase1Score + phase2Score + phase3Score; // Update global game score
                        totalScoreText.text = "Total Score: " + PersistentData.WheelGameScore.ToString(); // Update total score text

                    }
                    break;
                default:
                    Debug.Log("Game is finished. No more score updates.");
                    break;
            }
        }
    }




}

