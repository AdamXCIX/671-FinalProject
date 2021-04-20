using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    private float cameraElapsed; //Time camera has been moving
    private float cameraDuration; //Time camera takes to move
    private GameObject room;
    [SerializeField] private GameObject spawn;
    private GameObject player;

    private Vector3 startPos; //Starting Position of camera
    private Vector3 targetPos; //Position of room for camera to move to
    private Vector2 spawnPos; //Location for player to spawn to

    private Spawn spawnScript;
    private RoomManager exitRoomScript;
    private RoomManager spawnRoomScript;
    private bool movingRooms;


    // Start is called before the first frame update
    void Start()
    {
        cameraDuration = 1.0f;
        player = GameObject.Find("Player");

        if (spawn)
        {
            spawnPos = spawn.transform.position;
            spawnScript = spawn.GetComponent<Spawn>();
            if (spawnScript)
            {
                targetPos = spawnScript.RoomPos;
                targetPos.z = Camera.main.transform.position.z; //Saves camera's z position to target

                if (spawnScript.Room) //Gets reference to spawn room
                    spawnRoomScript = spawnScript.Room.GetComponent<RoomManager>();
            }
        }

        Transform tf = transform;
        while (tf.parent != null) //Gets ancestor room object
        {
            if (tf.parent.tag == "Room")
                room = tf.parent.gameObject;
            tf = tf.parent.transform;
        }
        
        if (room)
            exitRoomScript = room.GetComponent<RoomManager>();

        movingRooms = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (movingRooms)
        {
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, cameraElapsed / cameraDuration); //Moves camera to new position
            cameraElapsed += Time.deltaTime;

            if (cameraElapsed / cameraDuration >= 1.0f) //Camera is close enough to target
            {
                Camera.main.transform.position = targetPos; //Ensures camera is in correct place
                player.transform.position = spawnPos; //Moves player to new room
                player.GetComponent<Player>().Paused = false; //Resumes Player
                spawnRoomScript.SetPaused(false); //Resumes new room
                movingRooms = false;

                if (spawnRoomScript.Condition == ClearCondition.Boss) //Plays boss music if in boss room
                    GameManager.instance.PlayBossMusic();
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) //Handles collisions between transition and player
    {
        GameObject other = collision.gameObject;

        if (other.tag == "Player") //Player leaves room
        {
            exitRoomScript.SetPaused(true); //Pauses current room
            spawnRoomScript.Reset(); //Resets new room
            other.GetComponent<Player>().Paused = true; //Pause player
            startPos = Camera.main.transform.position;
            cameraElapsed = 0;
            movingRooms = true;
        }
    }
}
