using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private float lifeValue;
    [SerializeField] private List<GameObject> lifeIcons;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite halfSprite;
    [SerializeField] private Sprite emptySprite;

    [SerializeField] private GameObject ammoValue;
    private Text ammoText;

    [SerializeField] private GameObject keyValue;
    private Text keyText;

    private GameObject player;
    private Player playerScript;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        lifeValue = playerScript.Health;
        UpdateLife();

        ammoText = ammoValue.GetComponent<Text>();
        keyText = keyValue.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeValue != playerScript.Health)
        {
            lifeValue = playerScript.Health;
            UpdateLife();
        }

        if (ammoText.text != playerScript.Ammo.ToString()) //Update Ammo Text
            ammoText.text = playerScript.Ammo.ToString(); 

        if (keyText.text != playerScript.Keys.ToString()) //Update Key Text
            keyText.text = playerScript.Keys.ToString();
    }

    private void UpdateLife() //Updates life bubbles based on health
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeValue >= 2 + 2 * i)
                lifeIcons[i].GetComponent<Image>().sprite = fullSprite;
            else if (lifeValue >= 1 + 2 * i)
                lifeIcons[i].GetComponent<Image>().sprite = halfSprite;
            else
                lifeIcons[i].GetComponent<Image>().sprite = emptySprite;
        }
    }
}
