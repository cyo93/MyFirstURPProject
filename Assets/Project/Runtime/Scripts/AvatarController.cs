﻿using UnityEngine;
using MenteBacata.ScivoloCharacterController;
using MenteBacata.ScivoloCharacterControllerDemo;
//using MenteBacata.ScivoloCharacterControllerDemo;
using System.Text.RegularExpressions;
using Cinemachine;
using System.Collections;

namespace HeroicArcade.CC.Core
{
    public class AvatarController : MonoBehaviour
    {
        public Character Character { get; private set; }
        
        [SerializeField] CinemachineFreeLook cinemachineFreeLook;     // Recenter X (World Space)
        [SerializeField] SimpleFollowRecenterX simpleFollowRecenterX; // Recenter X (Simple Follow With World Up)

        [SerializeField] CinemachineFreeLook aimCameraLeft;
        [SerializeField] CinemachineFreeLook aimCameraRight;

        public enum AimCameraOffset
        {
            Left = 1,
            Right = -1
        }
        public AimCameraOffset aimCameraOffset = AimCameraOffset.Left;

        public float moveSpeed = 5f;

        public float gravity = -25f;

        public CharacterCapsule capsule;

        public CharacterMover mover;

        public GroundDetector groundDetector;

        public MeshRenderer groundedIndicator;

        private const float minVerticalSpeed = -12f;

        // Allowed time before the character is set to ungrounded from the last time he was safely grounded.
        private const float timeBeforeUngrounded = 0.02f;

        // Speed along the character local up direction.
        private float verticalSpeed = 0f;

        // Time after which the character should be considered ungrounded.
        private float nextUngroundedTime = -1f;

        private Transform cameraTransform;

        private Collider[] overlaps = new Collider[5];

        private int overlapCount;

        private MoveContact[] moveContacts = CharacterMover.NewMoveContactArray;

        private int contactCount;

        private bool isOnMovingPlatform = false;

        private MovingPlatform movingPlatform;

        private void Awake()
        {
            Character = GetComponent<Character>();
            if (cinemachineFreeLook.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
            {
                if (simpleFollowRecenterX == null)
                {
                    Debug.LogError("Unable to OnCameraRecenterX() because CinemachineTransposer.BindingMode is SimpleFollowWithWorldUp, but SimpleFollowRecenterX component is null");
                }
                else
                {
                    simpleFollowRecenterX.enabled = true;
                }
            }
        }

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            mover.canClimbSteepSlope = true;
        }
	
        private void Update()
        {
            Character.Animator.SetBool("IsAimPressed", Character.InputController.IsAimingPressed);
            Character.Animator.SetBool("IsShootPressed", Character.InputController.IsShootPressed);
            Character.Animator.SetBool("IsSprintPressed", Character.InputController.IsSprintPressed);
            float deltaTime = Time.deltaTime;
            Vector3 movementInput = GetMovementInput();

            Character.velocityXZ += Character.MoveAcceleration * deltaTime;
            if (Character.velocityXZ > Character.CurrentMaxMoveSpeed)
                Character.velocityXZ = Character.CurrentMaxMoveSpeed;

            Vector3 velocity = moveSpeed * movementInput;

            Character.velocity = Character.velocityXZ * movementInput;
            HandleOverlaps();

            bool groundDetected = DetectGroundAndCheckIfGrounded(out bool isGrounded, out GroundInfo groundInfo);

            Character.Animator.SetBool("IsJumping", !isGrounded);

            SetGroundedIndicatorColor(isGrounded);

            isOnMovingPlatform = false;

            if (isGrounded && Character.InputController.IsJumpPressed)
            {
                verticalSpeed = Character.jumpSpeed;
                nextUngroundedTime = -1f;
                isGrounded = false;
            }

            if (isGrounded)
            {
                mover.mode = CharacterMover.Mode.Walk;
                verticalSpeed = 0f;

                if (groundDetected)
                    isOnMovingPlatform = groundInfo.collider.TryGetComponent(out movingPlatform);
            }
            else
            {
                mover.mode = CharacterMover.Mode.SimpleSlide;

                BounceDownIfTouchedCeiling();

                verticalSpeed += gravity * deltaTime;

                if (verticalSpeed < minVerticalSpeed)
                    verticalSpeed = minVerticalSpeed;

                velocity += verticalSpeed * transform.up;
            }

            if (isGrounded)
            {
                if (movementInput.sqrMagnitude < 1E-06f)
                {
                    Character.velocityXZ = 0f;
                    //Character.Animator.SetBool("IsSprintPressed", false);
                }

                Character.Animator.SetFloat("MoveSpeed",
                    new Vector3(Character.velocity.x, 0, Character.velocity.z).magnitude / Character.CurrentMaxMoveSpeed);

                if (Character.velocityXZ >= 1E-06f)
                {
                    //Character.Animator.SetBool("IsSprintPressed", Character.InputController.IsSprintPressed);
                }

                Character.CurrentMaxMoveSpeed = Character.CurrentMaxWalkSpeed;
            }

            RotateTowards(velocity);


            Character.CurrentMaxMoveSpeed =
                Character.InputController.IsSprintPressed ? Character.CurrentMaxSprintSpeed : Character.CurrentMaxWalkSpeed;

            mover.Move(velocity * deltaTime, groundDetected, groundInfo, overlapCount, overlaps, moveContacts, out contactCount);

            Character.Animator.SetFloat("MoveSpeed",
                new Vector3(Character.velocity.x, 0, Character.velocity.z).magnitude / Character.CurrentMaxMoveSpeed);
        }

