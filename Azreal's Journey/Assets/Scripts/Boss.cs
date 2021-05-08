using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : RangedEnemy
{
    private GameObject player;
    [SerializeField] private float shotSpreadAngle;
    [SerializeField] private float speedModifier;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        player = GameObject.Find("Player");
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

                    if (FacingWall(halfWidthX + 0.05f, halfWidthY + 0.05f)) //Prevents enemy from walking into walls
                        SetRandomDirection(); //Ensures new direction does not have wall

                    if (health <= MaxHealth / 2.0f) //Boss becomes more aggressive when at half health or less
                        Move(walkSpeed * speedModifier);
                    else
                        Move(walkSpeed);

                    if (keepHoldingState)
                        ChangeEnemyState(EnemyState.Wander);
                    else
                    {
                        int value;
                        if (health <= MaxHealth / 2.0f) //Boss becomes more aggressive when at half health or less
                            value = GenerateWeightedRandom(new int[] { 3, 1 });
                        else
                            value = GenerateWeightedRandom(new int[] { 1, 1 });

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
                        StartCoroutine(HoldCurrentState(idleDuration)); //Determines when enemy should exit state
                        RangedAttack();
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
            Destroy(gameObject);
        }
    }

    protected override void UpdateAnimation() //Updates animation if state changes
    {
        if (state == EnemyState.Idle || state == EnemyState.RangedAtk)
            animator.Play("Zagan_Idle");
        else if (state == EnemyState.Wander)
            animator.Play("Zagan_Walk");
    }

    protected override void RangedAttack() //Ranged Attack
    {
        PlayAudio("event:/SFX/Enemy/Boss/Boss_Shoot");
        Vector3 pos = gameObject.transform.position;

        pos.z = gameObject.transform.position.z - 1;

        Vector2 playerDirection = (player.transform.position - transform.position).normalized;
        float playerAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        Vector2 leftDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (playerAngle - shotSpreadAngle)), Mathf.Sin(Mathf.Deg2Rad * (playerAngle - shotSpreadAngle)));
        Vector2 rightDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (playerAngle + shotSpreadAngle)), Mathf.Sin(Mathf.Deg2Rad * (playerAngle + shotSpreadAngle)));

        GameObject projectile0 = Instantiate(projectilePrefab, pos, Quaternion.identity); //Creates a projectile
        Projectile projScript0 = projectile0.GetComponent<Projectile>();

        GameObject projectile1 = Instantiate(projectilePrefab, pos, Quaternion.identity); //Creates a projectile
        Projectile projScript1 = projectile1.GetComponent<Projectile>();

        GameObject projectile2 = Instantiate(projectilePrefab, pos, Quaternion.identity); //Creates a projectile
        Projectile projScript2 = projectile2.GetComponent<Projectile>();
        if (projScript0)
        {
            projScript0.Damage = damage; //Set shot damage
            projScript0.Speed = rangedSpeed;
            projScript0.Direction = playerDirection;
        }
        if (projScript1)
        {
            projScript1.Damage = damage; //Set shot damage
            projScript1.Speed = rangedSpeed;
            projScript1.Direction = leftDirection;
        }
        if (projScript2)
        {
            projScript2.Damage = damage; //Set shot damage
            projScript2.Speed = rangedSpeed;
            projScript2.Direction = rightDirection;
        }

    }
}
