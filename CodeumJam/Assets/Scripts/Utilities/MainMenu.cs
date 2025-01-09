using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] Slider VolumeSlider;
    [SerializeField] private GameObject[] menuPanels;
    [SerializeField] private LevelScriptableObject[] levelArray;
    [SerializeField] private TextMeshProUGUI LevelText;


    private int Levelid = 0;

    // Start is called before the first frame update
    void Start()
    {
        
        ChangeScene(0);
        VolumeSlider.value = Load("Volume");
    }

    // Update is called once per frame
    void Update()
    {
        LevelInfo();
    }

    public void ChangeLevel(int change)
    {
        if(change == 1)
        {
            if(Levelid == levelArray.Length-1)
            {
                Levelid = 0;
            }
            else
            {
                Levelid++;
            }
        }
        else if(change == -1)
        {
            if (Levelid == 0)
            {
                Levelid = levelArray.Length - 1;
            }
            else
            {
                Levelid--;
            }
        }
    }
    public void ChangeScene(int Sceneid)
    {
        Save_Float("Volume", VolumeSlider.value);
        foreach (GameObject obj in menuPanels)
        {
            obj.SetActive(false);
        }
        menuPanels[Sceneid].SetActive(true);
    }

    public void LevelInfo()
    {
        LevelText.text = levelArray[Levelid].name;
    }

    public void StartLevel()
    {
        SceneManager.LoadScene(Levelid+1);
    }
    public void ChangeVolume()
    {
        AudioListener.volume = VolumeSlider.value;
    }
    public void Save_Float(string name, float value)
    {
        PlayerPrefs.SetFloat(name, value);
        PlayerPrefs.Save();
    }

    public float Load(string name)
    {
        return PlayerPrefs.GetFloat(name, 0.5f);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
