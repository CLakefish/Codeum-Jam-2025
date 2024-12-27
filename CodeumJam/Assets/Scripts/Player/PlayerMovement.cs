using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HFSMFramework;

public class PlayerMovement : MonoBehaviour
{
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float castRadius;
    [SerializeField] private float fallCastDist, groundCastDist;
    [SerializeField] private float floorStickTime;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCol;
    [SerializeField] private SphereCollider sphereCol;
    [SerializeField] private Transform cam;
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private PlayerCamera PlayerCamera;

    [Header("Gravity")]
    [SerializeField] private float gravityForce;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float momentumConserveTimeGround;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferTime;

    [Header("Rolling")]
    [SerializeField] private float rollRotationSpeed;
    [SerializeField] private float rollJumpForce;

    private readonly float CORRECTION_DIST       = 1.75f;
    private readonly float CORRECTION_RAD_REDUCT = 4.0f;
    private readonly float FLOOR_STICK_THRESHOLD = 0.05f;

    private Vector3 MoveDir
    {
        get
        {
            Vector3 forwardNoY = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
            Vector3 rightNoY   = new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
            return forwardNoY * PlayerInput.Inputs.normalized.y + rightNoY * PlayerInput.Inputs.normalized.x;
        }
    }

    private bool GroundCollision  { get; set; }
    private bool SlopeCollision   { get; set; }
    private bool WalkingOffGround { get; set; }
    private Vector3 GroundNormal  { get; set; }
    private Vector3 GroundPoint   { get; set; }

    private Vector2 HorizontalVelocity;
    private float jumpBuffer = 0;


    private StateMachine<PlayerMovement> fsm;
    private WalkingState Walking   { get; set; }
    private JumpingState Jumping   { get; set; }
    private FallingState Falling   { get; set; }
    private RollingState Rolling   { get; set; }

    private void OnEnable()
    {
        // Since its not a monobehavior, it needs to be initialized with a reference to this class
        fsm = new(this);

        // You also need to do the same with all states, its a bit obnoxious, might try to find a way to improve the code at some point but the time is not now haha
        Walking  = new(this);
        Jumping  = new(this);
        Falling  = new(this);
        Rolling  = new(this);

        // Initialize the FSM so that it has a reference to each state, and the transitions to and from each state
        fsm.AddTransitions(new()
        {
            /* STATE CURRENTLY IN */ /* STATE TRANSITIONING TO */ /* CONDITION FOR TRANSITION */
            // Syntax for bool expression follows the C# Func<bool> syntax, which is a bit odd, but it looks like this: () => { EXPRESSION }
            new(Walking, Jumping,  () => PlayerInput.Jump.Pressed || jumpBuffer > 0),
            new(Walking, Falling,  () => !GroundCollision),
                                   
            new(Jumping, Falling,  () => fsm.Duration > 0 && rb.velocity.y < 0),
                                   
            new(Falling, Walking,  () => GroundCollision),
            new(Falling, Jumping,  () => PlayerInput.Jump.Pressed && fsm.PreviousState == Walking && fsm.Duration <= coyoteTime),

            new(null,    Rolling,  () => PlayerInput.Roll),
            new(Rolling, Falling,  () => !PlayerInput.Roll),
        });

        // Since the fsm initial state is not assigned at addition, you have to do it manually. Might change it
        // Initialize HFSM with state
        fsm.Start(Walking);
    }

    void Update()
    {
        jumpBuffer -= Time.deltaTime;

        // Check the transitions, then update. You can invert this too or not update at all, either works. Hence why its seperated!
        // Updating calls the Update override function from the current state
        fsm.CheckTransitions();
        fsm.Update();
    }

    private void FixedUpdate()
    {
        CheckGroundCollisions();

        // Fixed Update call calls the current state's fixed update function
        fsm.FixedUpdate();
    }

