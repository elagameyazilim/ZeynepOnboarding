using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 4.0f;
    private float rotationSpeed = 5.0f;
    public Vector3 startPosition;
    public Transform stackPoint;
    private StackSystem stackSystem;

    private float dropInterval = 0.15f; 
    private float nextDropTime = 0.0f; 
    
    public float jumpForce = 2.5f; // Zıplama kuvveti
    private bool hasJumped = false;
    
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

    private void Jump()
    {
        if (!hasJumped)
        {
            // Zıplama kuvveti uygula
            FreezeYPosition(false);
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
        LayerMask groundLayer = LayerMask.GetMask("Ground");
        return Physics.Raycast(transform.position, Vector3.down, 1.5f, groundLayer);
    }

    private void FreezeYPosition(bool freeze)
    {
        if (freeze)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; 
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
