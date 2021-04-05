using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Wander,
    Seek,
    RangedAtk,
}

public class Enemy : Character
{
    [SerializeField] protected float idleDuration; //Amount of time enemy should idle
    [SerializeField] protected float wanderDuration; //Amount of time enemy should wander

    protected bool keepHoldingState; //Whether to move to new state

    protected LayerMask obstacleLayer;

    protected EnemyState state;
    protected EnemyState prevState;
    protected EnemyState startState;
    protected DirectionState dirState;
    protected DirectionState prevDirState;
    protected DirectionState startDirState;

    protected Vector2 startPos;

    protected float halfWidthX;
    protected float halfWidthY;

    [SerializeField] protected List<GameObject> pickups;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        state = EnemyState.Idle;
        prevState = state;
        startState = state;
        dirState = DirectionState.Down;
        prevDirState = dirState;
        startDirState = dirState;

        startPos = transform.position;

        halfWidthX = boxCollider.size.x / 2 * transform.localScale.x;
        halfWidthY = boxCollider.size.y / 2 * transform.localScale.y;

        obstacleLayer = LayerMask.GetMask("Obstacle");

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

                    break;


                //Enemy is moving
                case EnemyState.Wander:
                    if (state != prevState)
                    {
                        StartCoroutine(HoldCurrentState(wanderDuration)); //Determines when enemy should exit state
                        SetRandomDirection(); //Sets a new random direction                          
                    }

                    if (FacingWall(halfWidthX + 0.05f, halfWidthY / 2 + 0.05f)) //Prevents enemy from walking into walls
                        SetRandomDirection(); //Ensures new direction does not have wall

