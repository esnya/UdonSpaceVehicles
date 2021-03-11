
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Respawn Trigger")][HelpMessage("Interact or call Trigger event to respawn.")]
    public class RespawnTrigger : UdonSharpBehaviour
    {
        #region Public Variables
        public Transform target;
        public bool interactable = true;
        public bool sendToOwner = true;
        #endregion

        #region Internal Variables
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Rigidbody targetRigidbody;
        #endregion

        #region Unity Events
        private void Start() {
            targetRigidbody = target.GetComponent<Rigidbody>();
            initialPosition = target.position;
            initialRotation = target.rotation;
        }
        #endregion

        #region Udon Events
        public override void Interact() {
            if (interactable) Trigger();
        }
        #endregion

        #region Custom Events
        public void Trigger() {
            if (sendToOwner && !Networking.IsOwner(target.gameObject)) {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Trigger));
                return;
            } else {
                if (targetRigidbody != null) {
                    targetRigidbody.velocity = Vector3.zero;
                    targetRigidbody.angularVelocity = Vector3.zero;
                }

                target.position = initialPosition;
                target.rotation = initialRotation;

                if (targetRigidbody != null) {
                    targetRigidbody.Sleep();
                }
            }

        }
        #endregion
    }
}
