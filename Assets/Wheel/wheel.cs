using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class wheel : MonoBehaviour
{
    public enum Phase { Phase1, Phase2, Phase3 }
    public Phase currentPhase;

    public TextMeshProUGUI resultText;
    public TextMeshProUGUI phase1ScoreText, phase2ScoreText, phase3ScoreText;

    public Transform wheelCenter;
    public GameObject start;
    public float speed = -30f;
    private bool isMoving = false;
    private bool hasStoppedInPhase = false; 
    private float radius;
    public Transform pointTransform;
    public GameObject pointer;

    private int score = 0;
    private int phase1Score = 0, phase2Score = 0, phase3Score = 0;
    private HashSet<string> scoredSections = new HashSet<string>();

    void Start()
    {
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
        currentPhase = Phase.Phase1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !hasStoppedInPhase)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            Debug.DrawLine(wheelCenter.position, pointTransform.position, Color.red);

            float angle = speed * Time.deltaTime;
            transform.RotateAround(wheelCenter.position, Vector3.forward, angle);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

            transform.position = wheelCenter.position + (transform.position - wheelCenter.position).normalized * radius;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Phase1Start"))
        {
            ChangePhase(Phase.Phase1);
        }
        else if (collision.CompareTag("Phase2Start"))
        {
            ChangePhase(Phase.Phase2);
        }
        else if (collision.CompareTag("Phase3Start"))
        {
            ChangePhase(Phase.Phase3);
        }
        //else if (collision.CompareTag("End"))
        //{
        //}
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!isMoving && !scoredSections.Contains(collision.name))
        {
            Debug.Log("Colliding with " + collision.tag);

            if (collision.CompareTag("Perfect"))
            {
                score += 10;
                scoredSections.Add(collision.name);
                resultText.text += "\nThe pointer stopped on a Perfect region! Score: " + score.ToString();

                UpdatePhaseScore(10);
                return;
            }

            if (collision.CompareTag("Good"))
            {
                score += 5;
                scoredSections.Add(collision.name);
                resultText.text += "\nThe pointer stopped on a Good region! Score: " + score.ToString();

                UpdatePhaseScore(5);
                return;
            }

            if (collision.CompareTag("Bad"))
            {
                score += 2;
                scoredSections.Add(collision.name);
                resultText.text += "\nThe pointer stopped on a Bad region! Score: " + score.ToString();

                UpdatePhaseScore(2);
                return;
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Phase1Region"))
        {
            ChangePhase(Phase.Phase2);
        }
        else if (collision.CompareTag("Phase2Region"))
        {
            ChangePhase(Phase.Phase3);
        }
        else if (collision.CompareTag("Phase3Region"))
        {

            ChangePhase(Phase.Phase1);
        }
    }

    void UpdatePhaseScore(int points)
    {
        switch (currentPhase)
        {
            case Phase.Phase1:
                phase1Score += points;
                phase1ScoreText.text = "Phase 1 score: " + phase1Score.ToString();
                break;
            case Phase.Phase2:
                phase2Score += points;
                phase2ScoreText.text = "Phase 2 score: " + phase2Score.ToString();
                break;
            case Phase.Phase3:
                phase3Score += points;
                phase3ScoreText.text = "Phase 3 score: " + phase3Score.ToString();
                break;
        }
    }

    // Function to change phase and reset scoredSections and hasStoppedInPhase
    public void ChangePhase(Phase newPhase)
    {
        currentPhase = newPhase;
        scoredSections.Clear();
        hasStoppedInPhase = false;
    }
}


