using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jar : MonoBehaviour
{

    [SerializeField] protected GameObject pickup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles collisions between switch and non-physical GameObjects
    {
        GameObject other = collision.gameObject;

        if (other.tag == "PlayerWeapon") //Player breaks jar
        {
            PlayAudio("event:/SFX/Player/Player_BreakJar");
            SpawnPickup();
            Destroy(gameObject);
        }
    }

    private void PlayAudio(string path) //Plays audio found at path
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }

    //------------------------Other------------------------
    protected void SpawnPickup() //Spawns an item at the Enemy's position
    {
        /*if (pickups.Count >= 2)
        {
            int value = GenerateWeightedRandom(new int[] { 2, 2, 6 });
            if (value == 0 && pickups[0])
                Instantiate(pickups[0], transform.position, Quaternion.identity); //Creates pickup type 1
            else if (value == 1 && pickups[1])
                Instantiate(pickups[1], transform.position, Quaternion.identity); //Creates pickup type 2
        }*/

        if (pickup != null)
            Instantiate(pickup, transform.position, Quaternion.identity);
    }

    //------------------------Utility------------------------
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
