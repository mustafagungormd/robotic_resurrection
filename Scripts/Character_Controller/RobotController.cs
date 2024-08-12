using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _attackAction; // Attack input action
    
    private Vector3 platformVelocity;
    private Transform currentPlatform;
    private Dictionary<Transform, Vector3> previousPlatformPositions = new Dictionary<Transform, Vector3>();
    [SerializeField] private LayerMask platformMask;

    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float sprintSpeed = 16;
    [SerializeField] private GameObject mainCam;
    [SerializeField] private Transform cameraFollowTarget;
    [SerializeField] private float lookSensitivity = 0.06f;
    [SerializeField] private float jumpHeight = 10.0f;
    [SerializeField] private Transform groundCheck;  // Ground check object
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);  // Ground check size
    [SerializeField] private LayerMask groundMask;  // Ground layer

    private float xRotation, yRotation;
    private float currentSpeed = 0;
    private float speedSmoothVelocity;
    [SerializeField] private float speedSmoothTime = 0.1f;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -20f;
    private bool isDead = false; // Variable to check if the player is dead

    private bool isJumping = false;
    private bool isFalling = false;
    
    
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float throwForce = 100f;
    public float throwDelay = 0.17f;
    
    // private bool isCursorLocked = true;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _lookAction = _playerInput.actions["Look"];
        _sprintAction = _playerInput.actions["Sprint"];
        _attackAction = _playerInput.actions["Attack"]; // Attack input action

        xRotation = 0;
        yRotation = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // SetCursorState(true);
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        groundedPlayer = Physics.CheckBox(groundCheck.position, groundCheckSize / 2, Quaternion.identity, groundMask | platformMask);

        bool isJumpLanding = _animator.GetCurrentAnimatorStateInfo(0).IsName("JumpLand");

        Vector3 slopeNormal;
        bool isOnSlope = IsOnSlope(out slopeNormal);

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            _animator.SetBool("Grounded", true);
            _animator.SetBool("FreeFall", false);
            _animator.SetBool("Jump", false);
            isJumping = false;
            isFalling = false;
        }
        else if (!groundedPlayer && !isOnSlope)
        {
            _animator.SetBool("Grounded", false);
        }

        Vector2 inputMove = _moveAction.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(inputMove.x, 0, inputMove.y);
        float targetRotation = 0;
        float targetSpeed = 0;

        if (inputMove != Vector2.zero && !isJumpLanding)
        {
            bool isSprinting = _sprintAction.IsPressed();
            targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
            targetRotation = Quaternion.LookRotation(inputDirection).eulerAngles.y + mainCam.transform.rotation.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, targetRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 20 * Time.deltaTime);
        }

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
        float animationSpeed = (inputMove != Vector2.zero) ? currentSpeed / sprintSpeed * 6 : 0;
        _animator.SetFloat("Speed", animationSpeed);

        Vector3 targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        if (inputMove != Vector2.zero && !isJumpLanding)
        {
            if (isOnSlope)
            {
                targetDirection = Vector3.ProjectOnPlane(targetDirection, slopeNormal).normalized;
                _controller.Move(targetDirection * (currentSpeed * 0.75f) * Time.deltaTime); // Speed is reduced slightly on slopes for better control
            }
            else
            {
                _controller.Move(targetDirection * currentSpeed * Time.deltaTime);
            }
        }

        if (groundedPlayer && _jumpAction.triggered)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            _animator.SetBool("Jump", true);
            isJumping = true;
        }
        
        // Player movement including platform velocity
        Vector3 totalVelocity = playerVelocity + platformVelocity;
        _controller.Move(totalVelocity * Time.deltaTime);
        

        if (!groundedPlayer && !isOnSlope && !isJumping)
        {
            _animator.SetBool("FreeFall", true);
            isFalling = true;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        _controller.Move(playerVelocity * Time.deltaTime);

        if (_attackAction.triggered)
        {
            _animator.SetTrigger("Attack");
            StartCoroutine(ThrowProjectileWithDelay());
        }
        
        
        // Update platform velocity
        if (currentPlatform != null)
        {
            Vector3 previousPosition;
            if (previousPlatformPositions.TryGetValue(currentPlatform, out previousPosition))
            {
                platformVelocity = (currentPlatform.position - previousPosition) / Time.deltaTime;
                previousPlatformPositions[currentPlatform] = currentPlatform.position;
            }
            else
            {
                previousPlatformPositions[currentPlatform] = currentPlatform.position;
                platformVelocity = Vector3.zero;
            }
        }
        
        //  if (Input.GetKeyDown(KeyCode.Escape))
        //  {
        //      isCursorLocked = !isCursorLocked;
        //      SetCursorState(isCursorLocked);
        //  }
        
        
        
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    
    
  //  private void SetCursorState(bool isLocked)
  //  {
  //      if (isLocked)
  //      {
  //          Cursor.lockState = CursorLockMode.Locked;
  //          Cursor.visible = false;
  //      }
  //      else
  //      {
  //          Cursor.lockState = CursorLockMode.None;
  //          Cursor.visible = true;
  //      }
  //  }
    

    void CameraRotation()
    {
        Vector2 inputLook = _lookAction.ReadValue<Vector2>();
        xRotation -= inputLook.y * lookSensitivity;
        yRotation += inputLook.x * lookSensitivity;
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        cameraFollowTarget.rotation = rotation;
    }
    
    IEnumerator ThrowProjectileWithDelay()
    {
        yield return new WaitForSeconds(throwDelay);
        ThrowProjectile();
    }
    
    void ThrowProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = throwPoint.forward * throwForce;
    }

    // Function to handle death
    public void Die()
    {
        isDead = true;
        _animator.SetTrigger("Die");
        _controller.enabled = false; // Disable character controller to stop movement
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second
        isDead = false;
        _animator.SetBool("FreeFall", true); // Trigger the respawn animation
        _controller.enabled = true; // Enable character controller
    }

    // OnTriggerEnter function to detect collision with DMG object
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DMG"))
        {
            Debug.Log("Temas Etti");
            Die();
        }
    }
    
    private bool IsOnSlope(out Vector3 slopeNormal)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _controller.height / 2 * 1.5f, groundMask))
        {
            slopeNormal = hit.normal;
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Debug.Log("Slope Angle: " + slopeAngle);
            return slopeAngle > 0 && slopeAngle <= _controller.slopeLimit;
        }
        slopeNormal = Vector3.up;
        Debug.Log("No slope detected");
        return false;
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((platformMask.value & (1 << hit.gameObject.layer)) > 0)
        {
            if (currentPlatform != hit.transform)
            {
                currentPlatform = hit.transform;
                if (!previousPlatformPositions.ContainsKey(currentPlatform))
                {
                    previousPlatformPositions[currentPlatform] = currentPlatform.position;
                }
                platformVelocity = Vector3.zero;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentPlatform)
        {
            platformVelocity = Vector3.zero;
            currentPlatform = null;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform == currentPlatform)
        {
            platformVelocity = Vector3.zero;
            currentPlatform = null;
        }
    }
}
