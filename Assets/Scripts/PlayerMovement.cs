using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 startPosition;
    public Transform stackPoint;
    public float jumpForce;
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private Vector3 rayDirection;
    [SerializeField]
    private Vector3 rayPositionOffset;
    [SerializeField]
    private float rayDistance;
    
    private Rigidbody rb;
    [SerializeField]
    private float speed = 4.0f;
    [SerializeField]
    private float rotationSpeed = 5.0f;
    [SerializeField]
    private bool hasJumped = false;
    
    private StackSystem stackSystem;

    private float dropInterval = 0.15f; 
    private float nextDropTime = 0.0f; 
    
    
    
    private bool isOnWater = false;
    private float odunYuksekligi = 0.15f;

    private Transform waterSurface;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        stackSystem = GetComponent<StackSystem>();
        waterSurface = GameObject.FindGameObjectWithTag("Water")?.transform;

        Debug.Log("Player initialized at position: " + startPosition);
    }

    void FixedUpdate()
    {
        // Player ileri hareket
        Vector3 forwardMovement = transform.forward * speed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement); 

        // Dokunma ile sağa/sola döndürme
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, deltaX, 0);
                rb.MoveRotation(rb.rotation * rotation);
            }
        }
    }

    private void Update()
    {
        // Eğer zemin üzerinde değilse ve suda değilse
        if (!IsGrounded() && !isOnWater)
        {
            if (stackSystem != null && stackSystem.GetWoodCount() > 0) 
            {
                FreezeYPosition(true);

                if (Time.time >= nextDropTime)
                {
                    GameObject wood = stackSystem.RemoveWood();
                    if (wood != null)
                    {
                        wood.transform.position = new Vector3(transform.position.x, waterSurface.position.y + odunYuksekligi, transform.position.z);
                        wood.transform.rotation = transform.rotation;
                        wood.SetActive(true);
                    }
                    nextDropTime = Time.time + dropInterval;
                }
            }
            else
            {
                FreezeYPosition(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Collectible"))
        {
            Destroy(other.gameObject); 
            if (stackSystem != null)
            {
                stackSystem.AddWood(1);
            }
        }
        else if (other.CompareTag("Booster"))
        {
            Debug.Log("Player picked up booster.");
            Jump(); 
        }
        else if (other.CompareTag("WaterTrigger"))
        {
            isOnWater = true; 
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterTrigger"))
        {
            isOnWater = false;
        }
    }
    [Button]
    private void Jump()
    {
        if (!hasJumped)
        {
            FreezeYPosition(false);
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            hasJumped = true;

            Debug.Log("Player jumped.");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            hasJumped = false; 
            FreezeYPosition(true);
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            rb.rotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0);
        }
    }

    private bool IsGrounded()
    {
        var value = Physics.Raycast(transform.position + rayPositionOffset, rayDirection, rayDistance, groundLayer);
        if (value)
        {
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("On water");
        }

        return value;
    }

    
    private void FreezeYPosition(bool freeze)
    {
        if (freeze)
        {
            if (hasJumped)
            {
                return;
            } 
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; 
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
