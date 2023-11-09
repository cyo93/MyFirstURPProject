﻿//#define MB_DEBUG

using MenteBacata.ScivoloCharacterController;
using MenteBacata.ScivoloCharacterControllerDemo;
using System.Collections.Generic;
using UnityEngine;


namespace HeroicArcade.CC.Core


{
    public class SimpleCharacterController : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public Animator Animator { get => animator; }

        [SerializeField] InputController inputController;
        public InputController InputController { get => inputController; }

        public float moveSpeed = 5f;

        public float jumpSpeed = 8f;
        
        public float rotationSpeed = 720f;

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


        private void Start()
        {
            cameraTransform = Camera.main.transform;
            mover.canClimbSteepSlope = true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector3 movementInput = GetMovementInput();

            Vector3 velocity = moveSpeed * movementInput;
            
            HandleOverlaps();

            bool groundDetected = DetectGroundAndCheckIfGrounded(out bool isGrounded, out GroundInfo groundInfo);

            SetGroundedIndicatorColor(isGrounded);

            isOnMovingPlatform = false;

            if (isGrounded && inputController.IsJumpPressed)
            {
                verticalSpeed = jumpSpeed;
                nextUngroundedTime = -1f;
                isGrounded = false;
            }

            if (isGrounded)
            {
                mover.mode = CharacterMover.Mode.Walk;
                verticalSpeed = 0f;
                animator.SetBool("IsJumping", false);
                if (groundDetected)
                    isOnMovingPlatform = groundInfo.collider.TryGetComponent(out movingPlatform);
            }
            else
            {
                mover.mode = CharacterMover.Mode.SimpleSlide;
                BounceDownIfTouchedCeiling();
                animator.SetBool("IsJumping", true);
                verticalSpeed += gravity * deltaTime;

                if (verticalSpeed < minVerticalSpeed)
                    verticalSpeed = minVerticalSpeed;

                velocity += verticalSpeed * transform.up;
            }

            animator.SetFloat("MoveSpeed", new Vector3(velocity.x, 0, velocity.z).magnitude / moveSpeed);
            RotateTowards(velocity);

            mover.Move(velocity * deltaTime, groundDetected, groundInfo, overlapCount, overlaps, moveContacts, out contactCount);
        }

        private void LateUpdate()
        {
            if (isOnMovingPlatform)
                ApplyPlatformMovement(movingPlatform);
        }

        private Vector3 GetMovementInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
            Vector3 right = Vector3.Cross(transform.up, forward);

            return x * right + y * forward;
        }

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
            Vector3 flatDirection = Vector3.ProjectOnPlane(direction, transform.up);

            if (flatDirection.sqrMagnitude < 1E-06f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(flatDirection, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

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
    }
}