        private void LateUpdate()
        {
            if (isOnMovingPlatform)
                ApplyPlatformMovement(movingPlatform);
        }

        private Vector3 GetMovementInput()
        {
            float x = currentMovement.x; // 0; // Input.GetAxis("Horizontal");
            float y = currentMovement.z; //0; // Input.GetAxis("Vertical");
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
            Vector3 right = Vector3.Cross(transform.up, forward);
            return x * right + y * forward;
        }

        Vector3 projectedCameraForward;
        Quaternion rotationToCamera;
        //private Vector3 GetMovementInput()
        //{
        //    projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up);
        //    rotationToCamera = Quaternion.LookRotation(projectedCameraForward, transform.up);
        //    return rotationToCamera * (currentMovement.x * Vector3.right + currentMovement.z * Vector3.forward);
        //}

        private void HandleOverlaps()
        {
            if (capsule.TryResolveOverlap())
            {
                overlapCount = 0;
            }
            else
            {
                overlapCount = capsule.CollectOverlaps(overlaps);
            }
        }

        private bool DetectGroundAndCheckIfGrounded(out bool isGrounded, out GroundInfo groundInfo)
        {
            bool groundDetected = groundDetector.DetectGround(out groundInfo);

            if (groundDetected)
            {
                if (groundInfo.isOnFloor && verticalSpeed < 0.1f)
                    nextUngroundedTime = Time.time + timeBeforeUngrounded;
            }
            else
                nextUngroundedTime = -1f;

            isGrounded = Time.time < nextUngroundedTime;
            return groundDetected;
        }

        private void SetGroundedIndicatorColor(bool isGrounded)
        {
            if (groundedIndicator != null)
                groundedIndicator.material.color = isGrounded ? Color.green : Color.blue;
        }

        private void RotateTowards(Vector3 direction)
        {
            switch (Character.CamStyle)
            {
                case Character.CameraStyle.Adventure:
                    //Do nothing
                    break;

                case Character.CameraStyle.Combat:
                    direction = cameraTransform.forward;
                    break;

                default:
                    Debug.LogError($"Unexpected CameraStyle {Character.CamStyle}");
                    return;
            }

            Vector3 flatDirection = Vector3.ProjectOnPlane(direction, transform.up);
            if (flatDirection.sqrMagnitude < 1E-06f)
                return;
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Character.TurnSpeed * Time.deltaTime);
        }

        //private void RotateTowards(in Vector3 direction)
        //{
        //    Vector3 flatDirection = Vector3.ProjectOnPlane(direction, transform.up);

