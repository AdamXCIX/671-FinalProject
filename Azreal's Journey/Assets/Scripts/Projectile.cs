using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected float damage;
    protected float speed;
    protected Vector2 direction;
    protected Rigidbody2D rigidBody;

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    public Vector2 Direction
    {
        get { return direction; }
        set { direction = value; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic; 
        rigidBody.velocity = speed * direction;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (rigidBody.velocity != speed * direction) //Allows speed and direction to be updated
            rigidBody.velocity = speed * direction;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles Collisions
    {
        GameObject other = collision.gameObject;
        if (other.layer == LayerMask.NameToLayer("Obstacle")) //Projectile hits wall
            Destroy(gameObject);
        /*else if (tag == "PlayerProjectile" &&  other.tag == "Enemy" || tag == "EnemyProjectile" && other.tag == "Player") //Projectile hits player or enemy
        {
            other.GetComponent<Character>().TakeDamage(Damage, other.transform.position - transform.position); //Gives opposing character damage and knockback
            Destroy(gameObject); 
        }*/
    }
}
