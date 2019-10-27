using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymanController : MonoBehaviour
{
    public GameObject Model;
    private CharacterController CharacterController;
    private Vector3 desiredMoveDirection;
    private float CurrentSpeed;
    private Vector3 CurrentVelocity;
    private Vector3 SimpleVelocity = new Vector3();
    private Animator animator;
    private GameObject RaymanModel;
    private bool jump;
    private bool isGrounded = true;
    private bool isUsingHelicopter;
    public  float MaxSpeed = 10f;
    public float ACCELERATION = 1;
    public float DECELERATION = 1;
    public float STOP_TRESHOLD = 0.6f;
    private Vector3 LastLookedDirection = new Vector3();
    private float YVelocity = 0f;
    public float Gravity = 2;
    public float JumpForce = 2;
    public bool isGliding = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        
    }

    void SetAnimatorParameters()
    {
        animator.SetFloat("FlatSpeedAbsoluteValue", new Vector3(CurrentVelocity.x, 0, CurrentVelocity.z).magnitude);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("JumpPressed", jump);
        animator.SetFloat("VerticalSpeedValue", YVelocity);
        animator.SetBool("IsUsingHelicopter", isGliding);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isGliding)
            StartGlide();
        else if (Input.GetKeyDown(KeyCode.Space) && isGliding)
            EndGlide();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        SetAnimatorParameters();

        isGrounded = isRayGrounded();
        ApplyGravity();

        CharacterController.Move(CurrentVelocity * Time.fixedDeltaTime);
    }

    bool isRayGrounded()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1.1f))
        {
            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 2, Color.white);
        }

        return false;
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            jump = false;
            YVelocity -= Gravity * Time.deltaTime;
        }
    }

    void Jump()
    {
        YVelocity = 0f;
        jump = true;
        YVelocity += JumpForce;
    }

    void StartGlide()
    {
        isGliding = true;
        
    }
    void EndGlide()
    {
        isGliding = false;
    }

    private void GetInput()
    {
        var joystickX = Input.GetAxisRaw("Horizontal");
        var joystickY = Input.GetAxisRaw("Vertical");

        // Get joystick angle and convert to angles
        var joystickAngle = Mathf.Atan2(joystickX, joystickY);
        var joystickDirection = new Vector3(Mathf.Sin(joystickAngle), 0, Mathf.Cos(joystickAngle));

        if (joystickY == 0)
            joystickDirection.z = 0;

        //assuming we only using the single camera:
        var camera = Camera.main;

        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;
        forward.y = 0f;
        right.y = 0f;

        // Rotate the model towards joystick direction
        Model.transform.LookAt(transform.position + LastLookedDirection);
        Model.transform.eulerAngles = new Vector3(0, Model.transform.eulerAngles.y, 0);

        

        // Acceleration
        SimpleVelocity.x += joystickDirection.x * ACCELERATION;
        SimpleVelocity.z += joystickDirection.z * ACCELERATION;

        // Deceleration
        if (joystickDirection.x == 0 && Mathf.Abs(SimpleVelocity.x) > 0)
            SimpleVelocity.x -= DECELERATION * Mathf.Sign(SimpleVelocity.x);
        if (joystickDirection.z == 0 && Mathf.Abs(SimpleVelocity.z) > 0)
            SimpleVelocity.z -= DECELERATION * Mathf.Sign(SimpleVelocity.z);

        // Zero snapping
        if (Mathf.Abs(SimpleVelocity.x) < STOP_TRESHOLD && joystickDirection.x == 0)
            SimpleVelocity.x = 0;
        if (Mathf.Abs(SimpleVelocity.z) < STOP_TRESHOLD && joystickDirection.z == 0)
            SimpleVelocity.z = 0;

        if (Mathf.Abs(SimpleVelocity.x) > MaxSpeed)
            SimpleVelocity.x = MaxSpeed * Mathf.Sign(SimpleVelocity.x);
        if (Mathf.Abs(SimpleVelocity.z) > MaxSpeed)
            SimpleVelocity.z = MaxSpeed * Mathf.Sign(SimpleVelocity.z);


        CurrentVelocity = right * SimpleVelocity.x + forward * SimpleVelocity.z;

        CurrentVelocity.y = YVelocity;
        if ( new Vector2(CurrentVelocity.x, CurrentVelocity.z).magnitude > 0.5)
        {
            Debug.Log(CurrentVelocity);
            LastLookedDirection = CurrentVelocity;
        }

        if (isGliding && isGrounded)
            EndGlide();
        else if(isGliding && !isGrounded)
            YVelocity = Mathf.Clamp(YVelocity, -1, Mathf.Infinity);

    }
}
