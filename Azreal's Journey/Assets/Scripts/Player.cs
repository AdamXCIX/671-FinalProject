using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public enum PlayerState
{
    Idle,
    Walk,
    MeleeAtk,
    RangedAtk,
    ChargeAtk
}

public enum DirectionState
{
    Up,
    Down,
    Left,
    Right
}
public class Player : Character
{
    //For spritesheet animations
    //http://www.strandedsoft.com/using-spritesheets-with-unity3d/

    protected float ammo; //Player's current ranged ammo
    [SerializeField] protected int maxAmmo; //Player's maximum ranged ammo
    protected float keys; //Player's current key amount
    [SerializeField] protected int maxKeys; //Player's maximum number of keys

    [SerializeField] protected float attackDelay; //Amount of time attack lasts
    [SerializeField] protected float attackDuration; //Amount of time until player can attack again
    [SerializeField] protected float rangedSpeed; //Projectile Speed
    [SerializeField] protected float chargeDuration; //Amount of time melee button is held before charge attack is ready
    protected float chargeElapsed; //Amount of time since player started charging

    [SerializeField] protected GameObject weaponPrefab; //Reference to prefab for weapon
    [SerializeField] protected GameObject projectilePrefab; //Reference to prefab for shots
    [SerializeField] protected GameObject strongWeaponPrefab; //Reference to prefab for charge attack weapon

    protected bool charging;
    protected bool charged;
    protected bool canAttack;
    protected bool keepHoldingState; //Whether to move to new state
    protected bool hasWon;

    protected PlayerState state;
    protected PlayerState prevState;
    protected DirectionState dirState;
    protected DirectionState prevDirState;
    protected bool prevPaused;


    //Audio Variables
    private EventInstance lowHealthSound;
    private EventInstance footstepSound;
    [FMODUnity.EventRef]
    private PARAMETER_ID lowHealthParamId;

    public float Ammo
    {
        get { return ammo; }
    }

    public float Keys
    {
        get { return keys; }
    }

