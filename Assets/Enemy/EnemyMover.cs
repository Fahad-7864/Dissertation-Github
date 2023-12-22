using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
/*
The EnemyMover class manages the movement logic for enemy characters in the game.
The class facilitates movement calculation, animation direction updates, and pathfinding execution
based on A* algorithm and other related mechanics.
*/

public class EnemyMover : MonoBehaviour
{

    // ----- Section: Variables and References -----
    public Astar astar;
    public Tilemap tilemap;
    public GameObject ai;
    public Animator animator;
    public float moveSpeed = 3f;
    public CharacterStats characterStats;
    public CentreCharacter centreCharacter;
    private AISituationGrabber situationGrabber; 
    private HighlightandMovement highlightandmovement;
    public ChatboxController chatboxController;

    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;




    // Gathers and checks required components for the enemy mover.
    private void Start()
    {
        // Get the HighlightAndMovement component attached to the same GameObject
        highlightandmovement = GetComponent<HighlightandMovement>();

        if (highlightandmovement == null)
        {
            Debug.LogError("HighlightAndMovement component not found on this GameObject");
        }

        // Obtain the CharacterStats directly from the ai GameObject.
        characterStats = ai.GetComponent<CharacterStats>();
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats component not found on ai GameObject");
        }

        animator = ai.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on ai GameObject");
        }

        // Initialize situationGrabber
        situationGrabber = ai.GetComponent<AISituationGrabber>();
        if (situationGrabber == null)
        {
            Debug.LogError("AISituationGrabber not found on ai GameObject");
        }
        // Setup the button listeners
        upButton.onClick.AddListener(() => SetFacingDirection(Direction.Up));
        downButton.onClick.AddListener(() => SetFacingDirection(Direction.Down));
        leftButton.onClick.AddListener(() => SetFacingDirection(Direction.Left));
        rightButton.onClick.AddListener(() => SetFacingDirection(Direction.Right));
    }

    // Method to handle button clicks and set the character's facing direction
    private void SetFacingDirection(Direction direction)
    {
        if (characterStats != null)
        {
            characterStats.facingDirection = direction;
            UpdateFacingDirectionAnimation();
        }
    }


    // ----- Section: Update & Input Handling -----
    private void Update()
    {

        UpdateFacingDirectionAnimation();

        animator = ai.GetComponent<Animator>();

        if (ai == null)
        {
            Debug.LogError("AI GameObject is not assigned.");
            return;
        }

       


        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Moving to best tile");

            MoveToBestTile();
        }

    }


    // Updates the animation based on the character's facing direction.
    void UpdateFacingDirectionAnimation()
    {
        if (characterStats == null || animator == null) return;

        switch (characterStats.facingDirection)
        {
            case Direction.Up:
                animator.Play("Up");
                break;
            case Direction.Down:
                animator.Play("Down");
                break;
            case Direction.Left:
                animator.Play("Left");
                break;
            case Direction.Right:
                animator.Play("Right");
                break;
        }
    }


    // ----- Section: Movement Calculation & Execution -----
    public void CalculateAndMoveToBestTile()
    {
        StartCoroutine(CalculateAndMoveToBestTileCoroutine());
    }

    // Calculates the best tile for attack and then moves to it.
    private IEnumerator CalculateAndMoveToBestTileCoroutine()
    {
        yield return StartCoroutine(situationGrabber.CalculateBestSituationCoroutineForAttack());
        MoveToBestTile();
    }

    


    // Moves the AI character to the best tile determined.
    public void MoveToBestTile()
    {
        Debug.Log("MoveToBestTile function has been called");

        Vector3Int aiPos = tilemap.WorldToCell(ai.transform.position);
      //  AISituationGrabber situationGrabber = ai.GetComponent<AISituationGrabber>();
        if (situationGrabber == null)
        {
            Debug.LogError("No AISituationGrabber attached to AI GameObject.");
            return;
        }
        Vector3Int targetPos = situationGrabber.BestTile;

        Debug.Log("AI position: " + aiPos);
        Debug.Log("Target position: " + targetPos);

        List<Vector3Int> path = astar.FindPath(aiPos, targetPos);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Path is null or path count is 0");
            return;
        }

        // Print the best path in Debug.Log
        Debug.Log("Best path: " + string.Join(" -> ", path));

        if (path.Count == 1)
        {
            Debug.Log("Path count is 1, moving directly to target position");
            ai.transform.position = tilemap.CellToWorld(path[0]) + new Vector3(0.5f, 0.5f, 0); 
            return;
        }

        StartCoroutine(MoveAlongPath(path));
        Debug.Log("Started coroutine MoveAlongPath");

        // Call OnEndTurnButtonClick after movement is done
        TurnManager turnManager = FindObjectOfType<TurnManager>(); 
        if (turnManager != null)
        {
            turnManager.OnEndTurnButtonClick();
        }
        else
        {
            Debug.LogError("TurnManager not found in the scene.");
        }
    }





    private void SetAnimation(Vector3 direction)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null in SetAnimation");
            return;
        }

        if (characterStats == null)
        {
            Debug.LogError("CharacterStats is null in SetAnimation");
            return;
        }

        if (direction.x > 0)
        {
            animator.Play("Up");
            characterStats.facingDirection = Direction.Up;
        }
        else if (direction.x < 0)
        {
            animator.Play("Down");
            characterStats.facingDirection = Direction.Down;
        }
        else if (direction.y > 0)
        {
            animator.Play("Left");
            characterStats.facingDirection = Direction.Left;
        }
        else if (direction.y < 0)
        {
            animator.Play("Right");
            characterStats.facingDirection = Direction.Right;
        }
    }


    public int moveRange = 5;

    // Animates the movement of the AI character along the determined path.
    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        Debug.Log("MoveAlongPath function has been called");

        // Limit the length of the path to the move range
        int pathLength = Mathf.Min(path.Count, moveRange);

        for (int i = 0; i < pathLength; i++)
        {
            Vector3 startWorldPos = ai.transform.position;
            Vector3 targetWorldPos = tilemap.CellToWorld(path[i]) + new Vector3(0.5f, 0.5f, 0); 
            Vector3 direction = (targetWorldPos - startWorldPos).normalized;
            //SetAnimation(direction);

            float journeyTime = Vector3.Distance(startWorldPos, targetWorldPos) / moveSpeed;
            float elapsedTime = 0;

            while (elapsedTime < journeyTime)
            {
                float fractionOfJourney = (elapsedTime / journeyTime);
                ai.transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, fractionOfJourney);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Snap to the tile's center to make sure the GameObject is never in-between tiles
            ai.transform.position = targetWorldPos;


        }

        // When movement is over, check if the character is in target tile
        if (IsInTargetTile(path[pathLength - 1]))
        {
            // Once the AI has reached the target tile, make sure it is exactly at the center
            centreCharacter.CenterCharacterOnTile(ai.transform);  
            Debug.Log("AI has reached the target tile and is now at the center");
            highlightandmovement?.ClearHighlightedTiles();
        }
        else
        {
            Debug.LogError("AI failed to reach the target tile");
        }
    }


    // Checks if the AI character is in the target tile.
    private bool IsInTargetTile(Vector3Int targetTile)
    {
        Vector3Int currentTile = tilemap.WorldToCell(ai.transform.position);
        return currentTile == targetTile;
    }





}