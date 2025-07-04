using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystem_KillOnPlanes : MonoBehaviour
{
    [SerializeField]
    Transform[] planes = null;

    ParticleSystem sys;
    ParticleSystem.Particle[] particles;

    private void Awake()
    {
        sys = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[sys.main.maxParticles];
    }

    void LateUpdate()
    {
        int count = sys.particleCount;
        sys.GetParticles(particles, count);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = particles[i].position;
            bool kill = false;
            foreach (Transform plane in planes)
            {
                Vector3 dif = position - plane.position;
                if (Vector3.Dot(dif, plane.up) < 0f)
                {
                    kill = true;
                    break;
                }
            }
            if (kill)
                particles[i].remainingLifetime = 0f; // Kill the particle
        }
        sys.SetParticles(particles, count);
    }
}
