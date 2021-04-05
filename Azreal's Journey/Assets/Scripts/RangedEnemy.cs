using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] protected float attackDuration; //Amount of time until player can attack again
    [SerializeField] protected float rangedSpeed; //Projectile Speed
    [SerializeField] protected GameObject projectilePrefab; //Reference to prefab for shots

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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

                    if (FacingWall(halfWidthX + 0.05f, halfWidthX + 0.05f)) //Prevents enemy from walking into walls
                        SetRandomDirection(); //Ensures new direction does not have wall

                    Move(walkSpeed);

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.Wander);
                    else
                    {
                        int value = GenerateWeightedRandom(new int[] {4, 1});
                        if (value == 0)
                            ChangeEnemyState(EnemyState.RangedAtk);
                        else
                            ChangeEnemyState(EnemyState.Idle);
                    }
                    break;

                //Enemy is using ranged attack
                case EnemyState.RangedAtk:
                    if (state != prevState)
                    {
                        RangedAttack();
                        StartCoroutine(HoldCurrentState(attackDuration)); //Determines when enemy should exit state
                    }

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.RangedAtk);
                    else
                        ChangeEnemyState(EnemyState.Wander);

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
            animator.Play("Aim_Walk_Up");
        else if (dirState == DirectionState.Down)
            animator.Play("Aim_Walk_Down");
        else if (dirState == DirectionState.Left)
            animator.Play("Aim_Walk_Left");
        else if (dirState == DirectionState.Right)
            animator.Play("Aim_Walk_Right");

    }

    protected virtual void RangedAttack() //Ranged Attack
    {
        Vector3 pos = gameObject.transform.position;

        pos.z = gameObject.transform.position.z + 1;

        GameObject projectile = Instantiate(projectilePrefab, pos, Quaternion.identity); //Creates a projectile
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript)
        {
            projScript.Damage = damage; //Set shot damage
            projScript.Speed = rangedSpeed;

            if (dirState == DirectionState.Up) //Projectile shoots Up
            {
                projScript.transform.Rotate(new Vector3(0, 0, 0));
                projScript.Direction = Vector2.up;
            }
            else if (dirState == DirectionState.Down) //Projectile shoots Down
            {
                projScript.transform.Rotate(new Vector3(0, 0, 180));
                projScript.Direction = Vector2.down;
            }
            else if (dirState == DirectionState.Left) //Projectile shoots Left
            {
                projScript.transform.Rotate(new Vector3(0, 0, 90));
                projScript.Direction = Vector2.left;
            }
            else if (dirState == DirectionState.Right) //Projectile shoots Right
            {
                projScript.transform.Rotate(new Vector3(0, 0, 270));
                projScript.Direction = Vector2.right;
            }
        }
    }
}
