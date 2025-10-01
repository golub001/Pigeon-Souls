using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float inputSmoothSpeed = 10f;

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 5f;
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float rollCost = 20f; // koliko roll košta

    [Header("Stamina Settings")]
    public Slider StaminaSlider;           // ✅ Slider umesto Image
    public float Stamina = 100f;
    public float MaxStamina = 100f;
    [SerializeField] private float MovementCost = 5f; // stamina/sec za sprint
    [SerializeField] private float RegenRate = 10f;  // stamina/sec za regen

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

        // Podesi slider max/min i startnu vrednost
        if (StaminaSlider != null)
        {
            StaminaSlider.minValue = 0f;
            StaminaSlider.maxValue = MaxStamina;
            StaminaSlider.value = Stamina;
        }
    }

    // --- Input ---
    public void OnMove(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context) { }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && !isRolling && Time.time > lastRollTime + rollCooldown)
        {
            if (Stamina < rollCost)
            {
                Debug.Log("Nemaš dovoljno stamene za roll!");
                return;
            }

            Stamina -= rollCost;
            if (Stamina < 0f) Stamina = 0f;

            if (smoothInput.sqrMagnitude > 0.01f)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;
                camForward.y = camRight.y = 0f;
                camForward.Normalize(); camRight.Normalize();

                rollDirection = (camForward * smoothInput.y + camRight * smoothInput.x).normalized;
            }
            else
            {
                rollDirection = transform.forward;
            }

            isRolling = true;
            rollTimer = rollDuration;
            lastRollTime = Time.time;

            animator.SetTrigger("Roll");
            Debug.Log("Roll aktiviran!");
        }
    }

    private void Update()
    {
        if (HandleRoll()) return;

        HandleInputSmoothing();
        Vector3 move = GetCameraRelativeMovement();
        float currentSpeed = HandleSprint();
        ApplyGravity();
        MoveCharacter(move, currentSpeed);
        UpdateAnimator(move);
        RotateCharacter(move);

        HandleStaminaRegen();
        UpdateStaminaSlider(); // ✅ Update slider
    }

    private void UpdateAnimator(Vector3 move)
    {
        Vector3 localMove = transform.InverseTransformDirection(move);
        animator.SetFloat("MoveX", Mathf.Clamp(localMove.x, -1f, 1f));
        animator.SetFloat("MoveZ", Mathf.Clamp(localMove.z, -1f, 1f));
        animator.SetBool("IsMoving", smoothInput.sqrMagnitude > 0.01f);
    }

    private void RotateCharacter(Vector3 move)
    {
        if (smoothInput.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    private bool HandleRoll()
    {
        if (!isRolling) return false;

        controller.Move(rollDirection * rollSpeed * Time.deltaTime);
        rollTimer -= Time.deltaTime;

        if (rollTimer <= 0f)
            isRolling = false;

        return true;
    }

    private void HandleInputSmoothing()
    {
        smoothInput = Vector2.SmoothDamp(smoothInput, rawInput, ref smoothVelocity, 1f / inputSmoothSpeed);
    }

    private Vector3 GetCameraRelativeMovement()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return camForward * smoothInput.y + camRight * smoothInput.x;
    }

    private float HandleSprint()
    {
        bool sprintHeld = Keyboard.current.leftShiftKey.isPressed;
        bool isMoving = smoothInput.sqrMagnitude > 0.01f;

        if (sprintHeld && isMoving && Stamina > 0f)
        {
            animator.SetBool("Sprint", true);

            Stamina -= MovementCost * Time.deltaTime;
            if (Stamina < 0f) Stamina = 0f;

            return speed * 1.5f;
        }
        else
        {
            animator.SetBool("Sprint", false);
            return speed;
        }
    }

    private void ApplyGravity()
    {
        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = 0f;
    }

    private void MoveCharacter(Vector3 move, float currentSpeed)
    {
        controller.Move(move * currentSpeed * Time.deltaTime + velocity * Time.deltaTime);
    }

    private void HandleStaminaRegen()
    {
        bool sprintHeld = Keyboard.current.leftShiftKey.isPressed;

        if (!isRolling && !sprintHeld)
        {
            Stamina += RegenRate * Time.deltaTime;
            if (Stamina > MaxStamina) Stamina = MaxStamina;
        }
    }

    // ✅ Update Slider
    private void UpdateStaminaSlider()
    {
        if (StaminaSlider != null)
        {
            StaminaSlider.value = Stamina;
        }
    }
}
