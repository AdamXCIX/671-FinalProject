using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClearCondition
{
    Enemy,
    Switch,
    Boss,
    None
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

    [SerializeField] private float unlockDelay = 0.5f;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject keySpawn;

    private bool cleared;
    private float bossMaxHealth = 0;


    public ClearCondition Condition
    {
        get { return clearCondition; }
    }

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
            switches.Add(switchParent.transform.GetChild(i).gameObject);

        for (int i = 0; i < enemyParent.transform.childCount; i++) //Creates list of enemies from parent GameObject
            enemies.Add(enemyParent.transform.GetChild(i).gameObject);

        for (int i = 0; i < bossParent.transform.childCount; i++) //Creates list of bosses from parent GameObject
            bosses.Add(bossParent.transform.GetChild(i).gameObject);

        for (int i = 0; i < doorParent.transform.childCount; i++) //Creates list of doors from parent GameObject
            doors.Add(doorParent.transform.GetChild(i).gameObject);

        if ((clearCondition == ClearCondition.Enemy && enemies.Count == 0) || (clearCondition == ClearCondition.Boss && bosses.Count == 0) || 
            (clearCondition == ClearCondition.Switch && switches.Count == 0)) //Room is cleared by default if corresponding list is empty
        {
            cleared = true;
        }

        if (bosses.Count > 0) //Set starting health for all bosses
        {
            for (int i = 0; i < bosses.Count; i++)
            {
                Boss boss = bosses[i].GetComponent<Boss>();
                if (boss)
                    bossMaxHealth += boss.MaxHealth;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.CurrentRoom == gameObject)
        {
            if (!cleared)
            {
                if (clearCondition == ClearCondition.Enemy) //Room clears when enemies are defeated
                {
                    if (enemies.Count <= 0)
                        cleared = true;
                }
                else if (clearCondition == ClearCondition.Boss) //Room clears when bosses are defeated
                {
                    if (bosses.Count <= 0)
                    {
                        cleared = true;
                        GameManager.instance.PlayDungeonMusic();
                    }
                    else
                    {
                        float bossCurrentHealth = 0;

                        for (int i = 0; i < bosses.Count; i++) //Gets current health of all bosses
                        {
                            if (bosses[i] != null)
                            {
                                Boss boss = bosses[i].GetComponent<Boss>();
                                if (boss)
                                    bossCurrentHealth += boss.Health;
                            }
                        }

                        //Sets boss health variable of GameManager
                        if (bossMaxHealth > 0)
                            GameManager.instance.BossHealthPercent = bossCurrentHealth / bossMaxHealth;
                        else
                            GameManager.instance.BossHealthPercent = 1.0f;
                    }
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
                else //Room has no clear condition and opens by default
                    cleared = true;

                if (cleared)
                {
                    if (GameManager.instance.StartingRoom == gameObject) //Open starting room immediately
                        StartCoroutine(UnlockRoom(0));
                    else
                        StartCoroutine(UnlockRoom(unlockDelay)); //Open other rooms after delay
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
    }

    private IEnumerator UnlockRoom(float secondDelay)
    {
        while (secondDelay > 0) {
            yield return new WaitForSeconds(Time.deltaTime);
            secondDelay -= Time.deltaTime;
        }

        for (int i = 0; i < doors.Count; i++) //Hides doors when cleared
        {
            doors[i].GetComponent<Door>().OpenDoor();
        }

        if (GameManager.instance.StartingRoom != gameObject) //Don't play opening noise for first room
            PlayAudio("event:/SFX/Game/Game_OpenDoor");

        if (spawnKey)
        {
            Instantiate(keyPrefab, keySpawn.transform.position, gameObject.transform.rotation);
            PlayAudio("event:/SFX/Game/Game_SpawnKey");
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

    //------------------------Audio------------------------
    private void PlayAudio(string path) //Plays audio found at path
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }
}
