
using UdonSharp;
using UdonToolkit;
using UnityEngine;

namespace UdonSpaceVehicles
{
    [CustomName("USV Particle Driver")]
    [HelpMessage("Emits particle by custom event \"Trigger\".")]
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleDriver : UdonSharpBehaviour
    {
        public int count = 1;
        public void Trigger() {
            GetComponent<ParticleSystem>().Emit(count);
        }
    }
}
