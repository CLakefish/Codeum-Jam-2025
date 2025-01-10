using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hat")]
public class Hat : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private GameObject hatObject;

    public GameObject HatObject => hatObject;
    public string     ID        => id;
}
