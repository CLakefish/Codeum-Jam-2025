using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HFSMFramework;

public class PlayerMovement : Player.PlayerComponent
{
    [Header("Ground Collisions")]
    [SerializeField] private float castRadius;
    [SerializeField] private float fallCastDist, groundCastDist;

    [Header("Wall Collisions")]
    [SerializeField] private int   wallCastIncrement;
    [SerializeField] private float wallCastDistance;

    [Header("Rolling Collision")]
    [SerializeField] private float rollBounceCastDistance;
    [SerializeField] private float rollCastInterpolateSpeed;

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
    [SerializeField] private float rollBoostForce;
    [SerializeField] private float rollIdleSpeed;
    [SerializeField] private float rollMinBounceAngle;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpTime;
    [SerializeField] private float wallJumpAngle;

    private readonly float CORRECTION_DIST           = 1.75f;
    private readonly float CORRECTION_RAD_REDUCT     = 4.0f;
    private readonly float FLOOR_STICK_THRESHOLD     = 0.05f;
    private readonly float FLOOR_STICK_INTERPOLATION = 0.1f;
    private readonly float JUMP_GRACE_TIME           = 0.1f;
    private readonly float CAMERA_CORRECT_THRESHOLD  = 0.25f;

    private Vector3 MoveDir {
        get {
            Vector3 forwardNoY = new Vector3(Camera.transform.forward.x, 0, Camera.transform.forward.z).normalized;
            Vector3 rightNoY   = new Vector3(Camera.transform.right.x, 0, Camera.transform.right.z).normalized;
            return (forwardNoY * PlayerInput.Inputs.normalized.y + rightNoY * PlayerInput.Inputs.normalized.x).normalized;
        }
    }

    private Vector3 MomentumNoY {
        get {
            return new(rb.velocity.x, 0, rb.velocity.z);
        }
    }

    private StateMachine<PlayerMovement> fsm;
    private WalkingState  Walking  { get; set; }
    private JumpingState  Jumping  { get; set; }
    private FallingState  Falling  { get; set; }
    private WallJumpState WallJump { get; set; }


    private RollingStateMachine Rolling { get; set; }
    private RollingWalkState RollWalk   { get; set; }
    private RollingJumpState RollJump   { get; set; }
    private RollingFallState RollFall   { get; set; }

    public bool IsRolling { 
        get {
            return fsm.CurrentState == Rolling;
        }
    }

    public bool IsJumping {
        get {
            return fsm.CurrentState == Jumping;
        }
    }

    public bool  GroundCollision  { get; set; }
    private bool SlopeCollision   { get; set; }
    private bool WalkingOffGround { get; set; }
    private bool WallCollision    { get; set; }
    public Vector3  GroundNormal     { get; set; }
    public Vector3  GroundPoint      { get; set; }
    private Vector3 WallNormal       { get; set; }

    private Vector3 prevGroundPoint;


    public event System.Action HitGround;
    public event System.Action OnJump;
    public event System.Action HitWall;

    private Vector2 HorizontalVelocity;
    private float jumpBuffer = 0;

