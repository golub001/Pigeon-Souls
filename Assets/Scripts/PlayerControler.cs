using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float jumpHeight = 0.2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float inputSmoothSpeed = 10f;

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 5f;
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float rollCooldown = 1f;

    private Animator animator;
    private CharacterController controller;

    private Vector2 rawInput;
    private Vector2 smoothInput;
    private Vector2 smoothVelocity;
    private Vector3 velocity;

    // Roll kontrola
    private bool isRolling = false;
    private float rollTimer = 0f;
    private float lastRollTime = -10f;
    private Vector3 rollDirection;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // --- Input ---
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isRolling) // ignoriši input tokom roll-a
            rawInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isRolling && context.performed && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && !isRolling && Time.time > lastRollTime + rollCooldown)
        {
            // Roll ide u pravcu kretanja ili napred ako nema inputa
            if (smoothInput.sqrMagnitude > 0.01f)
                rollDirection = (Camera.main.transform.forward * smoothInput.y + Camera.main.transform.right * smoothInput.x).normalized;
            else
                rollDirection = transform.forward;

            isRolling = true;
            rollTimer = rollDuration;
            lastRollTime = Time.time;

            animator.SetTrigger("Roll");
            Debug.Log("Roll aktiviran!");
        }
    }

    private void Update()
    {
        // --- Roll logika ---
        if (isRolling)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
                isRolling = false;

            return; // preskoči normalno kretanje dok traje roll
        }

        // --- Smooth input ---
        smoothInput = Vector2.SmoothDamp(
            smoothInput,
            rawInput,
            ref smoothVelocity,
            1f / inputSmoothSpeed
        );

        // --- Movement u odnosu na kameru ---
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 move = camForward * smoothInput.y + camRight * smoothInput.x;

        // --- Sprint ---
        bool sprintHeld = Keyboard.current.leftShiftKey.isPressed;
        animator.SetBool("Sprint", sprintHeld);
        float currentSpeed = sprintHeld ? speed * 1.5f : speed;

        // --- Gravity ---
        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = 0;

        // --- Move CharacterController ---
        controller.Move(move * currentSpeed * Time.deltaTime + velocity * Time.deltaTime);

        // --- Animator ---
        Vector3 localMove = transform.InverseTransformDirection(move);
        animator.SetFloat("MoveX", Mathf.Clamp(localMove.x, -1f, 1f));
        animator.SetFloat("MoveZ", Mathf.Clamp(localMove.z, -1f, 1f));
        bool isMoving = smoothInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);

        // --- Rotacija lika u smer kretanja ---
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