        //    if (flatDirection.sqrMagnitude < 1E-06f)
        //        return;

        //    Quaternion targetRotation = Quaternion.LookRotation(flatDirection, transform.up);
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //}

        private void ApplyPlatformMovement(MovingPlatform movingPlatform)
        {
            GetMovementFromMovingPlatform(movingPlatform, out Vector3 movement, out float upRotation);

            transform.Translate(movement, Space.World);
            transform.Rotate(0f, upRotation, 0f, Space.Self);
        }

        private void GetMovementFromMovingPlatform(MovingPlatform movingPlatform, out Vector3 movement, out float deltaAngleUp)
        {
            movingPlatform.GetDeltaPositionAndRotation(out Vector3 platformDeltaPosition, out Quaternion platformDeltaRotation);
            Vector3 localPosition = transform.position - movingPlatform.transform.position;
            movement = platformDeltaPosition + platformDeltaRotation * localPosition - localPosition;

            platformDeltaRotation.ToAngleAxis(out float platformDeltaAngle, out Vector3 axis);
            float axisDotUp = Vector3.Dot(axis, transform.up);

            if (-0.1f < axisDotUp && axisDotUp < 0.1f)
                deltaAngleUp = 0f;
            else
                deltaAngleUp = platformDeltaAngle * Mathf.Sign(axisDotUp);
        }

        private void BounceDownIfTouchedCeiling()
        {
            for (int i = 0; i < contactCount; i++)
            {
                if (Vector3.Dot(moveContacts[i].normal, transform.up) < -0.7f)
                {
                    verticalSpeed = -0.25f * verticalSpeed;
                    break;
                }
            }
        }

        private Vector3 currentMovement;
        public void OnMoveInput(Vector2 moveInput)
        {
            //y needs to preserve its value from the previous Update.
            currentMovement.x = moveInput.x;
            currentMovement.z = moveInput.y;
        }

        IEnumerator CameraRecenterX(float duration)
        {
            yield return new WaitForSeconds(duration);
            cinemachineFreeLook.m_RecenterToTargetHeading.m_RecenteringTime = 0;
            cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = false;
        }

        public void OnCameraRecenterX(bool isCameraRecenterXPressed)
        {
            switch (cinemachineFreeLook.m_BindingMode)
            {
                case CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp:
                    if (isCameraRecenterXPressed)
                        simpleFollowRecenterX.recenter = true;
                    break;
                case CinemachineTransposer.BindingMode.WorldSpace:
                    if (cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled)
                    {
                        if (!isCameraRecenterXPressed)
                        {
                            cinemachineFreeLook.m_RecenterToTargetHeading.m_RecenteringTime = 0;
                            cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = false;
                        }
                    }
                    else if (isCameraRecenterXPressed)
                    {
                        const float duration = 2; // 0.01f;
                        cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = isCameraRecenterXPressed;
                        cinemachineFreeLook.m_RecenterToTargetHeading.m_RecenteringTime = duration;
                        cinemachineFreeLook.m_RecenterToTargetHeading.RecenterNow();
                        StartCoroutine("CameraRecenterX", duration + 0.03f); //A very long period
                    }
                    break;
            }
        }

        public void OnCameraAim(bool isCameraAimPressed)
        {
            Character.CamStyle = isCameraAimPressed ? Character.CameraStyle.Combat : Character.CameraStyle.Adventure;
            if (!isCameraAimPressed)
            {
                aimCameraLeft.Priority = 5;
                aimCameraRight.Priority = 5;
            }
            else if (aimCameraOffset == AimCameraOffset.Left)
            {
                aimCameraLeft.Priority = 20;
                aimCameraRight.Priority = 5;
            }
            else
            {
                aimCameraLeft.Priority = 5;
                aimCameraRight.Priority = 20;
            }
        }


        public void OnAimSwap()
        {
            if (aimCameraOffset == AimCameraOffset.Left)
            {
                aimCameraOffset = AimCameraOffset.Right;
            }
            else
            {
                aimCameraOffset = AimCameraOffset.Left;
            }
            OnCameraAim(Character.InputController.IsAimingPressed);
        }
    }
}
