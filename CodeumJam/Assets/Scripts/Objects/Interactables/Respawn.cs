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
    [SerializeField] private Vector3 spawnPosition;

    public Vector3 SpawnPosition => spawnPosition;
    public void SetSpawnPos(Vector3 pos) => spawnPosition = pos;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(SpawnPosition, 0.1f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        switch (other.gameObject.tag)
        {
            case "Player":
                LevelManager.Instance.ResetAll();
                other.attachedRigidbody.position = spawnPosition;
                break;

            case "Collidable":
                other.gameObject.SetActive(false);
                break;

            default:
                other.transform.position = spawnPosition;
                break;
        }
    }
}
