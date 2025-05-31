using System;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IVacuumable
{
    public bool CanBeVacuumed => true;
    public float VacuumResistance => 0.3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = 0f; // Hızlı çekilmesi için
            rb.angularDamping = 0f;
        }
    }

    public void OnVacuumStart()
    {
    }

    public void OnVacuumEnd()
    {
    }

    public void OnVacuumPull(Vector3 direction, float force)
    {
        if (rb != null)
            rb.AddForce(direction * force * (1f - VacuumResistance));
    }
}