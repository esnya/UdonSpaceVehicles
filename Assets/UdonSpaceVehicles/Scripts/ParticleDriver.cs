
using UdonSharp;
using UnityEngine;

namespace UdonSpaceVehicles
{
    public class ParticleDriver : UdonSharpBehaviour
    {
        public new ParticleSystem particleSystem;
        public int count = 1;
        public void Trigger() {
            particleSystem.Emit(count);
        }
    }
}
