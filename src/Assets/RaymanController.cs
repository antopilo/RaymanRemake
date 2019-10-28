using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymanController : MonoBehaviour
{
    public GameObject Model;
    private CharacterController CharacterController;

    private Vector3 JoystickDirection = new Vector3();

    public Transform TargetPosition;

    private Quaternion SurfaceAngle = new Quaternion();
    private Quaternion LookingAngle = new Quaternion();
    private Quaternion TiltAngle = new Quaternion();
    

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
    public bool isAiming = false;

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
        animator.SetBool("isTargetting", isAiming);
        animator.SetFloat("SideDirection", Input.GetAxisRaw("Horizontal"));
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isGliding)
            isGliding = true;
        else if (Input.GetKeyDown(KeyCode.Space) && isGliding)
            isGliding = false;

        if (Input.GetKeyDown(KeyCode.T))
        {
            isAiming = !isAiming;
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        isGrounded = isRayGrounded();

        UpdateVelocity();

        if (isAiming)
            MaxSpeed = 5f;
        else
            MaxSpeed = 10f;

        ApplyGravity();

        // Rotate the model towards joystick direction
        //Model.transform.LookAt(transform.position + LastLookedDirection);
        //Model.transform.eulerAngles = new Vector3(0, Model.transform.eulerAngles.y, 0);

        CharacterController.Move(CurrentVelocity * Time.fixedDeltaTime);

        GetLookingAngle();
        GetSurfaceAngle();
        

        SetAnimatorParameters();
    }

    bool isRayGrounded()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1.5f))
        {
            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 2, Color.white);
        }

        return false;
    }

    void GetSurfaceAngle()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1.1f))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(hit.normal * 2), Color.red);

            var targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            SurfaceAngle = targetRot;
        }
        else
        {
            SurfaceAngle = Quaternion.FromToRotation(transform.up, transform.up);
        }
        
    }

    void GetLookingAngle()
    {
        var lookPos = LastLookedDirection;
        lookPos.y = 0;

        var targetRot = Quaternion.LookRotation(lookPos);
        Tilt();

        if (isAiming)
        {
            transform.LookAt(TargetPosition);

            Camera.main.GetComponent<CameraFollow>().CenterCamToPlayer();
        }

        

        transform.rotation = Quaternion.Slerp(Model.transform.rotation, SurfaceAngle * targetRot, 0.1f);

    }

    void Tilt()
    {
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


    private void UpdateVelocity()
    {
        //assuming we only using the single camera:
        var camera = Camera.main;

        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;
        forward.y = 0f;
        right.y = 0f;


        // Acceleration
        SimpleVelocity.x += JoystickDirection.x * ACCELERATION;
        SimpleVelocity.z += JoystickDirection.z * ACCELERATION;

        // Deceleration
        if (JoystickDirection.x == 0 && Mathf.Abs(SimpleVelocity.x) > 0)
            SimpleVelocity.x -= DECELERATION * Mathf.Sign(SimpleVelocity.x);
        if (JoystickDirection.z == 0 && Mathf.Abs(SimpleVelocity.z) > 0)
            SimpleVelocity.z -= DECELERATION * Mathf.Sign(SimpleVelocity.z);

        // Zero snapping
        if (Mathf.Abs(SimpleVelocity.x) < STOP_TRESHOLD && JoystickDirection.x == 0)
            SimpleVelocity.x = 0;
        if (Mathf.Abs(SimpleVelocity.z) < STOP_TRESHOLD && JoystickDirection.z == 0)
            SimpleVelocity.z = 0;

        if (Mathf.Abs(SimpleVelocity.x) > MaxSpeed)
            SimpleVelocity.x = MaxSpeed * Mathf.Sign(SimpleVelocity.x);
        if (Mathf.Abs(SimpleVelocity.z) > MaxSpeed)
            SimpleVelocity.z = MaxSpeed * Mathf.Sign(SimpleVelocity.z);


        CurrentVelocity = right * SimpleVelocity.x + forward * SimpleVelocity.z;
        CurrentVelocity.y = YVelocity;
    }

    private void GetInput()
    {
        var joystickX = Input.GetAxisRaw("Horizontal");
        var joystickY = Input.GetAxisRaw("Vertical");

        // Get joystick angle and convert to angles
        var joystickAngle = Mathf.Atan2(joystickX, joystickY);
        JoystickDirection = new Vector3(Mathf.Sin(joystickAngle), 0, Mathf.Cos(joystickAngle));

        if (joystickY == 0)
            JoystickDirection.z = 0;

        if ( JoystickDirection.magnitude > 0.5)
        {
            var forward = Camera.main.transform.forward;
            var right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            LastLookedDirection = right * JoystickDirection.x + forward * JoystickDirection.z;
        }

        if (isGliding && isGrounded)
            isGliding = false;
        else if(isGliding && !isGrounded)
            YVelocity = Mathf.Clamp(YVelocity, -1, Mathf.Infinity);

    }


    private Vector3 GetGlobalDirection()
    {
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        return right * CurrentVelocity.x + forward * CurrentVelocity.z;
    }
}
