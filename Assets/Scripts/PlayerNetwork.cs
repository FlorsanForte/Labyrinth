using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] public GameObject spawnedObjectPrefab;
    [SerializeField] private float playerSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private AudioListener listener;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float verticalRotationSpeed = 0.1f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float zoomFOV = 40f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 2f;

    public CharacterController cc;
    private MyPlayerInput playerInput;
    public NetworkVariable<float> velocity = new NetworkVariable<float>();

    private WeaponController weaponController;
    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocityMovement;
    private bool isGrounded;
    private float accumulatedRotation;
    private float currentStamina;
    private float staminaRegenTimer;

    [SerializeField] private Animator animator;

    private void Start()
    {
        playerInput = new MyPlayerInput();
        playerInput.Enable();

        weaponController = GetComponentInChildren<WeaponController>();
        playerInput.Player.LeftClick.performed += _ => weaponController.Fire();
        playerInput.Player.Zoom.started += _ => Zoom(true);
        playerInput.Player.Zoom.canceled += _ => Zoom(false);

        currentStamina = maxStamina;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            listener.enabled = true;
            vc.Priority = 1;
        }
        else
        {
            vc.Priority = 0;
        }
    }

    private void Update()
    {
        Vector2 moveInput = playerInput.Player.Movement.ReadValue<Vector2>();
        bool jumpInput = playerInput.Player.Jump.triggered;
        bool sprintInput = playerInput.Player.Sprint.IsPressed();
        Vector2 lookInput = playerInput.Player.LookAround.ReadValue<Vector2>();

        if (IsServer && IsLocalPlayer)
        {
            Move(moveInput, jumpInput, sprintInput);
            LookAround(lookInput);
            HandleStamina(sprintInput);
        }
        else if (IsClient && IsLocalPlayer)
        {
            MoveServerRpc(moveInput, jumpInput, sprintInput);
            LookAroundServerRpc(lookInput);
            HandleStaminaServerRpc(sprintInput);
        }
    }

    private void HandleStamina(bool isSprinting)
    {
        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
            staminaRegenTimer = 0f;
        }
        else
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
    }

    private void Move(Vector2 _input, bool _jump, bool _sprint)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocityMovement.y < 0)
        {
            velocityMovement.y = -2f;
        }

        bool canSprint = _sprint && currentStamina > 0;
        float finalSpeed = canSprint ? speed * sprintMultiplier : speed;
        Vector3 move = _input.x * playerTransform.right + _input.y * playerTransform.forward;

        float targetMoveX = _input.x;
        float targetMoveY = _input.y;

        float smoothMoveX = Mathf.Lerp(animator.GetFloat("MoveX"), targetMoveX, 0.1f);
        float smoothMoveY = Mathf.Lerp(animator.GetFloat("MoveY"), targetMoveY, 0.1f);

        animator.SetFloat("MoveX", smoothMoveX);
        animator.SetFloat("MoveY", smoothMoveY);
        
        cc.Move(move * finalSpeed * Time.deltaTime);

        if (_jump && isGrounded)
        {
            velocityMovement.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocityMovement.y += gravity * Time.deltaTime;

        cc.Move(velocityMovement * Time.deltaTime);
    }

    private void Zoom(bool isZooming)
    {
        vc.m_Lens.FieldOfView = isZooming ? zoomFOV : normalFOV;
    }

    [ServerRpc]
    private void LookAroundServerRpc(Vector2 _input)
    {
        LookAround(_input);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 _input, bool _jump, bool _sprint)
    {
        Move(_input, _jump, _sprint);
    }

    [ServerRpc]
    private void HandleStaminaServerRpc(bool isSprinting)
    {
        HandleStamina(isSprinting);
    }

    private void LookAround(Vector2 _input)
    {
        float rotationAmount = _input.x * rotationSpeed;
        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);
    }
}
