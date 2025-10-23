using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class HealOnParticleCollision : MonoBehaviour
{
    [SerializeField] int livesPerCollision = 1;
    [SerializeField] string playerTag = "Player";

    ParticleSystem ps;
    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Awake() => ps = GetComponent<ParticleSystem>();

    void OnParticleCollision(GameObject other)
    {
        if (!other.CompareTag(playerTag)) return;

        int events = ps.GetCollisionEvents(other, collisionEvents);
        if (events <= 0) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        int totalLives = events * livesPerCollision;
        ph.AddLife(totalLives);
    }
}
