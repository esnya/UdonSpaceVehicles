using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Respawn Trigger")]
    [HelpMessage("Interact or call Trigger event to respawn.")]
    public class RespawnTrigger : UdonSharpBehaviour
    {
        #region Public Variables
        public Transform target;
        public bool networked = true;
        #endregion

        #region Internal Variables
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Rigidbody[] targetRigidbodies;
        #endregion

        #region Unity Events
        private void Start()
        {
            targetRigidbodies = target.GetComponentsInChildren<Rigidbody>();
            initialPosition = target.position;
            initialRotation = target.rotation;
        }
        #endregion

        #region Udon Events
        public override void Interact()
        {
            if (networked) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Trigger));
            else Trigger();
        }
        #endregion

        #region Custom Events
        public void Trigger()
        {
            foreach (var rigidbody in targetRigidbodies) {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            target.position = initialPosition;
            target.rotation = initialRotation;
        }
        #endregion
    }
}
