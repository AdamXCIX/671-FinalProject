using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClearCondition
{
    Enemy,
    Switch,
    Boss
}
public class RoomManager : MonoBehaviour
{
    //[SerializeField] private bool startPaused;
    [SerializeField] private bool spawnKey;
    [SerializeField] private ClearCondition clearCondition;
    [SerializeField] private GameObject switchParent;
    private List<GameObject> switches;
    [SerializeField] private GameObject enemyParent;
    private List<GameObject> enemies;
    [SerializeField] private GameObject bossParent;
    private List<GameObject> bosses;
    [SerializeField] private GameObject doorParent;
    private List<GameObject> doors;

    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject keySpawn;

    private bool cleared;

    // Start is called before the first frame update
    void Start()
    {
        cleared = false;
        //SetPaused(startPaused);
        switches = new List<GameObject>();
        enemies = new List<GameObject>();
        bosses = new List<GameObject>();
        doors = new List<GameObject>();

        for (int i = 0; i < switchParent.transform.childCount; i++) //Creates list of switches from parent GameObject
        {
            switches.Add(switchParent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < enemyParent.transform.childCount; i++) //Creates list of enemies from parent GameObject
        {
            enemies.Add(enemyParent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < bossParent.transform.childCount; i++) //Creates list of bosses from parent GameObject
        {
            bosses.Add(bossParent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < doorParent.transform.childCount; i++) //Creates list of doors from parent GameObject
        {
            doors.Add(doorParent.transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!cleared)
        {
            if (clearCondition == ClearCondition.Enemy) //Room clears when enemies are defeated
            {
                if (enemies.Count <= 0)
                    cleared = true;
            }
            if (clearCondition == ClearCondition.Boss) //Room clears when bosses are defeated
            {
                if (bosses.Count <= 0)
                    cleared = true;
            }
            else if (clearCondition == ClearCondition.Switch) //Room clears when switches are defeated
            {
                bool allActivated = true;
                for (int i = 0; i < switches.Count; i++)
                {
                    Switch switchScript = switches[i].GetComponent<Switch>();
                    if (switchScript) //Ensures script is on switch
                    {
                        if (!switchScript.Activated) //Checks if switch has been activated
                            allActivated = false;
                    }
                }

                if (allActivated) //All switches must be actiavted to clear
                    cleared = true;
            }

            if (cleared)
            {
                for (int i = 0; i < doors.Count; i++) //Hides doors when cleared
                {
                    doors[i].GetComponent<Door>().OpenDoor();
                }

                if (spawnKey)
                    Instantiate(keyPrefab, keySpawn.transform.position, gameObject.transform.rotation);
            }

        }

        for (int i = enemies.Count - 1; i >= 0; i--) //Removes defeated enemies
        {
            if (enemies[i] == null) 
                enemies.RemoveAt(i);
        }
        for (int i = bosses.Count - 1; i >= 0; i--) //Removes defeated bosses
        {
            if (bosses[i] == null)
                bosses.RemoveAt(i);
        }
    }

    public void SetPaused(bool pause) //Determines whether enemies in the room should be paused
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemyScript = enemies[i].GetComponent<Enemy>();
            if (enemyScript)
                enemyScript.Paused = pause;
        }

        for (int i = 0; i < bosses.Count; i++)
        {
            Enemy enemyScript = bosses[i].GetComponent<Enemy>();
            if (enemyScript)
                enemyScript.Paused = pause;
        }
    }

    public void Reset() //Resets enemies and any other objects that should reset when player enters
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemyScript = enemies[i].GetComponent<Enemy>();
            if (enemyScript)
                enemyScript.Reset();
        }
    }
}
