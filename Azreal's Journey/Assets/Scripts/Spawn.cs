using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private GameObject room;
    private Vector2 roomPos;

    public GameObject Room
    {
        get { return room; }
    }
    public Vector2 RoomPos
    {
        get { return roomPos; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform tf = transform;
        while (tf.parent != null) //Gets ancestor room object
        {
            if (tf.parent.tag == "Room")
                room = tf.parent.gameObject;
            tf = tf.parent.transform;
        }

        if (room)
            roomPos = room.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
