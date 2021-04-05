using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    protected float damage;
    protected float duration;
    protected float elapsed;
    protected Rigidbody2D rigidBody;

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public float Duration
    {
        get { return duration; }
        set { duration = value; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        elapsed = 0;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (elapsed >= duration) //Destroys weapon when attack is finished
            Destroy(gameObject);
        else
            elapsed += Time.deltaTime;
    }

    /*protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles Collisions
    {
        GameObject other = collision.gameObject;
        if (tag == "PlayerWeapon" && other.tag == "Enemy") //Weapon hits enemy
        {
            other.GetComponent<Character>().TakeDamage(Damage, other.transform.position - transform.position); //Gives opposing character damage and knockback
        }
    }*/
}
