using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum GameState
{
    Menu,
    Game,
    Win,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private float titleDuration;

    private GameObject player;
    private Player playerScript;
    [SerializeField] private GameObject roomsParent;
    private List<GameObject> rooms;
    [SerializeField] private GameObject startingRoom;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject menuButtons;

    [SerializeField] private GameObject hud;

    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameOverTitle;
    [SerializeField] private GameObject gameOverBox;
    [SerializeField] private GameObject gameOverButtons;


    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject winTitle;
    [SerializeField] private GameObject winBox;
    [SerializeField] private GameObject winButtons;

    private GameState state;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        state = GameState.Menu; //Game starts at menu

        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        rooms = new List<GameObject>();

        for (int i = 0; i < roomsParent.transform.childCount; i++) //Creates list of rooms from parent GameObject
        {
            rooms.Add(roomsParent.transform.GetChild(i).gameObject);
        }

        GoToTitle();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { //User presses Escape
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0)) { //User clicks on game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }



        if (state == GameState.Game && playerScript.IsDead) //Player dies
            GameOver();
        else if (state == GameState.Game && playerScript.HasWon) //Player reaches end
            Win();

        bool btnSelected = false;

        if (state == GameState.Menu) //Prevents menu buttons from becoming deselected
        {
            for (int i = 0; i < menuButtons.transform.childCount; i++)
            {
                if (menuButtons.transform.GetChild(i).gameObject == EventSystem.current.currentSelectedGameObject)
                    btnSelected = true;
            }
            if (!btnSelected)
                menuButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button
        }
        else if (state == GameState.GameOver) //Prevents game over screen buttons from becoming deselected
        {
            for (int i = 0; i < gameOverButtons.transform.childCount; i++)
            {
                if (gameOverButtons.transform.GetChild(i).gameObject == EventSystem.current.currentSelectedGameObject)
                    btnSelected = true;
            }
            if (!btnSelected)
                gameOverButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button
        }
        else if (state == GameState.Win) //Prevents win screen buttons from becoming deselected
        {
            for (int i = 0; i < winButtons.transform.childCount; i++)
            {
                if (winButtons.transform.GetChild(i).gameObject == EventSystem.current.currentSelectedGameObject)
                    btnSelected = true;
            }
            if (!btnSelected)
                winButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button
        }
    }

    public void GoToTitle()
    {
        playerScript.Paused = true; //Pauses player
        state = GameState.Menu;

        mainMenu.SetActive(true);
        hud.SetActive(false);
        gameOverMenu.SetActive(false);
        winMenu.SetActive(false);

        menuButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button


        for (int i = 0; i < rooms.Count; i++) //Pauses all rooms
        {
            rooms[i].GetComponent<RoomManager>().SetPaused(true);
        }
    }

    public void StartGame() //Starts game
    {
        state = GameState.Game;

        mainMenu.SetActive(false);
        hud.SetActive(true);
        gameOverMenu.SetActive(false);
        winMenu.SetActive(false);

        playerScript.Paused = false; //Starts player
        startingRoom.GetComponent<RoomManager>().SetPaused(false); //Starts first room
    }

    public void GameOver() //Game Over Screen
    {
        state = GameState.GameOver;

        mainMenu.SetActive(false);
        hud.SetActive(false);
        gameOverMenu.SetActive(true);
        winMenu.SetActive(false);

        playerScript.Paused = true; //Pauses player
        for (int i = 0; i < rooms.Count; i++) //Pauses all rooms
            rooms[i].GetComponent<RoomManager>().SetPaused(true); 

        StartCoroutine(GameOverText(titleDuration));
    }

    IEnumerator GameOverText(float duration)
    {
        gameOverTitle.SetActive(true);
        gameOverBox.SetActive(false);
        gameOverButtons.SetActive(false);

        float elapsed = 0;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            elapsed += Time.deltaTime;
        }

        gameOverTitle.SetActive(false);
        gameOverBox.SetActive(true);
        gameOverButtons.SetActive(true);

        gameOverButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button
    }

    public void Win() //Win Screen
    {
        state = GameState.Win;

        mainMenu.SetActive(false);
        hud.SetActive(false);
        gameOverMenu.SetActive(false);
        winMenu.SetActive(true);

        playerScript.Paused = true; //Pauses player
        for (int i = 0; i < rooms.Count; i++) //Pauses all rooms
            rooms[i].GetComponent<RoomManager>().SetPaused(true);

        StartCoroutine(WinText(titleDuration));
    }

    IEnumerator WinText(float duration)
    {
        winTitle.SetActive(true);
        winBox.SetActive(false);
        winButtons.SetActive(false);

        float elapsed = 0;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            elapsed += Time.deltaTime;
        }

        winTitle.SetActive(false);
        winBox.SetActive(true);
        winButtons.SetActive(true);

        winButtons.transform.GetChild(0).GetComponent<ButtonClick>().SelectManually(); //Automatically selects first button
    }

    public void Exit() //Closes Game
    {
        Application.Quit();
    }

    public void Restart() //Restarts Game
    {
        SceneManager.LoadScene("GameScene");
    }
}
