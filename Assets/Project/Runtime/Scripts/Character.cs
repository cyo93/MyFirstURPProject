using UnityEngine;

namespace HeroicArcade.CC.Core
{
    public class Character : MonoBehaviour
    {
        [SerializeField] InputController inputController;
        public InputController InputController { get => inputController; }

        [SerializeField] Animator animator;
        public Animator Animator { get => animator; }

        public enum CameraStyle
        {
            Adventure,
            Combat,
        }


        [SerializeField] CameraStyle camStyle;
        public CameraStyle CamStyle { get => camStyle; set => camStyle = value; }

        [Header("Character Parameters")]
        [SerializeField] float maxWalkSpeed; //6
        public float CurrentMaxWalkSpeed { get => maxWalkSpeed; set => maxWalkSpeed = value; }
        [SerializeField] float maxSprintSpeed;
        public float CurrentMaxSprintSpeed { get => maxSprintSpeed; set => maxSprintSpeed = value; }
        [SerializeField] public float jumpSpeed;
        [HideInInspector] public float CurrentMaxMoveSpeed;
        [SerializeField] float turnSpeed;

        //[SerializeField] AutoAiming autoAiming;
        //public AutoAiming AutoAiming { get => autoAiming; }

        public float TurnSpeed { get => turnSpeed; }
        [HideInInspector] public Vector3 velocity = Vector3.zero;
        [HideInInspector] public float velocityXZ = 0f;
        [SerializeField] float moveAcceleration;
        public float MoveAcceleration { get => moveAcceleration; set => moveAcceleration = value; }
    }
}
