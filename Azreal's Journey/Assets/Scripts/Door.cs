using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    protected bool locked;
    private BoxCollider2D boxCollider;
    private PolygonCollider2D polyCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private bool startClosed;
    [SerializeField]  private Sprite closedSprite;
    [SerializeField]  private Sprite openSprite;

    public bool Locked
    {
        get { return locked; }
    }

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (startClosed)
            CloseDoor();
        else
            OpenDoor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseDoor()
    {
        locked = true;
        boxCollider.enabled = true;
        polyCollider.enabled = false;
        spriteRenderer.sprite = closedSprite;
    }
    public void OpenDoor()
    {
        locked = false;
        boxCollider.enabled = false;
        polyCollider.enabled = true;
        spriteRenderer.sprite = openSprite;
    }
}
