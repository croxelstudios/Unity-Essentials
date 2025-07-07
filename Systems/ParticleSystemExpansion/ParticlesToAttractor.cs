using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticlesToAttractor : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string targetTag = "";
    [SerializeField]
    Transform target = null;
    [SerializeField]
    [Range(0f, 1f)]
    float attractPercent = 1f;
    [SerializeField]
    bool intantAdaption = true;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) DoUpdate(timeMode.DeltaTime());
    }

    void Update()
    {
        if (timeMode.IsSmooth()) DoUpdate(timeMode.DeltaTime());
    }

    void DoUpdate(float deltaTime)
    {
        if (target == null) target = FindWithTag.Transform(targetTag);

        if (target != null)
        {
            int amount = ps.GetParticles(particles, ps.particleCount);
            if (intantAdaption)
                for (int i = 0; i < amount; i++)
                {
                    Vector3 vel = particles[i].velocity;
                    particles[i].position = Vector3.SmoothDamp(
                        particles[i].position, target.position, ref vel,
                        particles[i].remainingLifetime / attractPercent,
                        Mathf.Infinity, deltaTime);
                    particles[i].velocity = vel;
                }
            else
                for (int i = 0; i < amount; i++)
                {
                    Vector3 vel = particles[i].velocity;
                    particles[i].position = Vector3.SmoothDamp(
                        particles[i].position, target.position, ref vel,
                        particles[i].startLifetime / attractPercent, Mathf.Infinity, deltaTime);
                    particles[i].velocity = vel;
                }
            ps.SetParticles(particles);
        }
    }
}
