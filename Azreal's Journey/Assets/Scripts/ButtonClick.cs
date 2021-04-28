using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonClick : MonoBehaviour
{
    private Button button;
    private bool selected;
    private bool prevSelected;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => PlayAudio("event:/Interface/UI_Confirm"));
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject == EventSystem.current.currentSelectedGameObject) //Determines whether object is selected
            selected = true;
        else
            selected = false;

        if (button)
        {
            if (selected && Input.GetKeyDown(KeyCode.K)) //Clicks button if button is selected and K key is pressed (also clicks if Enter is pressed
                button.onClick.Invoke();

            if (selected && !prevSelected) //Button was selected
                PlayAudio("event:/Interface/UI_Select");
        }

        prevSelected = selected; //Updates previous value
    }

    public void SelectManually()
    {
        if (button)
        {
            button.Select(); //Automatically selects first button
            button.OnSelect(null);
        }

        selected = true; //Prevents selection beep when selected by script
        prevSelected = true; 
    }

    private void PlayAudio(string path) //Plays audio found at path
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }
}
