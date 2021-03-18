
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("Kinetic Driver")][HelpMessage("Add or set force or velocity when custom event \"Trigger\" received.")]
    public class KineticDriver : UdonSharpBehaviour
    {
        public Rigidbody target;
        [Horizontal("Force")][Toggle] public bool addForce;
        [Horizontal("Force")][HideIf("@!addForce")] public Vector3 force;

        [Horizontal("Velocity")][Toggle] public bool setVelocity;
        [Horizontal("Velocity")][HideIf("@!setVelocity")] public Vector3 velocity;
        [Horizontal("Options")][Toggle] public bool localSpace, ownerOnly;
        [Horizontal("Triggers")][Toggle] public bool onStart, onUpdate;

        public float timeScale = 1.0f, lengthScale = 1.0f;

        private void Start() {
            if (onStart) Trigger();
        }

        private void Update()
        {
            if (onUpdate) Trigger();
        }

        private Vector3 ConvertSpace(Vector3 vector)
        {
            return localSpace ? target.transform.TransformVector(vector) : vector;
        }

        public void Trigger()
        {
            if (ownerOnly && Networking.IsOwner(target.gameObject)) return;

            if (addForce) target.AddForce(ConvertSpace(force) * timeScale * timeScale / lengthScale, ForceMode.Force);
            if (setVelocity) target.AddForce(ConvertSpace(velocity) * timeScale / lengthScale, ForceMode.VelocityChange);
        }
    }
}
