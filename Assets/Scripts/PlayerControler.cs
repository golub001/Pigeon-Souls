using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
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

    public void OnMove(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded && !isRolling)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && !isRolling && Time.time > lastRollTime + rollCooldown)
        {
            // Ako se kreće - roll ide u pravcu kretanja, inače napred
            if (smoothInput.sqrMagnitude > 0.01f)
                rollDirection = new Vector3(smoothInput.x, 0, smoothInput.y).normalized;
            else
                rollDirection = transform.forward;

            isRolling = true;
            rollTimer = rollDuration;
            lastRollTime = Time.time;

            animator.SetTrigger("Roll"); // Animator trigger
            Debug.Log("Roll aktiviran!");

        }
    }

    private void Update()
    {
        // Ako traje roll
        if (isRolling)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                isRolling = false;
            }
            return; // prekida ostatak Update dok traje roll
        }

        // Deadzone za raw input
        if (Mathf.Abs(rawInput.x) < 0.01f) rawInput.x = 0;
        if (Mathf.Abs(rawInput.y) < 0.01f) rawInput.y = 0;

        // SmoothDamp po X i Z osi
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, rawInput.x, ref smoothVelocity.x, 1f / inputSmoothSpeed);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, rawInput.y, ref smoothVelocity.y, 1f / inputSmoothSpeed);

        // Deadzone za smooth input
        if (Mathf.Abs(smoothInput.x) < 0.01f) smoothInput.x = 0;
        if (Mathf.Abs(smoothInput.y) < 0.01f) smoothInput.y = 0;

        Vector3 move = new Vector3(smoothInput.x, 0, smoothInput.y);
        Vector3 localMove = transform.InverseTransformDirection(move);

        // --- Sprint logika ---
        bool sprintHeld = Keyboard.current.leftShiftKey.isPressed;
        animator.SetBool("Sprint", sprintHeld);

        float currentSpeed = sprintHeld ? speed * 1.5f : speed;

        // Pomeri CharacterController
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Pošalji animatoru glatke vrednosti
        animator.SetFloat("MoveX", Mathf.Clamp(localMove.x, -1f, 1f));
        animator.SetFloat("MoveZ", Mathf.Clamp(localMove.z, -1f, 1f));

        bool isMoving = smoothInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);

        // Rotacija lika u smer kretanja
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
