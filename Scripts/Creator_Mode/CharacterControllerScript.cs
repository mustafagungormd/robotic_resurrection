using UnityEngine;
using Cinemachine;

public class CharacterControllerScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float verticalSpeed = 3f;
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraFollowTarget;  // Eklendi: Kamera takip hedefi
    public float lookSensitivity = 0.06f; // Eklendi: Bakış hassasiyeti

    private CharacterController characterController;
    private Transform cameraTransform;
    private float xRotation, yRotation;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        xRotation = 0;
        yRotation = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float speed = Input.GetKey(KeyCode.LeftAlt) ? sprintSpeed : moveSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0, vertical);
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            move.y = -verticalSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            move.y = verticalSpeed * Time.deltaTime;
        }

        characterController.Move(move * speed * Time.deltaTime);

        // Fare hareketleri ile kamera rotasyonu
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRotation -= mouseY * lookSensitivity;
        yRotation += mouseX * lookSensitivity;
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        cameraFollowTarget.rotation = rotation;

        // Karakterin kamera yönüne göre dönmesi
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraFollowTarget.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
