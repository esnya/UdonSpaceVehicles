
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Engine Driver")]
    public class EngineDriver : UdonSharpBehaviour
    {
        #region Public Variables
        public ControllerInput controllerInput;
        [ListView("Engines / Powers")][UTEditor] public ConstantForce[] engines = {};
        [ListView("Engines / Powers")][UTEditor] public float[] powers = {};
        #endregion

        #region Logics
        #endregion

        #region Unity Events
        int engineCount;
        Animator[] animators;
        Vector3[] axises;
        void Start()
        {
            engineCount = Mathf.Min(engines.Length, powers.Length);

            axises = new Vector3[engineCount];
            animators = new Animator[engineCount];
            for (int i = 0; i < engineCount; i++) {
                var engine = engines[i];
                axises[i] = transform.InverseTransformVector(engine.transform.forward);

                var animator = engine.GetComponentInChildren<Animator>();
                animators[i] = (animator == null) ? null : animator;
            }
        }
        #endregion

        #region Udon Events
        void Update()
        {
            if (!active) return;

            var input = controllerInput.input;
            for (int i = 0; i < engineCount; i++) {
                var normalizedPower =Vector3.Dot(input, axises[i]);
                engines[i].relativeForce =  Vector3.forward *  normalizedPower * powers[i];

                var animator = animators[i];
                if (animator != null) animator.SetFloat("Power", normalizedPower);
            }
        }
        #endregion

        #region Custom Events
        #endregion

        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;
        }

        public void Deactivate()
        {
            active = false;
        }
        #endregion
    }
}
