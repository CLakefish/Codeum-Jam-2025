using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerMovement  playerMovement;
    [SerializeField] private PlayerCamera    playerCamera;
    [SerializeField] protected PlayerInput     playerInput;
    [SerializeField] private PlayerViewmodel playerViewmodel;

    [Header("Player Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private SphereCollider  sphereCollider;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Player Camera")]
    [SerializeField] private Camera cam;

    private void Awake()
    {
        playerMovement.SetPlayer(this);
        playerCamera.SetPlayer(this);
        playerInput.SetPlayer(this);
        playerViewmodel.SetPlayer(this);
    }

    public void AllowMovement(bool allow)
    {
        playerMovement.enabled    = allow;
        playerCamera.canRotate    = allow;
        playerCamera.SetParent(allow);
        playerViewmodel.canRotate = allow;
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
