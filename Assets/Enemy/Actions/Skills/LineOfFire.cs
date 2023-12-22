using UnityEngine;

public class LineOfFire : LineOfArrows
{
    public AnimationClip fireAnimationClip;

    void Start()
    {
        elementType = ElementType.Fire;
        base.Start();
    }

    // Overriding the Attack method to use the fire animation instead of the arrow animation
    public override void Attack(Vector3Int targetTilePos)
    {
        // Define spellRange

        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(character.transform.position);
        Vector3 directionFloat = ((Vector3)targetTilePos - (Vector3)startTilePos).normalized;
        Vector3Int direction = new Vector3Int(Mathf.RoundToInt(directionFloat.x), Mathf.RoundToInt(directionFloat.y), 0);

        bool enemyHit = false;

        for (int i = 1; i <= spellRange && !enemyHit; i++)
        {
            Vector3Int tilePos = startTilePos + direction * i;

            if (pathfinding.IsWalkable(tilePos))
            {
                // Perform a raycast to check if there is an enemy at this tile
                RaycastHit2D hit = Physics2D.Raycast(lineOfArrowsTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.CompareTag("Enemy"))
                    {
                        attack.elementType = elementType;

                        // Check if the attack is successful
                        bool attackSuccessful = attack.AttackEnemy(hitObject.name);
                        if (attackSuccessful)
                        {
                            enemyHit = true;

                            // Play the fire animation
                            Animator animator = character.GetComponent<Animator>();
                            animator.Play(fireAnimationClip.name);
                        }
                    }
                }
            }
        }
    }

}
