using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewmodel : Player.PlayerComponent
{
    [Header("Viewmodels")]
    [SerializeField] private Transform viewmodel;
    [SerializeField] private GameObject snowMan;
    [SerializeField] private GameObject snowManHead;
    [SerializeField] private GameObject snowBall;
    [SerializeField] private GameObject shadow;
    [SerializeField] public Animator snowManAnimator;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float walkTrailSize;
    [SerializeField] private float rollTrailSize;

    [Header("Particles")]
    [SerializeField] private ParticleSystem landParticle;
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem hitWallParticle;
    [SerializeField] private ParticleSystem explodeParticle;

    [Header("Smoothing")]
    [SerializeField] private float viewmodelSmoothing;
    [SerializeField] private float snowManShadowSize;
    [SerializeField] private float snowBallShadowSize;
    [SerializeField] private float snowBallSizeSmoothing;
    private Coroutine swapEffect;

    [Header("Hats")]
    [SerializeField] private List<Hat> hats = new();
    private Hat currentHat;
    private GameObject currentHatObject;

    public GameObject Viewmodel {
        get {
            return IsSnowman ? snowMan : snowBall;
        }
    }

    public bool IsSnowman => isSnowman;
    public bool canRotate = true;

    private const float Z_FIGHTING_PUSH = 0.015f;

    private Quaternion prevRollRotation = Quaternion.identity;
    private float viewmodelVel = 0;
    private bool isSnowman = true;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("hat"))
        {
            EquipHat(PlayerPrefs.GetString("hat"));
        }
    }

    private void Start()
    {
        snowBall.transform.localScale = Vector3.zero;

        PlayerMovement.OnJump += () => {
            snowManAnimator.SetBool("Jump", true);
            Instantiate(jumpParticle, new Vector3(rb.position.x, PlayerMovement.GroundPoint.y, rb.position.z), Quaternion.identity);
        };

        PlayerMovement.HitGround += () => {
            snowManAnimator.SetBool("Grounded", true);
            Instantiate(landParticle, new Vector3(rb.position.x, PlayerMovement.GroundPoint.y, rb.position.z), Quaternion.identity);
        };

        PlayerMovement.HitWall += () => {
            Instantiate(hitWallParticle, rb.transform.position, Quaternion.identity);
        };
    }

    private void Update()
    {
        if (Physics.Raycast(rb.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, GroundLayer))
        {
            shadow.transform.position   = hit.point + (Vector3.up * Z_FIGHTING_PUSH);
            shadow.transform.forward    = hit.normal;
            shadow.transform.localScale = Vector3.one * (IsSnowman ? snowManShadowSize : snowBallShadowSize);
        }

        viewmodel.transform.position = rb.transform.position;

        if (!IsSnowman || !canRotate) return;

        float yAngleOffset = Mathf.Atan2(Camera.transform.forward.z, Camera.transform.forward.x) * Mathf.Rad2Deg - (90f - player.transform.eulerAngles.y);

        if (PlayerInput.Inputting) {
            float newAngle    = (Mathf.Atan2(PlayerInput.Inputs.normalized.x, PlayerInput.Inputs.normalized.y) * Mathf.Rad2Deg) - yAngleOffset;
            float interpAngle = Mathf.SmoothDampAngle(Viewmodel.transform.localEulerAngles.y, newAngle, ref viewmodelVel, viewmodelSmoothing);
            Viewmodel.transform.localEulerAngles = new Vector3(0, interpAngle, 0);
        }
    }

    private void FixedUpdate()
    {
        trail.emitting   = PlayerMovement.GroundCollision && rb.velocity.magnitude >= 1;
        trail.startWidth = isSnowman ? walkTrailSize : rollTrailSize;

        if (PlayerMovement.GroundCollision)
        {
            trail.transform.position = new Vector3(trail.transform.position.x, PlayerMovement.GroundPoint.y + 0.1f, trail.transform.position.z);
            trail.transform.forward  = -PlayerMovement.GroundNormal;
        }

        if (IsSnowman) {
            snowManAnimator.SetBool("Jump", PlayerMovement.IsJumping);
            snowManAnimator.SetBool("Grounded", PlayerMovement.GroundCollision);
            snowManAnimator.SetBool("Falling", !PlayerMovement.GroundCollision);
        }
    }

    public void Rolling(bool active)
    {
        rb.freezeRotation = !active;
        isSnowman         = !active;

        if (!active) {
            rb.angularVelocity = Vector3.zero;
            rb.rotation        = prevRollRotation;

            Viewmodel.transform.localEulerAngles = new Vector3(0, Viewmodel.transform.localEulerAngles.y, 0);

            Physics.SyncTransforms();
        }
        else {
            prevRollRotation = rb.rotation;
        }

        if (swapEffect != null) StopCoroutine(swapEffect);
        swapEffect = StartCoroutine(SwapEffect(active));
    }

    public void TurnOff()
    {
        snowMan.SetActive(false);
        snowBall.SetActive(false);
        shadow.SetActive(false);
    }

    public void Respawn()
    {
        snowBall.SetActive(false);
        snowMan.SetActive(true);
        shadow.SetActive(true);
        snowManAnimator.SetTrigger("Spawn");
    }

    public void Explode()
    {
        TurnOff();
        trail.emitting = false;

        Instantiate(explodeParticle, rb.transform.position, Quaternion.identity);
    }

    private IEnumerator SwapEffect(bool active)
    {
        snowBall.SetActive(true);
        snowMan.SetActive(true);

        Vector3 ballVel         = Vector3.zero;
        Vector3 desiredBallSize = Vector3.one * (active ? 1 : 0);

        while (Vector3.Distance(snowBall.transform.localScale, desiredBallSize) > 0.005f)
        {
            snowBall.transform.localScale = Vector3.SmoothDamp(snowBall.transform.localScale, desiredBallSize, ref ballVel, snowBallSizeSmoothing);
            yield return null;
        }

        snowMan.SetActive(!active);
        snowBall.SetActive(active);
    }

    public void EquipHat(string ID)
    {
        if (currentHat != null && ID == currentHat.ID) return;

        if (currentHatObject != null) Destroy(currentHatObject);

        var indexedHat = hats.Find(w => w.ID == ID);

        currentHatObject = Instantiate(indexedHat.HatObject, snowManHead.transform);
        currentHatObject.transform.localEulerAngles = new Vector3(90, 0, 0);
        currentHatObject.transform.localPosition   -= Vector3.forward * 0.2f;
        currentHat       = indexedHat;

        PlayerPrefs.SetString("hat", currentHat.ID);
    }
}