                    Move(walkSpeed);

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.Wander);
                    else
                    {
                        int value = GenerateWeightedRandom(new int[] { 2, 1 });
                        if (value == 0)
                        {
                            ChangeEnemyState(EnemyState.Idle);
                            ChangeEnemyState(EnemyState.Wander);
                        }
                        else
                            ChangeEnemyState(EnemyState.Idle);
                    }
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
            animator.Play("Amdusias_Walk_Up");
        else if (dirState == DirectionState.Down)
            animator.Play("Amdusias_Walk_Down");
    }


    //------------------------Changing State------------------------
    protected void ChangeEnemyState(EnemyState newState) //Changes current player state and records previous one
    {
        prevState = state; //Updates previous state
        state = newState; //Updates current state
    }

    protected void ChangeDirectionState(DirectionState newState) //Changes current direction state and records previous one
    {
        prevDirState = dirState; //Updates previous state
        dirState = newState; //Updates current state
    }

    protected IEnumerator HoldCurrentState(float duration) //Keeps player in current state for set time
    {
        EnemyState startState = state;
        keepHoldingState = true;
        float time = 0; //Time since attack started
        while (time < duration)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;

            if (state != startState) //Exits if state is changed outside of iterator
                yield break;
        }
        keepHoldingState = false;
    }

    //------------------------Basic Controls------------------------
    protected override void Move(float speed) //Moves the player horizontally
    {
        if (dirState == DirectionState.Up) //Player walks Up
            rigidBody.velocity = speed * Vector2.up;
        else if (dirState == DirectionState.Down) //Player walks Down
            rigidBody.velocity = speed * Vector2.down;
        else if (dirState == DirectionState.Left) //Player walks Left
            rigidBody.velocity = speed * Vector2.left;
        else if (dirState == DirectionState.Right) //Player walks Right
            rigidBody.velocity = speed * Vector2.right;
    }

    //------------------------Collision Handling------------------------
    protected virtual void OnCollisionEnter2D(Collision2D collision) //Handles collisions between enemy and physical GameObjects
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
        }
        /*else if (other.tag == "Enemy") //Enemy touches another enemy
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
        }*/
    }

    protected virtual void OnCollisionStay2D(Collision2D collision) //Handles collisions between enemy and physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (other.tag == "Enemy") //Enemy touches another enemy
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
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision) //Handles collisions between enemy and non-physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (other.tag == "PlayerProjectile" && canTakeDamage) //Player projectile hits enemy
        {
            Projectile projScript = other.GetComponent<Projectile>();
            if (projScript)
            {
                TakeDamage(projScript.Damage, collisionDirection); //Gives enemy damage and knockback
                Destroy(other);
            }
        }
        else if (other.tag == "PlayerWeapon" && canTakeDamage) //Player weapon hits enemy
        {
                Weapon wpnScript = other.GetComponent<Weapon>();
                if (wpnScript)
                {
                    TakeDamage(wpnScript.Damage, collisionDirection); //Gives enemy damage and knockback
                }
            }
    }

    //------------------------Other------------------------
    protected void SetRandomDirection()
    {
        int rand = Random.Range(0, 4); //Sets random direction to move
        if (rand == 0)
            ChangeDirectionState(DirectionState.Up);
        else if (rand == 1)
            ChangeDirectionState(DirectionState.Down);
        else if (rand == 2)
            ChangeDirectionState(DirectionState.Left);
        else
            ChangeDirectionState(DirectionState.Right);
    }

    protected bool FacingWall(float xDist, float yDist) //Returns whether enemy is facing a wall within a certain range
    {
        List<Vector2> wallDirections = GetObstacleCollisionDirections(xDist, yDist); //Prevents enemy from walking into walls
        return (dirState == DirectionState.Up && wallDirections.Contains(Vector2.up)) ||
            (dirState == DirectionState.Down && wallDirections.Contains(Vector2.down)) ||
            (dirState == DirectionState.Left && wallDirections.Contains(Vector2.left)) ||
            (dirState == DirectionState.Right && wallDirections.Contains(Vector2.right));
    }

    protected void SpawnPickup() //Spawns an item at the Enemy's position
    {
        if (pickups.Count >= 2)
        {
            int[] weights = { 3, 3, 14 };
            int index = GenerateWeightedRandom(weights);
            if (index == 0 && pickups[0])
                Instantiate(pickups[0], transform.position, Quaternion.identity); //Creates pickup type 1
            else if (index == 1 && pickups[1])
                Instantiate(pickups[1], transform.position, Quaternion.identity); //Creates pickup type 2
        }
    }

    public void Reset() //Resets Enemy's position, state, and health
    {
        state = startState;
        dirState = startDirState;
        health = maxHealth;
        transform.position = startPos;
    }

    //------------------------Utility------------------------
    protected Vector2 GetNearestAxis2D(Vector2 vector) //Returns the closest axis to the specified vector
    {
        vector = vector.normalized;

        if (vector.x > 0 && Mathf.Abs(vector.x) >= Mathf.Abs(vector.y)) //X-direction is positive and x-magnitude is greater than y-direction
            return Vector2.right;
        else if (vector.x < 0 && Mathf.Abs(vector.x) >= Mathf.Abs(vector.y)) //X-direction is negative and x-magnitude is greater or equal to than y-magnitude
            return Vector2.left;
        if (vector.y > 0 && Mathf.Abs(vector.y) > Mathf.Abs(vector.x)) //Y-direction is positive and y-magnitude is greater than x-magnitude
            return Vector2.up;
        else if (vector.y < 0 && Mathf.Abs(vector.y) > Mathf.Abs(vector.x)) //Y-direction is negative and y-magnitude is greater than x-magnitude
            return Vector2.down;
        else
            return Vector2.zero;
    }

    protected List<Vector2> GetObstacleCollisionDirections(float xDist, float yDist) //Returns which directions near the enemy have a wall
    {
        List<Vector2> directions = new List<Vector2>();

        Vector2 topPos = transform.position + new Vector3(0, halfWidthY, 0); //Starting points for edges
        Vector2 btmPos = transform.position - new Vector3(0, halfWidthY, 0);
        Vector2 leftPos = transform.position - new Vector3(halfWidthY, 0, 0);
        Vector2 rightPos = transform.position + new Vector3(halfWidthY, 0, 0);

        //Wall Above Enemy
        if (Physics2D.Raycast(leftPos, Vector2.up, yDist, obstacleLayer).collider != null ||
            Physics2D.Raycast(rightPos, Vector2.up, yDist, obstacleLayer).collider != null)
        {
            directions.Add(Vector2.up);
        }

        //Wall Below Enemy
        if (Physics2D.Raycast(leftPos, Vector2.down, yDist, obstacleLayer).collider != null ||
            Physics2D.Raycast(rightPos, Vector2.down, yDist, obstacleLayer).collider != null)
        {
            directions.Add(Vector2.down);
        }

        //Wall to Left of Enemy
        if (Physics2D.Raycast(topPos, Vector2.left, xDist, obstacleLayer).collider != null ||
            Physics2D.Raycast(btmPos, Vector2.left, xDist, obstacleLayer).collider != null)
        {
            directions.Add(Vector2.left);
        }

        //Wall to Right of Enemy
        if (Physics2D.Raycast(topPos, Vector2.right, xDist, obstacleLayer).collider != null ||
            Physics2D.Raycast(btmPos, Vector2.right, xDist, obstacleLayer).collider != null)
        {
            directions.Add(Vector2.right);
        }

        return directions;
    }

    protected int GenerateWeightedRandom(int[] weights) //Returns the index of the weight that was generated
    {
        int total = 0;
        for (int i = 0; i < weights.Length; i++) //Generates max value from weights
        {
            total += weights[i];
        }

        int rand = Random.Range(0, total);
        int current = 0;
        for (int i = 0; i < weights.Length; i++) //Determines which range random value was in
        {
            if (rand >= current && rand < current + weights[i]) //Return if value matches ranges
                return i;

            current += weights[i]; //Update current value
        }

        return 0;
    }
}
