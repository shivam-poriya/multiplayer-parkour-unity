using PlayerMovement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class playerController : MonoBehaviour
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
    [SerializeField] float FirstJumpHeight = 1.2f;
    [SerializeField] float SecondJumpHeight = 1.4f;

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
    [Space(10)]
    [SerializeField] Animator animator;
    CharacterController controller;
    InputSystem input;
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
    [SerializeField] Transform lowerRay;
    [SerializeField] Transform upperRay;
    [SerializeField] float raycastRange;
    [SerializeField] float ledgeYOffset;

    void Start()
    {
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputSystem>();
        AssignAnimationIDs();
        currentJumps = maxJumps;
        // reset our timeouts on start
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
    }


    void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animx = Animator.StringToHash("x");
        animy = Animator.StringToHash("y");
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
        // if there is an input and camera position is not fixed
        if (input.look.sqrMagnitude >= 0.01f)
        {
            cinemachineTargetYaw += input.look.x;
            cinemachineTargetPitch += input.look.y;
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
        float targetSpeed = input.sprint ? sprintSpeed : runSpeed;
        if (input.move == Vector2.zero) targetSpeed = 0.0f;
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

        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

        if (input.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            if (input.sprint)
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

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        // move the player
        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                         new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        animator.SetFloat(animIDSpeed, animationBlend);
        if (!input.sprint)
        {
            xanimBlend = Mathf.Lerp(xanimBlend, input.move.x, smoothSpeed * Time.deltaTime);
            yanimBlend = Mathf.Lerp(yanimBlend, input.move.y, smoothSpeed * Time.deltaTime);
            animator.SetFloat(animx, xanimBlend);
            animator.SetFloat(animy, yanimBlend);
        }
    }

    public void addJumpForce()
    {
        verticalVelocity = Mathf.Sqrt(FirstJumpHeight * -2f * Gravity);
    }

    private void JumpAndGravity()
    {

        if (Grounded)
        {
            // Reset fall timeout timer
            fallTimeoutDelta = FallTimeout;

            // Update animator
            animator.SetBool(animIDJump, false);
            animator.SetBool(animIDFreeFall, false);

            // Stop velocity drop when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump logic
            if (input.jump && jumpTimeoutDelta <= 0.0f)
            {
                PerformJump();
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
            if (input.jump && currentJumps >= 0)
            {
                PerformJump();
            }

            // Prevent jump spamming
            input.jump = false;
        }

        // Apply gravity over time if under terminal velocity
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void PerformJump()
    {
        // Add vertical velocity for jump
        float jumpHeight = (currentJumps > 1) ? FirstJumpHeight : SecondJumpHeight;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Gravity);

        // Update animator
        animator.SetBool(animIDJump, true);

        // Decrease available jumps
        currentJumps--;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

