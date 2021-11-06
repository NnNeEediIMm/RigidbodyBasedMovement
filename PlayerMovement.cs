using System.Collections;
using UnityEngine;

//only for custom
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

/*Player movement for 
 Rigidbody made by 
  NnNeEediIMm!*/

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    //Movement
    Vector3 movement;
    float x, y;
    [Header("Main Movement")]
    [Range(1, 100)]
    public float speed = 10f;
    public float maxSpeed = 20;
    public bool slipperyMovement = false;
    public Transform orientation;

    //Input
    [Header("Input")]
    public KeyCode crouch = KeyCode.LeftShift;
    public KeyCode sprint = KeyCode.LeftControl;
    public KeyCode jump = KeyCode.Space;

    //gravity
    [Header("Gravity")]
    public LayerMask ground;
    public int gravityScale = 45;
    public float checkRadius = 1f;
    public Transform groundCheck;

    //Rigidbody
    protected Rigidbody rb;
    protected Collider coll;

    //jumping
    bool jumping;
    [Header("Jumping")]
    public float jumpForce = 1000f;

    //Crouching
    bool crouching;
    [Header("Crouching")]
    public bool canCrouch = true;
    public float standUpAfter = 1f;
    public float standUpTime = 1f;
    private float speedWhileCrouching;
    private float originalSize;

    float startTime;
    bool isSliding = false;
    float reducedSize = 0.5f;

    //Sprinting
    [Header("Sprinting")]
    public bool canSprint = true;
    private float sprintSpeed;
    private bool sprinting;
    private bool upSprinting;
    float defaultSpeed;

    //Physics
    [Header("Physics")]
    public bool useSlope = true, useStairs = true, noWallSticking = true;
    public float forceForward = 40f;
    public float forceUp = 60f;
    public LayerMask hittable;
    public float stairRadiusDown = 1f;
    public float stairRadiusUp = 0.7f;

    Vector3 slerpDirection;
    bool isUp, isDown;
    
    private void Awake()
    {
        rb = /*Rigidbody Component*/ GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //for crouching
        originalSize = transform.localScale.y;
        speedWhileCrouching = speed / 2;
        reducedSize = transform.localScale.y / 2;
        startTime = Time.time;

        //for spinting
        defaultSpeed = speed;
        sprintSpeed = defaultSpeed * 1.5f;

        //for wall chucking fix
        wallChucking();
    }

    private void rbFix()
    {
        //rb fixes
        rb.mass = 1f;
        rb.angularDrag = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        //movement
        movementInput();
        movementHelp();

        //mechanics
        Jump();
        Crouching();
        standUp();
        Sprint();

        //slopes 
        stairAndSlopeFix();

        //rb
        rbFix();
    }

    /// <summary>
    /// Main movement
    /// </summary>
    private void FixedUpdate()
    {
        //movement
        playerMovement();

        //gravity
        Gravity();
    }


    //Start of the gravity 
    public void Gravity() //player gravity
    {
        //one multipler for addforce
        float multiplerV = 100f;

        rb.AddForce(Vector3.down * Time.deltaTime * gravityScale * multiplerV);
    }
    //end of gravity

    /// <summary>
    /// Input for entire
    /// script!!
    /// </summary>
    public void movementInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        jumping = Input.GetKeyDown(jump);

        crouching = Input.GetKey(crouch);

        sprinting = Input.GetKey(sprint) && Input.GetKey(KeyCode.W);
        upSprinting = Input.GetKeyUp(sprint);
    }

    public void Jump()
    {
        if (isGrounded())
        {
            if (jumping)
            {
                rb.AddForce(transform.up * jumpForce / Physics.gravity.y / -2f, ForceMode.Impulse);
            }
        }
    }

    //crouching
    public void Crouching()
    {
        if (canCrouch)
        {
            if (crouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, reducedSize, transform.localScale.z);
                rb.AddForce(transform.forward * speedWhileCrouching * Time.deltaTime);
            }
            if (!crouching)
            {
                StopCrouching();
            }
            if (Input.GetKeyUp(crouch))
            {
                isSliding = false;
                speed = defaultSpeed;
                StopCrouching();
                StopAllCoroutines();
            }
            if (Input.GetKeyDown(crouch))
            {
                StartCoroutine(slideStop());
            }
        }
    }

    public void StopCrouching()
    {
        transform.localScale = new Vector3(transform.localScale.x, originalSize, transform.localScale.z);
    }

    //sprinting
    public void Sprint()
    {
        if (canSprint && isGrounded())
        {
            if (sprinting)
            {
                speed = sprintSpeed;
            }
            if (upSprinting)
            {
                notSprinting();
            }
        }
    }
    public void notSprinting()
    {
        speed = defaultSpeed;
    }

    public void playerMovement()
    {
        //one multipler
        float multiplerM = 10f;
        FindWherePlayerIsLoking(y, x);

        if (!OnSlope() || !useSlope)
        {
            rb.AddForce(orientation.transform.forward * y * speed * multiplerM, ForceMode.Acceleration);
            rb.AddForce(orientation.transform.right * x * speed * multiplerM, ForceMode.Acceleration);
        }
        else if (isGrounded() && OnSlope() && useSlope)
        {
            rb.AddForce(slerpDirection.normalized * speed * multiplerM * 1.2f, ForceMode.Acceleration);
        }

    }

    public void movementHelp()
    {
        float drag = 1f;
        if (slipperyMovement)
        {
            drag = 1.12f;
        }
        else if (!slipperyMovement)
        {
            drag = 6f;
        }

        rb.drag = drag;
        slerpDirection = Vector3.ProjectOnPlane(movement, slopeHit.normal);
    }

    public void stairAndSlopeFix()
    {
        isUp = Physics.CheckSphere(transform.position, stairRadiusUp, hittable);
        isDown = Physics.CheckSphere(groundCheck.position, stairRadiusDown, hittable);
        bool isMovingX = x > 0.5f || x < -0.5f;
        bool isMovingY = y > 0.5f || y < -0.5f;


        if (isDown && isGrounded() && useStairs)
        {
            if (!isUp && isMovingX)
            {
                rb.AddForce(transform.TransformDirection(Vector3.up) * forceUp);
                rb.AddForce(transform.TransformDirection(Vector3.forward) * forceForward * y);
            }
            if (!isUp && isMovingY)
            {
                rb.AddForce(transform.TransformDirection(Vector3.up) * forceUp);
                rb.AddForce(transform.TransformDirection(Vector3.right) * forceForward * x);
            }
        }
    }

    public void FindWherePlayerIsLoking(float x, float y)
    {
        movement = orientation.transform.forward * x + orientation.transform.right * y;
    }

    RaycastHit slopeHit;
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundCheck.position.y / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    IEnumerator slideStop()
    {
        yield return new WaitForSecondsRealtime(standUpAfter);
        isSliding = true;
    }

    private void standUp()
    {
        float t = (Time.time - startTime) / standUpTime;

        if (isSliding)
        {
            speed = Mathf.SmoothStep(speed, 0, t);
        }

        if (speed == 0)
        {
            isSliding = false;
        }
    }

    public bool isGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, checkRadius, ground);
    }

    private void wallChucking()
    {
        if (noWallSticking)
        {
            //get any collider
            coll = GetComponent<Collider>();

            //make new physics material
            PhysicMaterial phy = new PhysicMaterial("Player Movement");

            //edit entire physics material
            phy.dynamicFriction = 0f;
            phy.staticFriction = 0f;
            phy.bounciness = 0f;
            phy.frictionCombine = PhysicMaterialCombine.Average;
            phy.bounceCombine = PhysicMaterialCombine.Average;

            //apply to collider
            coll.material = phy;
        }
    }
}
    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerMovement))]
    public class variables : Editor
    {
    bool usePhysicsButtons = false;

    public float stairRadiusDown = 1f;
    public float stairRadiusUp = 0.7f;
    public override void OnInspectorGUI()
        {
        PlayerMovement movement = (PlayerMovement)target;

        //movement
        GUILayout.Space(1);
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        GUILayout.Label($"  Speed: {movement.speed}");
        movement.speed = GUILayout.HorizontalSlider(movement.speed, 0, movement.maxSpeed);
        GUILayout.Space(15);
        movement.maxSpeed = EditorGUILayout.FloatField("Max Speed", movement.maxSpeed);
        movement.slipperyMovement = GUILayout.Toggle(movement.slipperyMovement, "Slippery Movement");
        GUILayout.Space(8);
        movement.orientation = EditorGUILayout.ObjectField("Orientation ", movement.orientation, typeof(Transform), true) as Transform;

        //Input
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
        movement.jump = (KeyCode)EditorGUILayout.EnumPopup("Jump", movement.jump);
        movement.crouch = (KeyCode)EditorGUILayout.EnumPopup("Crouch", movement.crouch);
        movement.sprint = (KeyCode)EditorGUILayout.EnumPopup("Sprint", movement.sprint);

        //gravity
        GUILayout.Space(7.5f);
        EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
        movement.ground = EditorGUILayout.LayerField("Ground", movement.ground);
        movement.gravityScale = EditorGUILayout.IntField("Gravity Scale", movement.gravityScale);
        movement.groundCheck = EditorGUILayout.ObjectField("Ground Check ", movement.groundCheck, typeof(Transform), true) as Transform;
        GUILayout.Space(4.75f);
        EditorGUILayout.LabelField($"  Ground Check raduis: {movement.checkRadius}");
        movement.checkRadius = GUILayout.HorizontalSlider(movement.checkRadius, 0, 2);
        GUILayout.Space(10);

        //jumping
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Jumping", EditorStyles.boldLabel);
        movement.jumpForce = EditorGUILayout.FloatField(movement.jumpForce);

        //crouch
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Crouching", EditorStyles.boldLabel);
        movement.canCrouch = GUILayout.Toggle(movement.canCrouch, "Can Crouch");
        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stand Up After");
        movement.standUpAfter = EditorGUILayout.FloatField(movement.standUpAfter);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stand Up Time");
        movement.standUpTime = EditorGUILayout.FloatField(movement.standUpTime);
        GUILayout.EndHorizontal();

        //sprint
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Sprint", EditorStyles.boldLabel);
        movement.canSprint = GUILayout.Toggle(movement.canSprint, "Can Sprint");

        //physics
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);
        usePhysicsButtons = EditorGUILayout.Foldout(usePhysicsButtons, "Physics Preferences", false);
        GUILayout.Space(4);
        if (usePhysicsButtons)
        {
            movement.useSlope = GUILayout.Toggle(movement.useSlope, "Slope");
            movement.useStairs = GUILayout.Toggle(movement.useStairs, "Use Stairs");
            movement.noWallSticking = GUILayout.Toggle(movement.noWallSticking, "No Wall Sticking");
        }

        GUILayout.Space(3.35f);
        GUILayout.BeginVertical();
        GUILayout.Label("Force Forward: ");
        movement.forceForward = EditorGUILayout.FloatField(movement.forceForward);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Force Forward: ");
        movement.forceUp = EditorGUILayout.FloatField(movement.forceUp);
        GUILayout.EndVertical();

        GUILayout.Label("  Stair ");
        GUILayout.Space(4);
        GUILayout.BeginVertical();
        movement.hittable = EditorGUILayout.LayerField("Wall", movement.hittable);
        movement.stairRadiusUp = EditorGUILayout.FloatField("Radius for upper check ", movement.stairRadiusUp);
        movement.stairRadiusDown = EditorGUILayout.FloatField("Radius for down check ", movement.stairRadiusDown);
        GUILayout.EndVertical();

        //for the end 
        GUILayout.Space(4.5f);
    }
}
#endif
    #endregion
