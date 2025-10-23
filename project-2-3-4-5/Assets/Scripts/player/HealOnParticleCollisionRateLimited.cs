using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class HealOnParticleCollisionRateLimited : MonoBehaviour
{
    [SerializeField] int livesPerCollision = 1;
    [SerializeField] int maxLivesPerTick = 3;           // tope por llamado
    [SerializeField] float minIntervalBetweenHeals = 0.25f; // cooldown por jugador
    [SerializeField] string playerTag = "Player";

    ParticleSystem ps;
    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    Dictionary<GameObject, float> lastHealTime = new Dictionary<GameObject, float>();

    void Awake() => ps = GetComponent<ParticleSystem>();

    void OnParticleCollision(GameObject other)
    {
        if (!other.CompareTag(playerTag)) return;

        int events = ps.GetCollisionEvents(other, collisionEvents);
        if (events <= 0) return;

        float now = Time.time;
        lastHealTime.TryGetValue(other, out float last);

        if (now - last < minIntervalBetweenHeals)
            return; // aún en cooldown

        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        int healAmount = Mathf.Clamp(events * livesPerCollision, 1, maxLivesPerTick);
        ph.AddLife(healAmount);
        lastHealTime[other] = now;
    }
}
