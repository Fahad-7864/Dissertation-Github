using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class newversionofwheel: MonoBehaviour
{
    public Transform wheelCenter;
    public float speed = -30f; 
    private bool isMoving = false;
    private float radius;
    public TextMeshProUGUI resultText;
    public Transform pointTransform;
    private string targetRegion = "";
    public Image whiteoutScreen;  

    //private Dictionary<int, (int, int, int)> diceOutcomes = new Dictionary<int, (int, int, int)>
    //{
    ////Good,Bad,Perfect
    //    {1, (60, 35, 5)},
    //    {2, (50, 40, 10)},
    //    {3, (40, 40, 20)},
    //    {4, (30, 50, 20)},
    //    {5, (20, 40, 40)},
    //    {6, (10, 50, 40)}
    //};


    private Dictionary<int, (int, int, int)> diceOutcomes = new Dictionary<int, (int, int, int)>
    {
    //Good,Bad,Perfect
        {1, (90, 35, 5)},
        {2, (80, 10, 10)},
        {3, (70, 10, 20)},
        {4, (60, 30, 10)},
        {5, (60, 35, 5)},
        {6, (60, 5, 40)}
    };


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
        RollDice();
        isMoving = true;
    }

    void Update()
    {
        Debug.DrawLine(wheelCenter.position, pointTransform.position, Color.red);

        if (isMoving)
        {
            float angle = speed * Time.deltaTime;
            transform.RotateAround(wheelCenter.position, Vector3.forward, angle);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

            // This line sets the position of the object to always be `radius` units away from `wheelCenter`
            transform.position = wheelCenter.position + (transform.position - wheelCenter.position).normalized * radius;
        }
    }




    void RollDice()
    {
        int diceRoll = UnityEngine.Random.Range(1, 7);
        var outcome = diceOutcomes[diceRoll];
        int randomResult = UnityEngine.Random.Range(1, 101);

        if (randomResult <= outcome.Item1)
        {
            targetRegion = "Good";
            Debug.Log("Dice roll: " + diceRoll + ". Random result: " + randomResult + ". Target region: " + targetRegion + " (probability range 1-" + outcome.Item1 + ")");
        }
        else if (randomResult <= outcome.Item1 + outcome.Item2)
        {
            targetRegion = "Bad";
            Debug.Log("Dice roll: " + diceRoll + ". Random result: " + randomResult + ". Target region: " + targetRegion + " (probability range " + (outcome.Item1 + 1) + "-" + (outcome.Item1 + outcome.Item2) + ")");
        }
        else
        {
            targetRegion = "Perfect";
            Debug.Log("Dice roll: " + diceRoll + ". Random result: " + randomResult + ". Target region: " + targetRegion + " (probability range " + (outcome.Item1 + outcome.Item2 + 1) + "-100)");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == targetRegion)
        {
            resultText.text = "The pointer stopped on a " + targetRegion + " region!";
            StartCoroutine(StopAtRegionCoroutine(1f));  // start the coroutine with a delay of 1 second
        }
    }

    IEnumerator StopAtRegionCoroutine(float delay)
    {
        // First, we stop the clock hand and show the whiteout screen.
        isMoving = false;
        whiteoutScreen.enabled = true;

        // We wait for the specified amount of seconds.
        yield return new WaitForSeconds(delay);

        // Then, we hide the whiteout screen, roll the dice again and start the clock hand.
        whiteoutScreen.enabled = false;
        RollDice();
        isMoving = true;
    }


    void CheckPosition()
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(pointTransform.position);
        Debug.Log("CheckPosition called. Found " + colliders.Length + " colliders.");

        foreach (Collider2D collider in colliders)
        {
            Debug.Log("Found collider with tag " + collider.tag);

            // If the clock hand has stopped at the target region, start the coroutine with a delay of 1 second.
            if (collider.tag == targetRegion)
            {
                resultText.text = "The pointer stopped on a " + targetRegion + " region!";
                StartCoroutine(StopAtRegionCoroutine(1f));
                return;
            }
        }
    }
}
