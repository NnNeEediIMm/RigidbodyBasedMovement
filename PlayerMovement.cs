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

    //gravity
    private bool isGrounded = false;
    [Header("Gravity")]
    public LayerMask ground;
    public int gravityScale = 20;

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
    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & ground) != 0)
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (isGrounded)
        {
            isGrounded = false;
        }
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

        crouching = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W);
        
        sprinting = Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.W);
        upSprinting = Input.GetKeyUp(KeyCode.LeftControl);
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
        if (canCrouch && isGrounded)
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
