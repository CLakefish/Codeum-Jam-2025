using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Custom/Level Data")]
public class LevelScriptableObject : ScriptableObject
{
    [Header("Scene Name")]
    [SerializeField] private string sceneName;

    [Header("Display")]
    [SerializeField] private Sprite displaySprite;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;

    public Sprite DisplaySprite => displaySprite;
    public string DisplayName   => displayName;
    public string Description   => description;

    public void ChangeScene() => SceneManager.LoadScene(sceneName);
}