    private void OnEnable()
    {
        // Since its not a monobehavior, it needs to be initialized with a reference to this class
        fsm = new(this);

        // You also need to do the same with all states, its a bit obnoxious, might try to find a way to improve the code at some point but the time is not now haha
        Walking  = new(this);
        Jumping  = new(this);
        Falling  = new(this);
        WallJump = new(this);

        Rolling  = new(this, fsm);
        RollWalk = new(this);
        RollJump = new(this);
        RollFall = new(this);

        Rolling.AddTransitions(new()
        {
            new(RollWalk, RollFall, () => !GroundCollision),
            new(RollWalk, RollJump, () => PlayerInput.Jump.Pressed || jumpBuffer > 0),
            new(RollJump, RollFall, () => fsm.Duration > 0 && rb.velocity.y < 0),
            new(RollJump, RollFall, () => GroundCollision && fsm.Duration > JUMP_GRACE_TIME),
            new(RollFall, RollJump, () => PlayerInput.Jump.Pressed && Rolling.PreviousState == RollWalk && Rolling.Duration <= coyoteTime),
            new(RollFall, RollWalk, () => GroundCollision),
        });

        Rolling.SetStartState(RollWalk);

        // Initialize the FSM so that it has a reference to each state, and the transitions to and from each state
        fsm.AddTransitions(new()
        {
            /* STATE CURRENTLY IN */ /* STATE TRANSITIONING TO */ /* CONDITION FOR TRANSITION */
            // Syntax for bool expression follows the C# Func<bool> syntax, which is a bit odd, but it looks like this: () => { EXPRESSION }
            new(Walking, Jumping,  () => PlayerInput.Jump.Pressed || jumpBuffer > 0),
            new(Walking, Falling,  () => !GroundCollision),
                                   
            new(Jumping, Falling,  () => fsm.Duration > 0 && rb.velocity.y < 0),
            new(Jumping, Walking,  () => fsm.Duration > JUMP_GRACE_TIME && GroundCollision),
            new(Jumping, WallJump, () => (PlayerInput.Jump.Pressed || jumpBuffer > 0) && WallCollision && fsm.Duration > 0),
                                   
            new(Falling, Walking,  () => GroundCollision && fsm.Duration > 0.1f),
            new(Falling, Jumping,  () => PlayerInput.Jump.Pressed && fsm.PreviousState == Walking && fsm.Duration <= coyoteTime),
            new(Falling, WallJump, () => PlayerInput.Jump.Pressed && WallCollision && !GroundCollision),

            new(null,    Rolling,  () => PlayerInput.Roll),
            new(Rolling, Falling,  () => !PlayerInput.Roll),

            new(WallJump, Falling, () => fsm.Duration > wallJumpTime),
            new(WallJump, Walking, () => GroundCollision && fsm.Duration > 0),
        });

        // Since the fsm initial state is not assigned at addition, you have to do it manually. Might change it
        // Initialize HFSM with state
        fsm.SetStartState(Falling);
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
        CheckWallCollisions();

        // Fixed Update call calls the current state's fixed update function
        fsm.FixedUpdate();
    }

