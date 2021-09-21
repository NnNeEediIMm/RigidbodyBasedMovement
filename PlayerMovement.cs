using System;
using UnityEngine;

/*Player movememnt for 
 Rigidbody made by 
  NnNeEediIMm!*/

public class PlayerMovement : MonoBehaviour
{

    //move system
    Vector3 movement;
    [Header("Main Movement")]
    [Range(1, 100)]
    public float speed = 10f;

    //input system part 1.
    [Header("Input")]
    public KeyCode crouch = KeyCode.LeftShift;
    public KeyCode sprint = KeyCode.LeftControl;

    //gravity
    private bool isGrounded = false;
    [Header("Gravity")]
    private GameObject grCheck;
    public LayerMask ground;
    public int gravityScale = 20;

    //for more gravity
    private float endOfYPosition;

    //Rigidbody
    protected Rigidbody rb;

    //jumping
    bool jumping;
    [Header("Jumping")]
    public float jumpForce = 150f;

    //crouching
    bool crouching;
    [Header("Crouching")]
    public bool canCrouch = true;
    private float speedWhileCrouching = 5f;
    private float originalSize;
    public float reducedSize = 0.5f;

    //sprinting
    [Header("Sprinting")]
    public bool canSprint = true;
    public float sprintSpeed = 13f;
    private bool sprinting;
    private bool upSprinting;
    float defaultSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //for gravity
        grCheck = new GameObject("Check");

        //for freezing rb rotation
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        //for crouching
        originalSize = transform.localScale.y;
        speedWhileCrouching = speed / 2;

        //for spinting
        defaultSpeed = speed;
    }

    private void Update()
    {
        //movement
        movementInput();

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
        rb.AddForce(Vector3.down * Time.deltaTime * gravityScale);
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
        movement.z = Input.GetAxis("Horizontal");
        movement.x = Input.GetAxis("Vertical");

        jumping = Input.GetKeyDown(KeyCode.Space);

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
        transform.Translate(Vector3.right * movement.z * speed * Time.fixedDeltaTime);
        transform.Translate(Vector3.forward * movement.x * speed * Time.fixedDeltaTime);
    }
}
