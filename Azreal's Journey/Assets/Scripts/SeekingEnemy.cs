using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekingEnemy : Enemy
{
    [SerializeField] protected float seekSpeed; //Speed while seeking player
    [SerializeField] protected float seekDist; //Distance Enemy will check for player
    protected LayerMask playerLayer; //Layer of the player used for Raycasting

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerLayer = LayerMask.GetMask("Player");
        UpdateAnimation(); //Starts character animation
    }

    // Update is called once per frame
    protected override void Update()
    {
        rigidBody.velocity = new Vector2(); //Resets movement
        if (!paused)
        {
            switch (state)
            {
                //Enemy is not moving
                case EnemyState.Idle:
                    if (state != prevState)
                        StartCoroutine(HoldCurrentState(idleDuration)); //Determines when enemy should exit state

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.Idle);
                    else
                        ChangeEnemyState(EnemyState.Wander);

                    if (SeePlayerAhead()) //Enemy chases after player if in front of enemy
                        ChangeEnemyState(EnemyState.Seek);
                    break;


                //Enemy is moving
                case EnemyState.Wander:
                    if (state != prevState)
                    {
                        StartCoroutine(HoldCurrentState(wanderDuration)); //Determines when enemy should exit state

                        if (prevState != EnemyState.Seek)
                            SetRandomDirection(); //Sets a new random direction                          
                    }

                    if (FacingWall(halfWidthX + 0.05f, halfWidthY + 0.05f)) //Prevents enemy from walking into walls
                        SetRandomDirection(); //Ensures new direction does not have wall

                    Move(walkSpeed);

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.Wander);
                    else
                    {
                        int value = GenerateWeightedRandom(new int[] { 3, 1 });
                        if (value == 0)
                            ChangeEnemyState(EnemyState.Wander);
                        else
                            ChangeEnemyState(EnemyState.Idle);
                    }

                    if (SeePlayerAhead()) //Enemy chases after player if in front of enemy
                        ChangeEnemyState(EnemyState.Seek);

                    break;

                //Enemy is moving in cardinal direction after seeing player
                case EnemyState.Seek:

                    Move(seekSpeed);

                    if (FacingWall(halfWidthX + 0.05f, halfWidthY / 2 + 0.05f)) //Stops seeking once enemy hits wall
                        ChangeEnemyState(EnemyState.Wander);
                    else
                        ChangeEnemyState(EnemyState.Seek);

                    break;
            }
        }

        if (prevState != state || prevDirState != dirState)
            UpdateAnimation();

        if (isDead) //Enemy is destroyed when defeated
        {
            SpawnPickup();
            Destroy(gameObject);
        }
    }

    protected override void UpdateAnimation() //Updates animation if state changes
    {
        if (dirState == DirectionState.Up)
            animator.Play("Furfur_Walk_Up");
        else if (dirState == DirectionState.Down)
            animator.Play("Furfur_Walk_Down");
        else if (dirState == DirectionState.Left)
            animator.Play("Furfur_Walk_Left");
        else if (dirState == DirectionState.Right)
            animator.Play("Furfur_Walk_Right");
    }

    //------------------------Collision Handling------------------------
    protected override void OnCollisionEnter2D(Collision2D collision) //Handles collisions between player and physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (other.layer == LayerMask.NameToLayer("Obstacle")) //Enemy runs into wall
        {
            List<Vector2> wallDirections = GetObstacleCollisionDirections(halfWidthX + 0.05f, halfWidthY + 0.05f);
            SetRandomDirection(); //Sets a new random direction
            while ((dirState == DirectionState.Up && wallDirections.Contains(Vector2.up)) ||
                (dirState == DirectionState.Down && wallDirections.Contains(Vector2.down)) ||
                (dirState == DirectionState.Left && wallDirections.Contains(Vector2.left)) ||
                (dirState == DirectionState.Right && wallDirections.Contains(Vector2.right)))
            {
                SetRandomDirection(); //Ensures new direction does not have wall
            }

            if (state == EnemyState.Seek) //Updates state seeking
                ChangeEnemyState(EnemyState.Wander);
        }
        /*else if (other.tag == "Enemy" || other.tag == "Player") //Enemy touches another enemy or the player
        {
            Vector2 collisionAxis = GetNearestAxis2D(-collisionDirection);

            if (dirState == DirectionState.Up && collisionAxis == Vector2.up) //Enemy walks away from other enemy
                ChangeDirectionState(DirectionState.Down);
            else if (dirState == DirectionState.Down && collisionAxis == Vector2.down)
                ChangeDirectionState(DirectionState.Up);
            else if (dirState == DirectionState.Left && collisionAxis == Vector2.left)
                ChangeDirectionState(DirectionState.Right);
            else if (dirState == DirectionState.Right && collisionAxis == Vector2.right)
                ChangeDirectionState(DirectionState.Left);

            if (state == EnemyState.Seek) //Updates state if seeking
                ChangeEnemyState(EnemyState.Wander);
        }*/
    }

    protected override void OnCollisionStay2D(Collision2D collision) //Handles collisions between player and physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (other.tag == "Enemy" || other.tag == "Player") //Enemy touches another enemy or the player
        {
            Vector2 collisionAxis = GetNearestAxis2D(-collisionDirection);

            if (dirState == DirectionState.Up && collisionAxis == Vector2.up) //Enemy walks away from other enemy
                ChangeDirectionState(DirectionState.Down);
            else if (dirState == DirectionState.Down && collisionAxis == Vector2.down)
                ChangeDirectionState(DirectionState.Up);
            else if (dirState == DirectionState.Left && collisionAxis == Vector2.left)
                ChangeDirectionState(DirectionState.Right);
            else if (dirState == DirectionState.Right && collisionAxis == Vector2.right)
                ChangeDirectionState(DirectionState.Left);

            if (state == EnemyState.Seek) //Updates state if seeking
                ChangeEnemyState(EnemyState.Wander);
        }
    }

    protected bool SeePlayerAhead() //Returns whether player is with the apecified rang in fron of the enemy
    {
        Vector2 pos = transform.position; //Starting point
        Vector2 direction = Vector2.zero;
        if (dirState == DirectionState.Up) //Sets direction to check
            direction = Vector2.up;
        else if (dirState == DirectionState.Down)
            direction = Vector2.down;
        else if (dirState == DirectionState.Left)
            direction = Vector2.left;
        else if (dirState == DirectionState.Right)
            direction = Vector2.right;

        RaycastHit2D obstacleHit = Physics2D.Raycast(pos, direction, seekDist, obstacleLayer);
        RaycastHit2D playerHit = Physics2D.Raycast(pos, direction, seekDist, playerLayer);

        if (obstacleHit.collider == null && playerHit.collider != null) //Player is in front of enemy
            return true;
        else if (obstacleHit.collider != null && playerHit.collider != null) //Player is in front of obstacle
        {
            if (Vector2.Distance(transform.position, obstacleHit.point) >= Vector2.Distance(transform.position, playerHit.point))
                return true;
        }
        return false; //Player is not in front of enemy
    }
}