    public bool HasWon
    {
        get { return hasWon; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        hasWon = false;
        canAttack = true;
        charging = false;
        canTakeDamage = true;
        ammo = maxAmmo;

        state = PlayerState.Idle;
        prevState = state;
        dirState = DirectionState.Down;
        prevDirState = dirState;

        UpdateAnimation(); //Starts character animation

        //Sets Player's low health parameter and starts event
        lowHealthSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interface/Player_LowHealth");

        PARAMETER_DESCRIPTION lowHealthParamDesc;
        FMODUnity.RuntimeManager.StudioSystem.getParameterDescriptionByName("PlayerHealth", out lowHealthParamDesc);
        lowHealthParamId = lowHealthParamDesc.id;

        footstepSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Player/Player_Footsteps");
    }

    // Update is called once per frame
    protected override void Update()
    {
        rigidBody.velocity = new Vector2(); //Resets movement

        if (!isDead && !paused && !prevPaused)
        {
            switch (state)
            {
                //Player is not moving
                case PlayerState.Idle:

                    //animator.Play("PlayerIdle");

                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) //Player Walks
                        ChangePlayerState(PlayerState.Walk);
                    else if (Input.GetKeyDown(KeyCode.K) && canAttack) //Melee Attack
                        ChangePlayerState(PlayerState.MeleeAtk);
                    else if (Input.GetKeyDown(KeyCode.J) && canAttack) //Range Attack
                        ChangePlayerState(PlayerState.RangedAtk);
                    else if (Input.GetKeyUp(KeyCode.K) && canAttack && charged) //Charge Attack
                        ChangePlayerState(PlayerState.ChargeAtk);
                    else
                        ChangePlayerState(PlayerState.Idle);
                    break;

                //Player is moving
                case PlayerState.Walk:
                    //animator.Play("PlayerWalk");
                    if (Input.GetKey(KeyCode.W)) //Player Walks Up
                        ChangeDirectionState(DirectionState.Up);
                    else if (Input.GetKey(KeyCode.S)) //Player Walks Down
                        ChangeDirectionState(DirectionState.Down);
                    else if (Input.GetKey(KeyCode.A)) //Player Walks Left
                        ChangeDirectionState(DirectionState.Left);
                    else if (Input.GetKey(KeyCode.D)) //Player Walks Right
                        ChangeDirectionState(DirectionState.Right);


                    if (Input.GetKeyDown(KeyCode.K) && canAttack) //Melee Attack
                        ChangePlayerState(PlayerState.MeleeAtk);
                    else if (Input.GetKeyDown(KeyCode.J) && canAttack) //Range Attack
                        ChangePlayerState(PlayerState.RangedAtk);
                    else if (Input.GetKeyUp(KeyCode.K) && canAttack && charged) //Charge Attack
                        ChangePlayerState(PlayerState.ChargeAtk);
                    else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) //Player Moves
                    {
                        Move(walkSpeed);
                        ChangePlayerState(PlayerState.Walk);
                    }
                        
                    else
                        ChangePlayerState(PlayerState.Idle);
                    break;

                //Player is using melee attack
                case PlayerState.MeleeAtk:
                    if (state != prevState)
                    {
                        MeleeAttack();
                        StartCoroutine(HoldCurrentState(attackDuration)); //Determines when player should exit state
                        //animator.Play("PlayerSwipe", -1, 0f);
                    }

                    if (keepHoldingState)
                        ChangePlayerState(PlayerState.MeleeAtk);
                    else
                        ChangePlayerState(PlayerState.Idle);

                    break;

                //Player is using ranged attack
                case PlayerState.RangedAtk:
                    if (state != prevState && ammo > 0)
                    {
                        ChangeAmmo(-1);
                        RangedAttack();
                        StartCoroutine(HoldCurrentState(attackDuration)); //Determines when player should exit state
                        //animator.Play("PlayerShoot", -1, 0f);
                    }
                    else if (state != prevState) //Displays "dry fire" animation
                        StartCoroutine(HoldCurrentState(attackDuration)); //Determines when player should exit state

                    if (keepHoldingState)
                        ChangePlayerState(PlayerState.RangedAtk);
                    else
                        ChangePlayerState(PlayerState.Idle);

                    break;

                //Player is using charge attack
                case PlayerState.ChargeAtk:
                    if (state != prevState)
                    {
                        charged = false;
                        ChargeAttack();
                        StartCoroutine(HoldCurrentState(attackDuration)); //Determines when player should exit state
                        //animator.Play("PlayerSwipe", -1, 0f);
                    }

                    if (keepHoldingState)
                        ChangePlayerState(PlayerState.ChargeAtk);
                    else
                        ChangePlayerState(PlayerState.Idle);

                    break;
            }

            //Handle Charge Attack
            if (!charging && Input.GetKeyDown(KeyCode.K)) 
            {
                charging = true;
                chargeElapsed = 0;
            }

            if (charging)
                Charge();

            if (prevState != state || prevDirState != dirState)
                UpdateAnimation();
        }

        prevPaused = paused;


