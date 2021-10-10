using System;
using UnityEngine;

/*Player movement for 
 Rigidbody made by 
  NnNeEediIMm!*/

public class PlayerMovement : MonoBehaviour
{
    //move system
    Vector3 movement;
    float x, y;
    GameObject movementHelper;
    [Header("Main Movement")]
    [Range(1, 100)]
    public float speed = 10f;
    public bool slipperyMovement = true;

    //input system part 1.
    [Header("Input")]
    public KeyCode crouch = KeyCode.LeftShift;
    public KeyCode sprint = KeyCode.LeftControl;
    public KeyCode jump = KeyCode.Space;

    //gravity
    private bool isGrounded = false;
    private GameObject grCheck;
    [Header("Gravity")]
    public LayerMask ground;
    public int gravityScale = 20;
    public float checkRadius = 0.34f;

    //for more gravity
    private float endOfYPosition;

    //Rigidbody
    protected Rigidbody rb;

    //jumping
    bool jumping;
    [Header("Jumping")]
    public float jumpForce = 1000f;

    //crouching
    bool crouching, crouchingI;
    [Header("Crouching")]
    public bool canCrouch = true;
    private float speedWhileCrouching;
    private float originalSize;
    public float reducedSize = 0.5f;

    //sprinting
    [Header("Sprinting")]
    public bool canSprint = true;
    private float sprintSpeed;
    private bool sprinting;
    private bool upSprinting;
    float defaultSpeed;

    //physics fix
    [Header("Physics")]
    public float forceForward= 4f;
    public float forceUp = 5f;
    public LayerMask hittable;
    public Vector3 upperCheck, downCheck;
    public float stairCheckRadius = 1f;

    GameObject upper, downer;
    bool isUp, isDown;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //for movement
        movementHelper = new GameObject("Movement Helper");
        movementHelper.transform.parent = this.transform;

        //for ground
        groundCheck();
        endOfYPosition = transform.lossyScale.y;
        
        //rb fixes
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        //for crouching
        originalSize = transform.localScale.y;
        speedWhileCrouching = speed / 2;

        //for spinting
        defaultSpeed = speed;
        sprintSpeed = defaultSpeed * 1.5f;

        //for physics
        summonSlope();
    }

    private void groundCheck()
    {
        //for gravity
        grCheck = new GameObject("Check");
        grCheck.transform.position = new Vector3(transform.position.x, transform.position.y - endOfYPosition, transform.position.z);
        grCheck.transform.parent = movementHelper.transform;
    }

    private void Update()
    {
        //movement
        movementInput();
        movementHelp();

        //gravity
        normalizeGravity();

        //mechanics
        Jump();
        Crouching();
        Sprint();

        //slopes 
        stairAndSlopeFix();
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
    public void normalizeGravity()
    {
        isGrounded = Physics.CheckSphere(grCheck.transform.position, checkRadius, ground);
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
        crouchingI = Input.GetKeyUp(crouch);

        sprinting = Input.GetKey(sprint) && Input.GetKey(KeyCode.W);
        upSprinting = Input.GetKeyUp(sprint);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            if (jumping)
            {
                rb.AddForce(Vector3.up * jumpForce);
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
                speed = speedWhileCrouching;
            }
            if (!crouching)
            {
                StopCrouching();
            }
            if (crouchingI)
            {
                speed = defaultSpeed;
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
        if (canSprint && isGrounded)
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

        rb.AddForce(movement.normalized * speed * multiplerM, ForceMode.Acceleration);
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
        movement = transform.forward * y + transform.right * x;
    }

    private void summonSlope()
    {
        upper = new GameObject("UpperCheck");
        downer = new GameObject("DownCheck");
        upper.transform.parent = movementHelper.transform;
        downer.transform.parent = movementHelper.transform;
        upper.transform.position = upperCheck;
        downer.transform.position = downCheck;
    }

    public void stairAndSlopeFix()
    {
        isUp = Physics.CheckSphere(upper.transform.position, stairCheckRadius, hittable);
        isDown = Physics.CheckSphere(downer.transform.position, stairCheckRadius, hittable);
        bool isMovingX = x > 0.5f || x < -0.5f;
        bool isMovingY = y > 0.5f || y < -0.5f;

        if (isDown)
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
}
