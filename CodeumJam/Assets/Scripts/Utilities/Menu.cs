using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Menu : MonoBehaviour
{
    public int menuID = 0;
    [SerializeField] private GameObject player;
    public GameObject[] menuPanel;
    [SerializeField] Slider VolumeSlider;
    // Start is called before the first frame update
    void Start()
    {
        //I find all panels, which is one
        /* menuPanel = GameObject.FindGameObjectsWithTag("Menu");
         menuPanel[0] = GameObject.Find("Menu");
         menuPanel[1] = GameObject.Find("Options");*/
        Player player = GetComponent<Player>();
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
            //Debug.Log(panel.name);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            SwitchtoMenu(0);
        }
        
    }

    /*public void OpenMenu()
    {
        menuPanel[menuID].SetActive(true);
        Time.timeScale = 0.0f;
    }
    public void Resume()
    {
        menuPanel[menuID].SetActive(false);
        Time.timeScale = 1.0f;
    }*/
    public void SwitchtoMenu(int menuID)
    {
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
            //Debug.Log(panel.name);
        }

        menuPanel[menuID].SetActive(true);
        player.GetComponent<PlayerInput>().MouseLock = false;
        player.GetComponent<Player>().AllowMovement(false);
        //player.
        //player.playerComponent.PlayerInput.MouseLock = true;

        Time.timeScale = 0.0f;

    }
    public void CloseMenu()
    {
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
            //Debug.Log(panel.name);
        }
        player.GetComponent<PlayerInput>().MouseLock = true;
        player.GetComponent<Player>().AllowMovement(true);
        Time.timeScale = 1.0f;
    }
    public void ReturntoMenu()
    {
        Time.timeScale = 1.0f;
        //Returns to main menu
        //SceneManager.LoadScene(0);
    }
    public void ChangeVolume()
    {
        AudioListener.volume = VolumeSlider.value;
    }
}