    private void OnGUI()
    {
        fsm.OnGUI();

        GUILayout.BeginArea(new Rect(10, 150, 800, 200));

        string current = $"Current Velocity: { rb.velocity }\nCurrent Magnitude: { rb.velocity.magnitude }\nRotational Velocity: {rb.angularVelocity}";
        GUILayout.Label($"<size=15>{current}</size>");
        GUILayout.EndArea();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.transform.position + (Vector3.down * groundCastDist), castRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rb.transform.position + (Vector3.down * fallCastDist), castRadius);
    }

    private void SetY(float y)  => rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
    private void ApplyGravity() => rb.velocity -= gravityForce * Time.fixedDeltaTime * Vector3.up;

    private void Move(bool maintainMomentum = true)
    {
        float speed = maintainMomentum
            ? Mathf.Max(moveSpeed, new Vector2(rb.velocity.x, rb.velocity.z).magnitude)
            : moveSpeed;

        HorizontalVelocity = Vector2.MoveTowards(HorizontalVelocity, new Vector2(MoveDir.x, MoveDir.z) * speed, Time.fixedDeltaTime * acceleration);

        Vector3 set = new(HorizontalVelocity.x, rb.velocity.y, HorizontalVelocity.y);

        if (fsm.CurrentState == Walking && SlopeCollision) {
            set.y = 0;
            set = Quaternion.FromToRotation(Vector3.up, GroundNormal) * set;
        }

        rb.velocity = set;
    }

    private void CheckGroundCollisions()
    {
        float dist = fsm.CurrentState == Walking ? groundCastDist : fallCastDist;

        if (!Physics.SphereCast(rb.transform.position, castRadius, Vector3.down, out RaycastHit ground, dist, groundLayers))
        {
            GroundCollision = false;
            return;
        }

        Vector3 dir = new Vector3(Mathf.Sign(rb.velocity.x), 0, Mathf.Sign(rb.velocity.z)).normalized * (castRadius / CORRECTION_RAD_REDUCT);

        if (Physics.Raycast(rb.transform.position + dir, Vector3.down, out RaycastHit nonInterpolated, CORRECTION_DIST, groundLayers))
        {
            WalkingOffGround = false;

            if (Vector3.Angle(Vector3.up, nonInterpolated.normal) < 90) {
                GroundNormal = ground.normal;
            }
            else {
                GroundNormal = Vector3.up;
            }
        }
        else
        {
            WalkingOffGround = true;
            GroundNormal = Vector3.up;
        }

        float angle     = Vector3.Angle(Vector3.up, GroundNormal);
        SlopeCollision  = angle > 0 && angle < 90;
        GroundCollision = true;
        GroundPoint     = ground.point;
    }

    // When creating a state, make sure you have it inherit the state class, but ensure you have the template type set to the PlayerMovement.
    // Sounds odd, but the reason you do this and provide the constructor below is because on construction, the base "context" has reference to all private variables from the PlayerMovement class passed in
    // TL;DR everything can be private :)
    private class WalkingState : State<PlayerMovement>
    {
        // Variables can be put in the states, they cannot be accessed by the hfsm!
        private Vector3 stickVel;

        // Constructor, if you're making your own state just duplicate this and change the name from WalkingState to whatever it is you want it to be called :)
        public WalkingState(PlayerMovement context) : base(context) { }

        // Called when state is first created
        // Ensure its marked as "public override void" rather than "public void", else it will not function!
        public override void Enter() {
            if (context.rb.velocity.y < 0) context.SetY(0);
        }

        // Called every update call (done via fsm.Update())
        public override void Update() {
            context.PlayerCamera.SetBoxBoundBottom();
        }

        // Called every fixed update call (done via fsm.FixedUpdate())
        public override void FixedUpdate()
        {
            float halfSize    = 1;
            float yPos        = context.rb.transform.position.y - halfSize - context.GroundPoint.y;
            float yCheck      = context.FLOOR_STICK_THRESHOLD;
            Vector3 targetPos = new(context.rb.transform.position.x, context.GroundPoint.y + halfSize, context.rb.transform.position.z);

            if (yPos > yCheck) {
                Vector3 currentPos = Vector3.SmoothDamp(context.rb.transform.position, targetPos, ref stickVel, context.floorStickTime, Mathf.Infinity, Time.fixedDeltaTime);
                context.rb.MovePosition(currentPos);
            }

            if (!context.SlopeCollision && !context.WalkingOffGround) context.SetY(0);
            context.Move(context.fsm.Duration < context.momentumConserveTimeGround);
        }

        // Called when the state is changed
        public override void Exit() {
        }
    }

    // Same applies to this as above
    private class JumpingState : State<PlayerMovement>
    {
        public JumpingState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            context.SetY(context.jumpForce);
            context.jumpBuffer = 0;
        }

        public override void FixedUpdate() {
            context.Move();
            context.ApplyGravity();
        }
    }

    private class FallingState : State<PlayerMovement>
    {
        public FallingState(PlayerMovement context) : base(context) { }

        public override void Update()
        {
            if (context.PlayerInput.Jump.Pressed) {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate() {
            context.Move();
            context.ApplyGravity();
        }
    }

    private class RollingState : State<PlayerMovement>
    {
        public RollingState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            context.capsuleCol.enabled = false;
            context.sphereCol.enabled  = true;
            context.rb.freezeRotation  = false;
        }

        public override void Update()
        {
            if (context.PlayerInput.Jump.Pressed) context.jumpBuffer = context.jumpBufferTime;

            if (context.GroundCollision)
            {
                context.PlayerCamera.SetBoxBoundBottom();

                if (context.jumpBuffer > 0)
                {
                    context.jumpBuffer = 0;
                    context.SetY(context.rollJumpForce);
                }
            }

            if (context.PlayerInput.Inputting)
            {
                Vector3 currentVel  = context.rb.velocity;
                Quaternion rotation = Quaternion.FromToRotation(currentVel.normalized, context.MoveDir);
                Vector3 rotatedVel  = rotation * currentVel;
                rotatedVel.y = 0;

                Vector3 desiredVel  = Vector3.MoveTowards(currentVel, rotatedVel, Time.deltaTime * context.rollRotationSpeed);
                desiredVel.y = context.rb.velocity.y;
                context.rb.velocity = desiredVel;
            }
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }

        public override void Exit()
        {
            context.capsuleCol.enabled = true;
            context.rb.freezeRotation  = true;
            context.sphereCol.enabled  = false;

            context.HorizontalVelocity = new(context.rb.velocity.x, context.rb.velocity.z);

            context.rb.transform.rotation = Quaternion.Euler(0, context.rb.transform.eulerAngles.y, 0);
        }
    }
}
