using UnityEngine;
using System.Collections.Generic;

public class EnemyVacuumManager : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private float destructionDistance = 1.5f;
    [SerializeField] private GameObject destructionEffect;
    [SerializeField] private AudioClip destructionSound;

    private VacuumWeapon vacuumWeapon;
    private AudioSource audioSource;
    private List<GameObject> pulledEnemies = new List<GameObject>();

    void Start()
    {
        vacuumWeapon = GetComponent<VacuumWeapon>();
        audioSource = GetComponent<AudioSource>();

        if (vacuumWeapon == null)
        {
            return;
        }

        vacuumWeapon.OnVacuumCollidersDetected += OnVacuumHit;
        vacuumWeapon.OnVacuumStopped += OnVacuumStopped;
    }

    void Update()
    {
        if (vacuumWeapon == null) return;

        if (vacuumWeapon.IsFiring)
        {
            PullEnemies();
            CheckDestruction();
        }
    }

    private void OnVacuumHit(Collider[] colliders)
    {
        List<GameObject> currentEnemies = new List<GameObject>();

        foreach (var collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }


            IVacuumable vacuumable = collider.GetComponent<IVacuumable>();
            if (vacuumable != null)
            {
                if (vacuumable.CanBeVacuumed)
                {
                    currentEnemies.Add(collider.gameObject);

                    // Yeni enemy ise OnVacuumStart çağır
                    if (!pulledEnemies.Contains(collider.gameObject))
                    {
                        pulledEnemies.Add(collider.gameObject);
                        vacuumable.OnVacuumStart();
                    }
                }
            }
        }

        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            if (pulledEnemies[i] == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            if (!currentEnemies.Contains(pulledEnemies[i]))
            {
                GameObject enemy = pulledEnemies[i];
                IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    private void PullEnemies()
    {
        foreach (GameObject enemy in pulledEnemies)
        {
            if (enemy == null) continue;

            IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
            Vector3 direction = (vacuumWeapon.FirePoint.position - enemy.transform.position).normalized;
            float force = vacuumWeapon.PullStrength;

            vacuumable.OnVacuumPull(direction, force * Time.deltaTime);
        }
    }

    private void CheckDestruction()
    {
        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = pulledEnemies[i];
            if (enemy == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(vacuumWeapon.FirePoint.position, enemy.transform.position);

            if (distance <= destructionDistance)
            {
                DestroyEnemy(enemy);
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    private void DestroyEnemy(GameObject enemy)
    {
        if (destructionEffect != null)
            Instantiate(destructionEffect, enemy.transform.position, enemy.transform.rotation);

        if (destructionSound != null && audioSource != null)
            audioSource.PlayOneShot(destructionSound);

        Destroy(enemy);
    }

    private void OnVacuumStopped()
    {
        foreach (GameObject enemy in pulledEnemies)
        {
            if (enemy != null)
            {
                IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
            }
        }

        pulledEnemies.Clear();
    }

    void OnDestroy()
    {
        if (vacuumWeapon != null)
        {
            vacuumWeapon.OnVacuumCollidersDetected -= OnVacuumHit;
            vacuumWeapon.OnVacuumStopped -= OnVacuumStopped;
        }
    }
}