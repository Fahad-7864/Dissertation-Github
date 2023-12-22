using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NewBehaviourScript : MonoBehaviour
{
    public GameObject user;
    public GameObject enemy;
    public float radius = 3.0f;
    public ParticleSystem meteorParticles;

    private void Start()
    {
        meteorParticles = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnMeteorsOnTargets();
        }
    }

    private void SpawnMeteorsOnTargets()
    {
        Vector3 userPosition = user.transform.position;
        Vector3 enemyPosition = enemy.transform.position;

        SpawnMeteorsAround(userPosition);
        SpawnMeteorsAround(enemyPosition);
    }

    private void SpawnMeteorsAround(Vector3 targetPosition)
    {
        Vector3 randomOffset = Random.insideUnitSphere * radius;
        randomOffset.y = 0; // Keep meteors on the same Y plane

        Vector3 spawnPosition = targetPosition + randomOffset;
        ParticleSystem newParticles = Instantiate(meteorParticles, spawnPosition, Quaternion.identity);
        newParticles.Play();
        Destroy(newParticles.gameObject, newParticles.main.duration);
    }
}
