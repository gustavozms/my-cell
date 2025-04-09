using UnityEngine;
using System;

public class Draggable : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private float distanceToCamera;
    private Vector3 offset;

    [Header("Drag Settings")]
    [SerializeField] private float dragForce = 10f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float snapDistance = 0.1f;
    [SerializeField] private Color highlightColor = Color.red;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceToCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = distanceToCamera;
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            Vector2 direction = (targetPosition - transform.position);

            if (direction.magnitude < snapDistance)
            {
                rb.MovePosition(targetPosition);
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.AddForce(direction.normalized * dragForce * rb.mass);

                if (rb.linearVelocity.magnitude > maxVelocity)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
                }
            }
        }
    }

    void OnMouseDown()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = highlightColor;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = distanceToCamera;
        offset = transform.position - Camera.main.ScreenToWorldPoint(mousePosition);

        isDragging = true;
        rb.gravityScale = 0f;
    }

    void OnMouseUp()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = Color.white;

        isDragging = false;
        rb.gravityScale = 0f;
    }

    void AddGravityForce(Rigidbody2D attractor, Rigidbody2D target)
    {
        float G = 9.81f;

        float massProduct = attractor.mass * attractor.mass * G;

        Vector3 deltaDistance = target.position - attractor.position;
        float distance = deltaDistance.magnitude;

        double unscaledForceMagnitude = massProduct / Math.Pow(distance, 2);
        float forceMagnitude = Convert.ToSingle(G * unscaledForceMagnitude);

        Vector3 forceDirection = deltaDistance.normalized;
        Vector3 forceVector = forceDirection * forceMagnitude;

        target.AddForce(forceVector);
    }
}
