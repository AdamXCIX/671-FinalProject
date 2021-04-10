using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    protected bool activated;
    [SerializeField] protected Sprite deactivatedSprite;
    [SerializeField] protected Sprite activatedSprite;

    public bool Activated
    {
        get { return activated; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Deactivate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles collisions between switch and non-physical GameObjects
    {
        GameObject other = collision.gameObject;

        if (other.tag == "Player" && !activated) //Player steps on switch
        {
            Activate();
        }
    }*/

    public void Deactivate()
    {
        activated = false;
        GetComponent<SpriteRenderer>().sprite = deactivatedSprite;
    }

    public void Activate()
    {
        activated = true;
        GetComponent<SpriteRenderer>().sprite = activatedSprite;
    }
}
