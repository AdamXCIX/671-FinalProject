using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongWeapon : Weapon
{
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected List<Sprite> animFrames;
    protected List<Vector2> localPositions;
    public List<Vector2> LocalPositions
    {
        get { return localPositions; }
        set { localPositions = value; }
    }

    protected float timePerFrame;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        timePerFrame = duration / animFrames.Count;

        if (elapsed >= duration) //Destroys weapon when attack is finished
            Destroy(gameObject);
        else
        {
            for (int i = animFrames.Count - 1; i >= 0; i--) //Updates stages of animation based on speed
            {
                if (elapsed <= duration / animFrames.Count * (i + 1))
                    spriteRenderer.sprite = animFrames[i];
            }

            for (int i = localPositions.Count - 1; i >= 0; i--) //Updates stages of position based on speed
            {
                if (elapsed <= duration / localPositions.Count * (i + 1))
                    transform.localPosition = localPositions[i];
            }

            elapsed += Time.deltaTime;
        }
        
    }
}