    #region Debugging

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
        Rigidbody rb = GetComponentInChildren<Rigidbody>();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.transform.position + (Vector3.down * groundCastDist), castRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rb.transform.position + (Vector3.down * fallCastDist), castRadius);
    }

    #endregion

    #region Movement

    private void SetY(float y)  => rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
    private void ApplyGravity() => rb.velocity -= gravityForce * Time.fixedDeltaTime * Vector3.up;
    private void Move(bool maintainMomentum = true) {
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

    #endregion

    public void Launch(Vector3 force) {
        rb.velocity += force;
        SetY(force.y);
        HorizontalVelocity += new Vector2(force.x, force.z);

        if (fsm.CurrentState != Rolling) {
            fsm.ChangeState(Falling);
        }

        GroundCollision = false;
    }

    public void Respawn() {
        fsm.ChangeState(Walking);
        rb.velocity        = Vector3.zero;
        HorizontalVelocity = Vector2.zero;
    }

    private void CheckGroundCollisions() {
        float dist = fsm.CurrentState == Walking ? groundCastDist : fallCastDist;

        if (!Physics.SphereCast(rb.transform.position, castRadius, Vector3.down, out RaycastHit ground, dist, GroundLayer)) {
            GroundCollision = false;
            return;
        }

        Vector3 dir = new Vector3(Mathf.Sign(rb.velocity.x), 0, Mathf.Sign(rb.velocity.z)).normalized * (castRadius / CORRECTION_RAD_REDUCT);

        if (Physics.Raycast(rb.transform.position + dir, Vector3.down, out RaycastHit nonInterpolated, CORRECTION_DIST, GroundLayer)) {
            WalkingOffGround = false;

            if (Vector3.Angle(Vector3.up, nonInterpolated.normal) < 90) {
                GroundNormal = ground.normal;
            }
            else {
                GroundNormal = Vector3.up;
            }
        }
        else {
            WalkingOffGround = true;
            GroundNormal = Vector3.up;
        }

        float angle     = Vector3.Angle(Vector3.up, GroundNormal);
        SlopeCollision  = angle > 0 && angle < 90;
        GroundPoint     = ground.point;
        GroundCollision = true;
    }

    private void CheckWallCollisions() {
        float PISQR = Mathf.PI * 2.0f / wallCastIncrement;

        for (int i = 0; i < wallCastIncrement; i++) {
            Vector3 dir = new Vector3(Mathf.Cos(PISQR * i), 0, Mathf.Sin(PISQR * i)).normalized;
            Debug.DrawRay(rb.transform.position, dir);

            if (Physics.Raycast(rb.transform.position, dir, out RaycastHit hit, wallCastDistance, GroundLayer)) {
                WallCollision = true;
                WallNormal    = hit.normal;
                return;
            }
        }

        WallCollision = false;
    }

    // When creating a state, make sure you have it inherit the state class, but ensure you have the template type set to the PlayerMovement.
    // Sounds odd, but the reason you do this and provide the constructor below is because on construction, the base "context" has reference to all private variables from the PlayerMovement class passed in
    // TL;DR everything can be private :)
    private class WalkingState : State<PlayerMovement>
    {
        private Vector3 vel;
        // Variables can be put in the states, they cannot be accessed by the hfsm!
        // Constructor, if you're making your own state just duplicate this and change the name from WalkingState to whatever it is you want it to be called :)
        public WalkingState(PlayerMovement context) : base(context) { }

        public override void Enter() {
            context.HitGround?.Invoke();
            context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.positionSmoothing);
        }

        // Called when state is first created
        // Ensure its marked as "public override void" rather than "public void", else it will not function!
        // Called every update call (done via fsm.Update())
        public override void Update() {
            context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.positionSmoothing);
        }

        // Called every fixed update call (done via fsm.FixedUpdate())
        public override void FixedUpdate() {
            float halfSize    = 1;
            float yPos        = context.rb.transform.position.y - halfSize - context.GroundPoint.y;
            float yCheck      = context.FLOOR_STICK_THRESHOLD;

            if (yPos > yCheck && !context.WalkingOffGround) {
                Vector3 targetPos  = new(context.rb.transform.position.x, context.GroundPoint.y + halfSize, context.rb.transform.position.z);
                Vector3 currentPos = Vector3.SmoothDamp(context.rb.position, targetPos, ref vel, context.FLOOR_STICK_INTERPOLATION);
                context.rb.MovePosition(currentPos);
            }

            if (!context.SlopeCollision && !context.WalkingOffGround) {
                context.SetY(0);
            }
            context.Move(context.fsm.Duration < context.momentumConserveTimeGround);
        }

        // Called when the state is changed
        public override void Exit() { }
    }

    // Same applies to this as above
    private class JumpingState : State<PlayerMovement>
    {
        public JumpingState(PlayerMovement context) : base(context) { }

        public override void Enter() {
            context.OnJump?.Invoke();
            context.SetY(context.jumpForce);
            context.jumpBuffer = 0;
            context.prevGroundPoint = context.GroundPoint;
        }

        public override void Update() {
            if (Physics.Raycast(context.rb.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, context.GroundLayer)) {
                if (hit.point.y - context.prevGroundPoint.y > context.CAMERA_CORRECT_THRESHOLD) {
                    context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.positionCorrectSpeed);
                }
            }
        }

        public override void FixedUpdate() {
            context.Move();
            context.ApplyGravity();
        }
    }

    private class FallingState : State<PlayerMovement>
    {
        public FallingState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            context.prevGroundPoint = context.GroundPoint;
        }

        public override void Update() {
            if (context.PlayerInput.Jump.Pressed) {
                context.jumpBuffer = context.jumpBufferTime;
            }

            if (Physics.Raycast(context.rb.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, context.GroundLayer))
            {
                if (hit.point.y - context.prevGroundPoint.y > context.CAMERA_CORRECT_THRESHOLD)
                {
                    context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.positionCorrectSpeed);
                }
            }
        }

        public override void FixedUpdate() {
            context.Move();
            context.ApplyGravity();
        }
    }

    private class WallJumpState : State<PlayerMovement>
    {
        public WallJumpState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            context.OnJump?.Invoke();

            float normalAngle  = Repeat(Vector3.SignedAngle(context.WallNormal, Vector3.forward, Vector3.up) + 90.0f);
            Vector3 reflect    = Vector3.Reflect(context.MoveDir, context.WallNormal).normalized;
            float reflectAngle = Repeat(Vector3.SignedAngle(reflect, Vector3.forward, Vector3.up) + 90.0f);

            float min = Repeat(normalAngle - context.wallJumpAngle);
            float max = Repeat(normalAngle + context.wallJumpAngle);

            if (!IsInRange(reflectAngle, min, max)) {
                bool minLarger = Mathf.Abs(reflectAngle - min) > Mathf.Abs(reflectAngle - max);
                reflectAngle = minLarger ? max : min;
            }

            Vector3 dir   = new Vector3(Mathf.Cos(Mathf.Deg2Rad * reflectAngle), 0, Mathf.Sin(Mathf.Deg2Rad * reflectAngle)).normalized;
            Vector3 force = dir.normalized * context.wallJumpForce;

            context.rb.velocity += force;
            context.HorizontalVelocity = new Vector2(context.rb.velocity.x, context.rb.velocity.z);

            context.SetY(context.wallJumpHeight);

            context.jumpBuffer = 0;
            context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.positionSmoothing);
        }

        public override void FixedUpdate() => context.ApplyGravity();

        private float Repeat(float value) {
            return Mathf.Repeat(value, 360.0f);
        }

        private bool IsInRange(float reflectedNormal, float min, float max) {
            if (min < max) {
                return reflectedNormal >= min && reflectedNormal <= max;
            }
            else {
                return reflectedNormal >= min || reflectedNormal <= max;
            }
        }
    }

    private class RollingStateMachine : StateMachine<PlayerMovement>
    {
        private Vector3 castDir;
        private Vector3 dirVel;

        public RollingStateMachine(PlayerMovement context, StateMachine<PlayerMovement> parent) : base(context, parent) { }

        public override void Enter() {
            context.CapsuleCollider.enabled = false;
            context.SphereCollider.enabled  = true;

            context.PlayerViewmodel.Rolling(true);

            if (context.MomentumNoY.magnitude <= context.moveSpeed) {
                Vector3 dir = context.PlayerInput.Inputting ? context.MoveDir : context.PlayerCamera.Camera.transform.forward;
                Vector3 boost = context.rollBoostForce * dir;

                context.rb.velocity += boost;
                context.HorizontalVelocity += new Vector2(boost.x, boost.z);

                context.PlayerCamera.AddFOV(context.PlayerCamera.rollBoostFOV);
            }

            castDir = context.MomentumNoY.normalized;

            base.Enter();
        }

        public override void Update() {
            CheckTransitions();

            context.PlayerCamera.SetBoxBoundBottom(context.PlayerCamera.ballSmoothing);
            context.PlayerCamera.AddFOV(context.rb.velocity.magnitude / context.PlayerCamera.rollFOVReduction);

            castDir = Vector3.SmoothDamp(castDir, context.MomentumNoY, ref dirVel, context.rollCastInterpolateSpeed);

            base.Update();
        }

        public override void FixedUpdate() {
            context.rb.angularVelocity = Quaternion.Euler(0, 90, 0) * context.rb.velocity;

            Collider[] colliders = Physics.OverlapSphere(context.rb.position, context.SphereCollider.radius + 0.01f, context.PlayerLayer);

            foreach (var collider in colliders) {
                if (collider.TryGetComponent<Launchable>(out Launchable p)) {
                    p.Launch(context.rb.position);
                }
            }

            if (Physics.SphereCast(context.rb.position, context.SphereCollider.radius - 0.1f, castDir.normalized, out RaycastHit hit, context.rollBounceCastDistance, context.GroundLayer))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);

                if (angle >= context.rollMinBounceAngle && !hit.collider.CompareTag("NoBounce"))
                {
                    Vector3 rotatedVel = Vector3.Reflect(context.rb.velocity, hit.normal);
                    rotatedVel.y = 0;
                    rotatedVel = rotatedVel.normalized * Mathf.Max(context.rb.velocity.magnitude, context.rollIdleSpeed);
                    context.rb.velocity = rotatedVel;

                    context.HitWall?.Invoke();
                }
            }

            if (context.PlayerInput.Inputting) {
                Vector3 currentVel = context.MomentumNoY.normalized;
                Vector3 targetVel  = context.MoveDir;
                Vector3 newVel     = Vector3.RotateTowards(currentVel, targetVel, context.rollRotationSpeed * Time.deltaTime, 0f);

                newVel   = newVel.normalized * context.MomentumNoY.magnitude;
                newVel.y = context.rb.velocity.y;
                context.rb.velocity = newVel;
            }

            context.ApplyGravity();

            base.FixedUpdate();
        }

        public override void Exit() {
            context.CapsuleCollider.enabled = true;
            context.SphereCollider.enabled = false;

            context.PlayerViewmodel.Rolling(false);

            context.HorizontalVelocity = new(context.rb.velocity.x, context.rb.velocity.z);
            context.PlayerCamera.ResetFOV();
        }
    }


    private class RollingWalkState : State<PlayerMovement>
    {
        public RollingWalkState(PlayerMovement context) : base(context) { }

        public override void Enter() {
            context.HitWall?.Invoke();
        }
    }

    private class RollingJumpState : State<PlayerMovement>
    {
        public RollingJumpState(PlayerMovement context) : base(context) { }

        public override void Enter() {
            context.OnJump?.Invoke();

            if (context.rb.velocity.y <= context.rollJumpForce) {
                context.SetY(context.rollJumpForce);
            }
            else {
                context.rb.velocity += Vector3.up;
            }
        }
    }

    private class RollingFallState : State<PlayerMovement>
    {
        public RollingFallState(PlayerMovement context) : base(context) { }

        public override void Update() {
            if (context.PlayerInput.Jump.Pressed) {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }
    }
}
