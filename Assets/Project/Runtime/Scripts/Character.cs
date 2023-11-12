using UnityEngine;

namespace HeroicArcade.CC.Core
{
    public class Character : MonoBehaviour
    {
        [SerializeField] InputController inputController;
        public InputController InputController { get => inputController; }

        [SerializeField] Animator animator;
        public Animator Animator { get => animator; }

        [Header("Character Parameters")]
        [SerializeField] float maxWalkSpeed; //6
        public float CurrentMaxWalkSpeed { get => maxWalkSpeed; set => maxWalkSpeed = value; }
        [HideInInspector] public float CurrentMaxMoveSpeed;
        [HideInInspector] public Vector3 velocity = Vector3.zero;
        [HideInInspector] public float velocityXZ = 0f;
        [SerializeField] float moveAcceleration;
        public float MoveAcceleration { get => moveAcceleration; set => moveAcceleration = value; }
    }
}
