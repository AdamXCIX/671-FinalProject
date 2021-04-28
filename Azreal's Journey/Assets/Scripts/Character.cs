using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    protected float health; //Character's max health
    [SerializeField] protected float maxHealth; //Character's max health
    [SerializeField] protected float damage; //Amount of Damage Character Does
    [SerializeField] protected float kbForce; //Knockback force

    protected bool canTakeDamage; //Character can be damaged
    protected bool isDead;
    protected bool paused; //Whether updating the character is paused

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Rigidbody2D rigidBody;
    protected BoxCollider2D boxCollider;

    [SerializeField] protected float walkSpeed;

    public float Health
    {
        get { return health; }
    }

    public float MaxHealth
    {
        get { return maxHealth; }
    }

    public float Damage
    {
        get { return damage; }
    }

    public bool Paused
    {
        get { return paused; }
        set { paused = value; }
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        canTakeDamage = true;
        isDead = false;
        health = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rigidBody.freezeRotation = true; //Prevents player from rotating
    }

    // Update is called once per frame
    protected virtual void Update()
    {
    }

    protected virtual void UpdateAnimation()
    {

    }

    public virtual void TakeDamage(float damage, Vector2 kbDirection) //Character takes damage and shows an indication of damage
    {
        if (canTakeDamage)
        {
            health -= damage;
            StartCoroutine(TakeKnockBack(0.1f, kbDirection));
            StartCoroutine(Flash(0.35f, 0.09f));
        }

        if (health <= 0) //Sets flag when dead
            isDead = true;
    }

    //------------------------Damage Indicators------------------------
    protected IEnumerator Flash(float flashDuration, float flashDelay)
    {
        canTakeDamage = false;

        float flashElapsed = 0; //Time player has been flashing
        //Color temp = spriteRenderer.color; //Temporary color used to change player's alpha
        int index = 0;
        while (flashElapsed < flashDuration)
        {
            //Changes Alpha
            /*temp.a = 0f;
            spriteRenderer.color = temp;
            yield return new WaitForSeconds(flashDelay);
            temp.a = 255f;
            spriteRenderer.color = temp;
            yield return new WaitForSeconds(flashDelay);
            flashElapsed += flashDelay * 2;*/

            //Changes Color
            if (index == 0)
            {
                yield return new WaitForSeconds(flashDelay);
                spriteRenderer.color = Color.red;
            }
            else
            {
                yield return new WaitForSeconds(flashDelay);
                spriteRenderer.color = Color.gray;
            }

            index++;
            if (index > 1)
                index = 0;

            flashElapsed += flashDelay;
        }
        spriteRenderer.color = Color.white;
        canTakeDamage = true;
    }

    protected IEnumerator TakeKnockBack(float kbDuration, Vector2 kbDirection)
    {
        float kbElapsed = 0; //Time player has been flashing

        while (kbElapsed < kbDuration)
        {
            rigidBody.velocity = kbDirection.normalized * kbForce;
            yield return new WaitForSeconds(Time.deltaTime);
            kbElapsed += Time.deltaTime;
        }
    }


    protected virtual void Move(float speed)
    {
        rigidBody.velocity = new Vector2(speed, rigidBody.velocity.y);
    }
}
