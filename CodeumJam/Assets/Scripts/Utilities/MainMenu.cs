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
    [SerializeField] private Image image;

    [Header("Cutscene")]
    [SerializeField] private Transform endPos;
    [SerializeField] private RotateAroundObject cam;
    [SerializeField] private float interpolateSpeed;

    private int Levelid = 0;

    // Start is called before the first frame update
    void Start()
    {
        ChangeScene(0);
        VolumeSlider.value = Load("Volume");

        image.sprite = levelArray[0].DisplaySprite;
    }

    // Update is called once per frame
    void Update()
    {
        LevelInfo();
    }

    public void ChangeLevel(int change)
    {
        if (change == 1)
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

        image.sprite = levelArray[Levelid].DisplaySprite;
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
        LevelText.text = levelArray[Levelid].DisplayName;
    }

    public void StartLevel()
    {
        StartCoroutine(IntroCutscene(Levelid + 1));
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

    private IEnumerator IntroCutscene(int sceneID)
    {
        foreach (GameObject obj in menuPanels)
        {
            obj.SetActive(false);
        }

        cam.enabled = false;
        Transform camTransform = cam.transform;
        Vector3 posVel = Vector3.zero;
        Vector3 dirVel = Vector3.zero;

        while (Vector3.Distance(camTransform.position, endPos.position) > 0.01f)
        {
            camTransform.position = Vector3.SmoothDamp(camTransform.position, endPos.position, ref posVel, interpolateSpeed);
            camTransform.forward = Vector3.SmoothDamp(camTransform.forward, endPos.forward, ref dirVel, interpolateSpeed);

            yield return null;
        }

        SceneManager.LoadScene(sceneID);
    }
}