        //Sets Audio Parameters
        FMODUnity.RuntimeManager.StudioSystem.setParameterByID(lowHealthParamId, health / maxHealth);
    }

    protected override void UpdateAnimation() //Updates animation if state changes
    {
        if (state == PlayerState.Idle) //Player is idle
        {
            if (dirState == DirectionState.Up)
                animator.Play("Azrael_Idle_Up");
            else if (dirState == DirectionState.Down)
                animator.Play("Azrael_Idle_Down");
            else if (dirState == DirectionState.Left)
                animator.Play("Azrael_Idle_Left");
            else if (dirState == DirectionState.Right)
                animator.Play("Azrael_Idle_Right");
        }
        else if (state == PlayerState.Walk) //Player is walking
        {
            if (dirState == DirectionState.Up)
                animator.Play("Azrael_Walk_Up");
            else if (dirState == DirectionState.Down)
                animator.Play("Azrael_Walk_Down");
            else if (dirState == DirectionState.Left)
                animator.Play("Azrael_Walk_Left");
            else if (dirState == DirectionState.Right)
                animator.Play("Azrael_Walk_Right");
        }
        else if (state == PlayerState.MeleeAtk || state == PlayerState.RangedAtk || state == PlayerState.ChargeAtk) //Player is attacking
        {
            if (dirState == DirectionState.Up)
                animator.Play("Azrael_Attack_Up");
            else if (dirState == DirectionState.Down)
                animator.Play("Azrael_Attack_Down");
            else if (dirState == DirectionState.Left)
                animator.Play("Azrael_Attack_Left");
            else if (dirState == DirectionState.Right)
                animator.Play("Azrael_Attack_Right");
        }
    }

    //------------------------Changing State------------------------
    protected void ChangePlayerState(PlayerState newState) //Changes current player state and records previous one
    {
        prevState = state; //Updates previous state
        state = newState; //Updates current state

            if (state == PlayerState.Walk && prevState != PlayerState.Walk) //Starts footsteps
                footstepSound.start();
            else if (state != PlayerState.Walk && prevState == PlayerState.Walk) //Stops footsteps
            footstepSound.stop(STOP_MODE.ALLOWFADEOUT);
    }

    protected void ChangeDirectionState(DirectionState newState) //Changes current direction state and records previous one
    {
        prevDirState = dirState; //Updates previous state
        dirState = newState; //Updates current state
    }

    protected IEnumerator HoldCurrentState(float duration) //Keeps player in current state for set time
    {
        PlayerState startState = state;
        keepHoldingState = true;
        float elapsed = 0; //Time since attack started
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            elapsed += Time.deltaTime;

            if (state != startState) //Exits if state is changed outside of iterator
                yield break;
        }
        keepHoldingState = false;
    }

    //------------------------Audio------------------------
    private void PlayAudio(string path) //Plays audio found at path
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }

    private bool IsAudioPlaying(EventInstance instance) //Returns whether the passed event is currently playing
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != PLAYBACK_STATE.STOPPED;
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

    protected void MeleeAttack() //Melee Attack
    {
        PlayAudio("event:/SFX/Player/Player_Attack");
        Vector3 pos = gameObject.transform.position;

        pos.z = gameObject.transform.position.z + 1;

        GameObject weapon = Instantiate(weaponPrefab, pos, Quaternion.identity); //Creates a weapon
        weapon.transform.parent = gameObject.transform;
        Weapon wpnScript = weapon.GetComponent<Weapon>();
        if (wpnScript)
        {
            wpnScript.Damage = damage; //Set shot damage
            wpnScript.Duration = attackDuration;

            if (dirState == DirectionState.Up) //Player attacks Up
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 0));
                wpnScript.transform.localPosition += new Vector3(0, 0.75f, 0);
            }
            else if (dirState == DirectionState.Down) //Player attacks Down
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 180));
                wpnScript.transform.localPosition += new Vector3(0, -0.75f);
            }
            else if (dirState == DirectionState.Left) //Player attacks Left
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 90));
                wpnScript.transform.localPosition += new Vector3(-0.75f, -0.0625f);
            }
            else if (dirState == DirectionState.Right) //Player attacks Right
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 270));
                wpnScript.transform.localPosition += new Vector3(0.75f, -0.0625f);
            }
        }

        //Set thread to turn swiping back on in a short amount of time
        StartCoroutine(DelayNextAttack(attackDelay));
    }

    protected void RangedAttack() //Ranged Attack
    {
        PlayAudio("event:/SFX/Player/Player_Shoot");
        Vector3 pos = gameObject.transform.position;

        if (dirState == DirectionState.Left || dirState == DirectionState.Right)
            pos.y -= 0.0625f;

        pos.z += 1;

        GameObject projectile = Instantiate(projectilePrefab, pos, gameObject.transform.rotation); //Creates a projectile
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript)
        {
            projScript.Damage = damage; //Set shot damage
            projScript.Speed = rangedSpeed;

            if(dirState == DirectionState.Up) //Projectile shoots Up
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

        //Set thread to turn shooting back on in a short amount of time
        StartCoroutine(DelayNextAttack(attackDelay));
    }

    protected void ChargeAttack() //Charge Attack
    {
        PlayAudio("event:/SFX/Player/Player_StrongAttack");
        Vector3 pos = gameObject.transform.position;
        GameObject weapon = Instantiate(strongWeaponPrefab, pos, gameObject.transform.rotation); //Creates a strong weapon
        weapon.transform.parent = gameObject.transform;
        weapon.transform.localPosition = new Vector3(0, 0, 1);
        StrongWeapon wpnScript = weapon.GetComponent<StrongWeapon>();
        if (wpnScript)
        {
            wpnScript.Damage = damage * 2; //Set shot damage
            wpnScript.Duration = attackDuration;
            List<Vector2> positions = new List<Vector2>();
            if (dirState == DirectionState.Up) //Player attacks Up
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 0));
                wpnScript.transform.localPosition += new Vector3(-0.8125f, 0.8125f, 0);
                positions = new List<Vector2>() { new Vector2(-0.8125f, 0.8125f), new Vector2(0, 1.0f), new Vector2(0.8125f, 0.8125f) };
            }
            else if (dirState == DirectionState.Down) //Player attacks Down
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 180));
                wpnScript.transform.localPosition += new Vector3(0.8125f, -0.8125f, 0);
                positions = new List<Vector2>() { new Vector2(0.8125f, -0.8125f), new Vector2(0, -1.0f), new Vector2(-0.8125f, -0.8125f) };
            }
            else if (dirState == DirectionState.Left) //Player attacks Left
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 90));
                wpnScript.transform.localPosition += new Vector3(-0.8125f, -0.8125f, 0);
                positions = new List<Vector2>() { new Vector2(-0.8125f, -0.8125f), new Vector2(-1.0f, 0), new Vector2(-0.8125f, 0.8125f) };
            }
            else if (dirState == DirectionState.Right) //Player attacks Right
            {
                wpnScript.transform.Rotate(new Vector3(0, 0, 270));
                wpnScript.transform.localPosition += new Vector3(0.8125f, 0.8125f, 0);
                positions = new List<Vector2>() { new Vector2(0.8125f, 0.8125f), new Vector2(1.0f, 0), new Vector2(0.8125f, -0.8125f) };
            }
            wpnScript.LocalPositions = positions;
        }

        //Set thread to turn swiping back on in a short amount of time
        StartCoroutine(DelayNextAttack(attackDelay));
    }

    protected void Charge() //charges attack
    {
        if (chargeElapsed < chargeDuration && Input.GetKey(KeyCode.K))
            chargeElapsed += Time.deltaTime;
        else if (chargeElapsed >= chargeDuration && Input.GetKey(KeyCode.K))
        {
            charged = true;
            charging = false;
            StartCoroutine(ChargeFlash(0.09f));
        }
        else
        {
            charging = false;
            chargeElapsed = 0;
        }
    }

    protected IEnumerator ChargeFlash(float flashDelay) //Player flashes while attack is charged
    {
        int index = 0;

        while (charged)
        {
            //Changes Color
            if (index == 0)
            {
                yield return new WaitForSeconds(flashDelay);
                spriteRenderer.color = Color.gray;
            }
            else
            {
                yield return new WaitForSeconds(flashDelay);
                spriteRenderer.color = Color.white;
            }

            index++;
            if (index > 1)
                index = 0;
        }
        spriteRenderer.color = Color.white;
    }

    protected IEnumerator DelayNextAttack(float delay)
    {
        canAttack = false;
        float elapsed = 0; //Time since last attack

        while (elapsed < delay)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            elapsed += Time.deltaTime;
        }
        canAttack = true;
    }

    //------------------------Collision Handling------------------------
    protected virtual void OnCollisionStay2D(Collision2D collision) //Handles collisions between player and physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (canTakeDamage) //Player touches an enemy
        {
            if (other.tag == "Enemy") //Player takes damage from contact with enemy
            {
                Enemy enemyScript = other.GetComponent<Enemy>();
                if (enemyScript)
                    TakeDamage(enemyScript.Damage, collisionDirection);
            }
            else if (other.tag == "Lock") //Player touches a locked door
            {
                Door doorScript = other.GetComponent<Door>();
                if (doorScript)
                {
                    if (doorScript.Locked && keys > 0)
                    {
                        ChangeKeys(-1);
                        other.GetComponent<Door>().OpenDoor();
                        PlayAudio("event:/SFX/Game/Game_OpenDoor");
                    }
                }
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles collisions between player and non-physical GameObjects
    {
        GameObject other = collision.gameObject;
        
        if (other.tag == "Switch") //Player steps on switch
        {
            Switch switchScript = other.GetComponent<Switch>();
            if (switchScript)
            {
                if (!switchScript.Activated)
                {
                    switchScript.Activate();
                    PlayAudio("event:/SFX/Game/Game_PressSwitch");
                }
            }
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision) //Handles collisions between player and non-physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = (transform.position - other.transform.position).normalized;

        if (other.tag == "Pickup") //Player touches Pickup
        {
            Pickup pickupScript = other.GetComponent<Pickup>();
            if (pickupScript)
            {
                if (pickupScript.Type == PickupType.Health || pickupScript.Type == PickupType.Ammo)
                {
                    if (pickupScript.Type == PickupType.Health)
                        ChangeHealth(pickupScript.Value);
                    else
                        ChangeAmmo(pickupScript.Value);

                    PlayAudio("event:/SFX/Player/Player_GrabPotion");
                }
                else if (pickupScript.Type == PickupType.Key)
                {
                    ChangeKeys(pickupScript.Value);
                    PlayAudio("event:/SFX/Player/Player_GrabKey");
                }
            }
            Destroy(other);
        }
        else if (other.tag == "EnemyProjectile" && canTakeDamage) //Enemy projectile hits player
        {
            Projectile projScript = other.GetComponent<Projectile>();
            if (projScript)
            {
                float height = boxCollider.size.y; //Height of collider
                float width = boxCollider.size.x; //Width of collider
                Vector2 pos = transform.position; //Position of player
                Vector2 otherPos = other.transform.position; //Position of player
                bool blockProjectile = false;

                if (state == PlayerState.Idle || state == PlayerState.Walk) //Player can block projectiles while idle
                {
                    if (Mathf.Abs(collisionDirection.y) / height >= Mathf.Abs(collisionDirection.x) / width) //Player blocks projectile from top or bottom
                    {
                        if ((otherPos.y >= pos.y && dirState == DirectionState.Up) || (otherPos.y <= pos.y && dirState == DirectionState.Down))
                            blockProjectile = true;
                    }
                    else if (Mathf.Abs(collisionDirection.x) / width > Mathf.Abs(collisionDirection.y) / height) //Player blocks projectile from left or right
                    {
                        if ((otherPos.x >= pos.x && dirState == DirectionState.Right) || (otherPos.x <= pos.x && dirState == DirectionState.Left))
                            blockProjectile = true;
                    }
                }

                if (!blockProjectile)
                    TakeDamage(projScript.Damage, collisionDirection); //Gives player damage and knockback
                else
                    PlayAudio("event:/SFX/Player/Player_Block");

                /*//Player blocks projectile
                if (state == PlayerState.Idle &&
                    ((dirState == DirectionState.Up && GetNearestAxis2D(collisionDirection) == Vector2.down) ||
                    (dirState == DirectionState.Down && GetNearestAxis2D(collisionDirection) == Vector2.up) ||
                    (dirState == DirectionState.Left && GetNearestAxis2D(collisionDirection) == Vector2.right) ||
                    (dirState == DirectionState.Right && GetNearestAxis2D(collisionDirection) == Vector2.left)))
                {
                    //Empty in case logic is needed later
                }
                //Player does not block projectile
                else
                {
                    TakeDamage(projScript.Damage, collisionDirection); //Gives player damage and knockback
                }*/
                
            }
            Destroy(other);
        }
        else if (other.tag == "End") //Player reaches the end of the game
        {
            hasWon = true;
        }
    }

    //------------------------Health and Damage------------------------
    protected void ChangeHealth(float value) //Increases and Decreases player's health
    {
        if ((health + value) > maxHealth) //Health raised to max
            health = maxHealth;
        else if ((health + value) <= 0) //Health lowered to 0
        {
            health = 0;
            isDead = true;
        }
        else //Raises or lowers health
            health += value;

        
        if (health / maxHealth <= 0.5f && !IsAudioPlaying(lowHealthSound)) //Start sound when health drops below 50%
            lowHealthSound.start();
        else if (health / maxHealth > 0.5f && IsAudioPlaying(lowHealthSound)) //Fade sound out when health rises above 50%
            lowHealthSound.stop(STOP_MODE.ALLOWFADEOUT);
        else if (isDead && IsAudioPlaying(lowHealthSound)) //Stop sound when dead
            lowHealthSound.stop(STOP_MODE.IMMEDIATE);
    }

    public override void TakeDamage(float value, Vector2 kbDirection) //Decreases player's health and Handles knockback
    {
        ChangeHealth(-value);


        if (charging) //Stops player from charging
            charging = false;

        if (!isDead)
        {
            PlayAudio("event:/SFX/Player/Player_TakeDamage");
            StartCoroutine(Flash(1.0f, 0.09f));
            StartCoroutine(TakeKnockBack(0.1f, kbDirection));
        }
    }

    protected void ChangeAmmo(float value) //Increases and Decreases player's ammo
    {
        if ((ammo + value) > maxAmmo) //Ammo raised to max
            ammo = maxAmmo;
        else if ((ammo + value) <= 0) //Ammo lowered to 0
            ammo = 0;
        else //Raises or lowers ammo
            ammo += value;
    }

    protected void ChangeKeys(float value) //Increases and Decreases player's ammo
    {
        if ((keys + value) > maxKeys) //Keys raised to max
            keys = maxKeys;
        if ((keys + value) <= 0) //Keys lowered to 0
            keys = 0;
        else //Raises or lowers keys
            keys += value;
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
}
