using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HFSMFramework;

public class PlayerMovement : MonoBehaviour
{
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float castDistance;
    [SerializeField] private float castRadius;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    [Header("Gravity")]
    [SerializeField] private float gravityForce;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTime;

    private bool GroundCollision
    {
        get
        {
            if (!Physics.SphereCast(rb.transform.position, castRadius, Vector3.down, out RaycastHit ground, castDistance, groundLayers)) {
                return false;
            }

            Vector3 dir = (ground.point - rb.transform.position).normalized;

            if (Physics.Raycast(rb.transform.position, dir, out RaycastHit nonInterpolated, dir.magnitude + 0.01f, groundLayers)) {
                if (Vector3.Angle(Vector3.up, nonInterpolated.normal) >= 90) {
                    GroundNormal = Vector3.up;
                }
                else {
                    GroundNormal = nonInterpolated.normal;
                }
            }

            float angle = Vector3.Angle(Vector3.up, GroundNormal);
            SlopeCollision = angle > 0 && angle < 90;
            GroundPoint    = ground.point;

            return true;
        }
    }

    private bool SlopeCollision;
    private Vector3 GroundNormal;
    private Vector3 GroundPoint;

    private Vector2 HorizontalVelocity;

    private StateMachine<PlayerMovement> fsm;

    private WalkingState Walking { get; set; }
    private JumpingState Jumping { get; set; }
    private FallingState Falling { get; set; }

    private void OnEnable()
    {
        // Since its not a monobehavior, it needs to be initialized with a reference to this class
        fsm = new(this);

        // You also need to do the same with all states, its a bit obnoxious, might try to find a way to improve the code at some point but the time is not now haha
        Walking = new(this);
        Jumping = new(this);
        Falling = new(this);

        // Initialize the FSM so that it has a reference to each state, and the transitions to and from each state
        fsm.AddTransitions(new()
        {
            /* STATE CURRENTLY IN */ /* STATE TRANSITIONING TO */ /* CONDITION FOR TRANSITION */
            // Syntax for bool expression follows the C# Func<bool> syntax, which is a bit odd, but it looks like this: () => { EXPRESSION }
            new(Walking, Jumping, () => Input.GetKeyDown(KeyCode.Space)),
            new(Jumping, Falling, () => true),
            new(Falling, Walking, () => GroundCollision),
        });

        // Since the fsm initial state is not assigned at addition, you have to do it manually. Might change it
        // Initialize HFSM with state
        fsm.Start(Walking);
    }

    void Update()
    {
        // Check the transitions, then update. You can invert this too or not update at all, either works. Hence why its seperated!
        // Updating calls the Update override function from the current state
        fsm.CheckTransitions();
        fsm.Update();
    }

    private void FixedUpdate()
    {
        // Fixed Update call calls the current state's fixed update function
        fsm.FixedUpdate();
    }

    private void Move()
    {
        float speed = Mathf.Max(moveSpeed, Mathf.Abs(new Vector2(rb.velocity.x, rb.velocity.z).magnitude));
        HorizontalVelocity = Vector2.MoveTowards(HorizontalVelocity, new Vector2(transform.forward.x, transform.forward.z) * speed, Time.fixedDeltaTime * acceleration);

        Vector3 set = new(HorizontalVelocity.x, rb.velocity.y, HorizontalVelocity.y);

        if (SlopeCollision && fsm.CurrentState == Walking)
        {
            set.y = 0;
            set = Quaternion.FromToRotation(Vector3.up, GroundNormal) * set;
        }

        rb.velocity = set;
    }

    // When creating a state, make sure you have it inherit the state class, but ensure you have the template type set to the PlayerMovement.
    // Sounds odd, but the reason you do this and provide the constructor below is because on construction, the base "context" has reference to all private variables from the PlayerMovement class passed in
    // TL;DR everything can be private :)
    private class WalkingState : State<PlayerMovement>
    {
        // Constructor, if you're making your own state just duplicate this and change the name from WalkingState to whatever it is you want it to be called :)
        public WalkingState(PlayerMovement context) : base(context) { }

        // Called when state is first created
        // Ensure its marked as "public override void" rather than "public void", else it will not function!
        public override void Enter()
        {
            base.Enter();
        }

        // Called every update call (done via fsm.Update())
        public override void Update()
        {
            base.Update();
        }

        // Called every fixed update call (done via fsm.FixedUpdate())
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        // Called when the state is changed
        public override void Exit()
        {
            base.Exit();
        }
    }

    // Same applies to this as above
    private class JumpingState : State<PlayerMovement>
    {
        public JumpingState(PlayerMovement context) : base(context) { }

        public override void FixedUpdate() {
            context.rb.AddForce(Vector3.down * context.gravityForce);
        }
    }

    private class FallingState : State<PlayerMovement>
    {
        public FallingState(PlayerMovement context) : base(context) { }

        public override void FixedUpdate() {
            context.rb.AddForce(Vector3.down * context.gravityForce);
        }
    }
}
