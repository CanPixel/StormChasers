using UnityEngine;

namespace com.zibra.liquid.Samples
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int Jumped = Animator.StringToHash("Jumped");

        [SerializeField]
        private float moveSpeed;

        private Animator animator;
        private CharacterController controller;
        private bool hasInput;
        private static readonly int Dance = Animator.StringToHash("Dance");
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");

        protected void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
        }

        public void AnimationJumpEvent()
        {
            animator.SetBool(Jumped, false);
            Debug.Log("Event triggered");
        }

        protected void Update()
        {
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space) && !animator.GetBool(Dance))
            {
                animator.SetBool(Jumped, true);
                return;
            }

            if (animator.GetBool(Jumped))
            {
                return;
            }

            if (Input.GetKey(KeyCode.E))
            {
                animator.SetBool(IsWalking, false);
                animator.SetBool(Dance, true);
                return;
            }

            animator.SetBool(Dance, false);

            var direction = new Vector3(horizontal, 0.0f, vertical);
            hasInput = direction != Vector3.zero;

            if (hasInput)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.5f);
                controller.Move(transform.forward * moveSpeed * Time.deltaTime);
            }

            animator.SetBool(IsWalking, hasInput);

            // Quaternion qRotation = Quaternion.LookRotation(direction);
            // if (hasInput)
            // {
            //     transform.Translate(transform.forward * moveSpeed * Time.deltaTime);
            //     transform.rotation = Quaternion.Slerp(transform.rotation, qRotation, moveSpeed*10 * Time.deltaTime);
            // }
        }
    }
}
