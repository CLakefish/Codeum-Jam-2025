using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Respawn))]
public class RespawnEditor : Editor
{
    private void OnSceneGUI()
    {
        Respawn spawn = (Respawn)target;

        spawn.SetSpawnPos(Handles.DoPositionHandle(spawn.SpawnPosition, Quaternion.identity));
    }
}

#endif

public class Respawn : MonoBehaviour
{
    public static Respawn Instance { get; private set; }


    [SerializeField] private Vector3 spawnPosition;

    public Vector3 SpawnPosition => spawnPosition;
    public void SetSpawnPos(Vector3 pos) => spawnPosition = pos;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(SpawnPosition, 0.1f);
    }

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start() {
        Player.Instance.SetSpawn(SpawnPosition);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        switch (other.gameObject.tag)
        {
            case "Player":
                RespawnPlayer();
                break;

            case "Collidable":
                other.gameObject.SetActive(false);
                break;
        }
    }

    public void RespawnPlayer()
    {
        LevelManager.Instance.ResetAll();
        Player.Instance.SetSpawn(SpawnPosition);
        Player.Instance.RespawnPlayer();
    }
}
