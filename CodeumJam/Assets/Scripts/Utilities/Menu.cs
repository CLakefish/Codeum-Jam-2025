using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using TMPro;

public class Menu : MonoBehaviour
{
    public int menuID = 0;
    [SerializeField] private GameObject player;
    public GameObject[] menuPanel;
    [SerializeField] Slider VolumeSlider;

    [Header("Menu Objects")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private bool MenuBool = false;

    [Header("Level References")]
    private string description;
    private string name;
    // Start is called before the first frame update
    void Start()
    {
        Player player = GetComponent<Player>();
        VolumeSlider.value = Load("Volume");
        Setup();
        nameText.text = name;
        descriptionText.text = description;
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
        }
    }
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && MenuBool == false)
        {
            SwitchtoMenu(0);
            MenuBool = true;
        }
        else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && MenuBool == true)
        {
            CloseMenu();
            MenuBool = false;
        }
        
    }
    public void SwitchtoMenu(int menuID)
    {
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
        }
        menuPanel[menuID].SetActive(true);
        player.GetComponent<PlayerInput>().MouseLock = false;
        Time.timeScale = 0.0f;
    }
    public void CloseMenu()
    {
        Save_Float("Volume", VolumeSlider.value);
        foreach (GameObject panel in menuPanel)
        {
            panel.SetActive(false);
        }
        player.GetComponent<PlayerInput>().MouseLock = true;
        Time.timeScale = 1.0f;
    }
    public void SwitchtoScene(int sceneId)
    {
        CloseMenu();
        SceneManager.LoadScene(sceneId);
    }
    private void Setup()
    {
        name = LevelManager.Instance.currentLevel.name;
        description = LevelManager.Instance.currentLevel.Description;

    }
    public void ChangeVolume()
    {
        AudioListener.volume = VolumeSlider.value;
    }
    private void Save_Float(string name, float value)
    {
        PlayerPrefs.SetFloat(name, value);
        PlayerPrefs.Save();
    }
  
    private float Load(string name)
    {
        return PlayerPrefs.GetFloat(name, 0.5f);
    }
}
