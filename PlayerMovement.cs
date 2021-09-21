using System;
using UnityEngine;

/*Player movememnt for 
 Rigidbody made by 
  NnNeEediIMm!*/

public class PlayerMovement : MonoBehaviour
{

    //move system
    Vector3 movement;
    float x, y;
    [Header("Main Movement")]
    [Range(1, 100)]
    public float speed = 10f;

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

    //for more gravity
    private float endOfYPosition;

    //Rigidbody
    protected Rigidbody rb;

    //jumping
    bool jumping;
    [Header("Jumping")]
    public float jumpForce = 1000f;

    //crouching
    bool crouching;
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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //for gravity
        grCheck = new GameObject("Check");

        //rb fixes
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        //for crouching
        originalSize = transform.localScale.y;
        speedWhileCrouching = speed / 2;

        //for spinting
        defaultSpeed = speed;
        sprintSpeed = defaultSpeed * 1.5f;
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
        //transform position
        GameObject groundd = GameObject.Find("Check");
        groundd.transform.position = new Vector3(transform.position.x, transform.position.y - endOfYPosition, transform.position.z);

        //for ground checking
        endOfYPosition = transform.lossyScale.y;
        isGrounded = Physics.CheckSphere(groundd.transform.position, 0.0011f, ground);
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

        crouching = Input.GetKey(crouch) && Input.GetKey(KeyCode.W);

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
        }

    }

    public void StopCrouching()
    {
        transform.localScale = new Vector3(transform.localScale.x, originalSize, transform.localScale.z);
        speed = defaultSpeed;
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
        float drag = 6f;
        rb.drag = drag;
        movement = transform.forward * y + transform.right * x;
    }
}
