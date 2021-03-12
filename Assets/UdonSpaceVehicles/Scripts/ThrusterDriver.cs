
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Thruster Driver")]
    public class ThrusterDriver : UdonSharpBehaviour
    {
        #region Public Variables
        public RCSController rcsController;
        public ConstantForce[] thrusters;
        public float thrustPower = 20.0f;
        [Range(0.0f, 1.0f)] public float thrustThreshold = 0.5f;
        #endregion

        #region Internal Variables
        private int thrusterCount;
        private Vector3[] thrusterRotationAxises, thrusterTranslationAxises;
        private Animator[] thrusterAnimators;
        #endregion

        #region Logics
        private void SetThrust(int i, bool thrust) {
            thrusters[i].relativeForce = thrust ? -Vector3.forward * thrustPower : Vector3.zero;
            if (thrusterAnimators[i] != null) thrusterAnimators[i].SetFloat("Power", thrust  ? 1 : 0);
        }
        #endregion

        #region Unity Events
        private void Start() {
            var center = transform.position;

            thrusterCount = thrusters.Length;
            thrusterAnimators = new Animator[thrusterCount];
            thrusterRotationAxises = new Vector3[thrusterCount];
            thrusterTranslationAxises = new Vector3[thrusterCount];
            for (int i = 0; i < thrusterCount; i++) {
                var thruster = thrusters[i];

                var centerToThruster = (thruster.transform.position - center).normalized;
                var thrusterForward = thruster.transform.TransformDirection(Vector3.forward);
                thrusterRotationAxises[i] = transform.InverseTransformDirection(Vector3.Cross(thrusterForward, centerToThruster).normalized);
                thrusterTranslationAxises[i] = -transform.InverseTransformDirection(thrusterForward.normalized);

                thrusterAnimators[i] = thruster.GetComponentInChildren<Animator>();
            }
        }

        private void Update() {
            if (!active) return;

            var rotationInput = rcsController.rotation;
            var translationInput = rcsController.translation;

            for (int i = 0; i < thrusterCount; i++) {
                var rotation = Mathf.Clamp01(Vector3.Dot(thrusterRotationAxises[i], rotationInput));
                var translation = Mathf.Clamp01(Vector3.Dot(thrusterTranslationAxises[i], translationInput));

                var thrust = rotation + translation >= thrustThreshold;
                SetThrust(i, thrust);
            }
        }
        #endregion

        #region Udon Events
        #endregion

        #region Custom Events
        #endregion

        #region Internal Logics
        #endregion

        #region Activatable
        private bool active;
        public void Activate() {
            active = true;
        }

        public void Dectivate() {
            active = false;

            for (int i = 0; i < thrusterCount; i++) {
                SetThrust(i, false);
            }
        }
        #endregion

    }
}
