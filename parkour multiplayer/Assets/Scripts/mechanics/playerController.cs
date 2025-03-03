using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class playerController : NetworkBehaviour
{
    [Header("Player Movement")]
    [SerializeField] float runSpeed;
    [SerializeField] float sprintSpeed;

    [SerializeField] float speedChangeRate;
    [SerializeField] float RotationSmoothTime = 0.12f;

    [Space(10)]
    [SerializeField] float JumpTimeout = 0.50f;

    [SerializeField] float FallTimeout = 0.15f;

    [Space(10)]
    [SerializeField] float JumpHeight = 1.2f;
    

    [SerializeField] float Gravity = -15.0f;

    [Header("Player Grounde")]
    bool Grounded = true;

    [SerializeField] float GroundedOffset = -0.14f;

    [SerializeField] float GroundedRadius = 0.28f;

    [SerializeField] LayerMask GroundLayers;

    [Header("Cinemachine")]
    [SerializeField] GameObject CinemachineCameraTarget;

    [Header("Double Jump")]
    [SerializeField] int maxJumps = 2;
    private int currentJumps;

    //Private variables

    //Player
    float speed;
    float animationBlend;
    float targetRotation = 0f;
    float rotationVelocity;
    float verticalVelocity;
    float terminalVelocity = 53.0f;
    float xanimBlend, yanimBlend;

    // animation IDs
    int animIDSpeed;
    int animIDJump;
    int animIDFreeFall;
    int animx;
    int animy;
    int animIDCrouch;
    [Space(10)]
    [SerializeField] Animator animator;
    [SerializeField]CharacterController controller;

    [SerializeField] Transform mainCamera;
    [SerializeField] float smoothSpeed = 10f;


    // timeout deltatime
    float jumpTimeoutDelta;
    float fallTimeoutDelta;

    // cinemachine
    float cinemachineTargetYaw;
    float cinemachineTargetPitch;
    float TopClamp = 70.0f;
    float BottomClamp = -30.0f;
    [SerializeField] GameObject tppCamera;
    public GameObject tppCameraVirtual;
    bool isCrouched = false;
    bool jump;
    bool canMove = true;
    void Start()
    {
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        AssignAnimationIDs();
        currentJumps = maxJumps;
        // reset our timeouts on start
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
        if (IsOwner)
        {
            DisableOtherCameras();
        }
        else
        {
            tppCamera.SetActive(false);
            tppCameraVirtual.SetActive(false);
        }
    }

    private void DisableOtherCameras()
    {
        GameObject[] allCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        GameObject[] vCameras = GameObject.FindGameObjectsWithTag("vc");
        foreach (var camera in allCameras)
        {
            if (camera.gameObject != tppCamera)
            {
                camera.gameObject.SetActive(false);
            }
        }
        foreach (var camera in vCameras)
        {
            if (camera.gameObject != tppCameraVirtual)
            {
                camera.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (!IsOwner) return;
        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    void LateUpdate()
    {

        if (!IsOwner) return;
        CameraRotation();
 
    }

    void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animx = Animator.StringToHash("x");
        animy = Animator.StringToHash("y");
        animIDCrouch= Animator.StringToHash("crouch");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        if (Grounded)
        {
            // Reset jump count when grounded
            currentJumps = maxJumps;
        }

    }

    void CameraRotation()
    {
        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");
        Vector2 look = new Vector2(x,y*-1);
        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= 0.01f)
        {
            cinemachineTargetYaw += look.x;
            cinemachineTargetPitch += look.y;
        }

        // clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch,
            cinemachineTargetYaw, 0.0f);
    }

    void Move()
    {
        if(!canMove) { return; }
        if(Input.GetKeyDown(KeyCode.C) && Grounded)
        {
            isCrouched = isCrouched ? false : true;
        }
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : runSpeed;
        if (move == Vector2.zero) targetSpeed = 0.0f;
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        if (currentHorizontalSpeed < targetSpeed - 0.1f || currentHorizontalSpeed > targetSpeed + 0.1f)
        {
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) { animationBlend = 0f; }

        Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

        if (move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            if ((Input.GetKey(KeyCode.LeftShift) && Grounded) && !isCrouched)
            {
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                                                       RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            else
            {
                float smoothAngle = Mathf.SmoothDampAngle(gameObject.transform.eulerAngles.y, mainCamera.eulerAngles.y, ref rotationVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0.0f, smoothAngle, 0.0f);
            }
        }
        Vector3 targetDirection = new Vector3(0f,0f,0f);
        if(!isCrouched)
            targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        // move the player
        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                         new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        animator.SetFloat(animIDSpeed, animationBlend);
        if (!Input.GetKey(KeyCode.LeftShift) || isCrouched)
        {
            xanimBlend = Mathf.Lerp(xanimBlend, move.x, smoothSpeed * Time.deltaTime);
            yanimBlend = Mathf.Lerp(yanimBlend, move.y, smoothSpeed * Time.deltaTime);
            animator.SetFloat(animx, xanimBlend);
            animator.SetFloat(animy, yanimBlend);
        }
        if(isCrouched && Grounded)
        {
            if(!animator.applyRootMotion)
            {
                animator.applyRootMotion = true;
                float time = 0.633f;
                if(targetSpeed == 6f) { time = 1.333f; }
                animator.SetBool(animIDCrouch,true);
                StartCoroutine(waitForCrouchTransition(time));
            }
        }
        else
        {
            if(animator.applyRootMotion)
            {
                isCrouched = false;
                animator.SetBool(animIDCrouch, false);
                animator.applyRootMotion = false;
            }
        }
    }

    IEnumerator waitForCrouchTransition(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time-0.01f);
        canMove = true;
    }

    public void addJumpForce()
    {
        verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
    }
    private void JumpAndGravity()
    {
        jump = Input.GetKeyDown(KeyCode.Space);
        if (Grounded)
        {
            // Reset fall timeout timer
            fallTimeoutDelta = FallTimeout;

            // Update animator
            animator.SetBool(animIDJump, false);
            animator.SetBool(animIDFreeFall, false);
            animator.SetBool("doubleJump", false);
            // Stop velocity drop when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump logic
            if (jump && jumpTimeoutDelta <= 0.0f && !isCrouched)
            {
                animator.SetBool(animIDJump, true);
                currentJumps --;
            }

            // Jump timeout logic
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset jump timeout timer
            jumpTimeoutDelta = JumpTimeout;

            // Fall timeout logic
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // Update animator
                animator.SetBool(animIDFreeFall, true);
            }

            // Double jump logic
            if (jump && currentJumps > 0)
            {
                animator.SetBool("doubleJump", true);
                currentJumps--;
                StartCoroutine(waitForDoubleJump());
            }

            // Prevent jump spamming
            jump = false;
        }

        // Apply gravity over time if under terminal velocity
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    IEnumerator waitForDoubleJump()
    {
        yield return new WaitForSeconds(0.4f);
        animator.SetBool("doubleJump", false);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

