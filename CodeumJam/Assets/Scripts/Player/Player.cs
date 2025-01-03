using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Player Components")]
    [SerializeField] private   PlayerMovement  playerMovement;
    [SerializeField] private   PlayerCamera    playerCamera;
    [SerializeField] protected PlayerInput     playerInput;
    [SerializeField] private   PlayerViewmodel playerViewmodel;

    [Header("Player Physics")]
    [SerializeField] private Rigidbody       rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private SphereCollider  sphereCollider;
    [SerializeField] private LayerMask       groundLayer;
    [SerializeField] private LayerMask       playerLayer;

    [Header("Player Camera")]
    [SerializeField] private Camera cam;

    private void Awake()
    {
        Instance = this;

        playerMovement.SetPlayer(this);
        playerCamera.SetPlayer(this);
        playerInput.SetPlayer(this);
        playerViewmodel.SetPlayer(this);

        playerViewmodel.TurnOff();
    }

    public void AllowMovement(bool allow)
    {
        playerMovement.enabled    = allow;
        playerViewmodel.canRotate = allow;
    }

    public void CutsceneCam(bool allow)
    {
        playerCamera.canRotate = allow;
        playerCamera.SetParent(allow);
    }

    public void SetSpawn(Vector3 pos) => rb.transform.position = pos;

    public void Respawn() => CutsceneManager.Instance.TriggerCutscene(CutsceneManager.CutsceneType.Respawn);

    public void Explode() {
        playerViewmodel.Explode();
        AllowMovement(false);
        rb.velocity = Vector3.zero;
    }

    public PlayerMovement GetMovement() {
        return playerMovement;
    }

    public PlayerInput GetInput() {
        return playerInput;
    }

    public PlayerCamera GetCamera() {
        return playerCamera;
    }

    public class PlayerComponent : MonoBehaviour
    {
        protected Player player;
        public void SetPlayer(Player player) => this.player = player;

        public PlayerMovement  PlayerMovement  => player.playerMovement;
        public PlayerCamera    PlayerCamera    => player.playerCamera;
        public PlayerInput     PlayerInput     => player.playerInput;
        public PlayerViewmodel PlayerViewmodel => player.playerViewmodel;

        public Rigidbody       rb              => player.rb;
        public CapsuleCollider CapsuleCollider => player.capsuleCollider;
        public SphereCollider  SphereCollider  => player.sphereCollider;
        public LayerMask       GroundLayer     => player.groundLayer;
        public LayerMask       PlayerLayer     => player.playerLayer;

        public Camera Camera => player.cam;
    }
}
