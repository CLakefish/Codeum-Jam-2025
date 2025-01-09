using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCollider : MonoBehaviour
{
    private Respawn respawn;

    private void Awake()                        => respawn = FindObjectOfType<Respawn>();
    private void OnTriggerEnter(Collider other) => respawn.HandleRespawn(other);
}
