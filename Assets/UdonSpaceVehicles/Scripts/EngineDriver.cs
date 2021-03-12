
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
        private void SetPower(int index, float power) {
            engines[index].relativeForce =  Vector3.forward * powers[index] * power;

            var animator = animators[index];
            if (animator != null) animator.SetFloat("Power", power);
        }
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

            Log("Initialized");
        }
        #endregion

        #region Udon Events
        private void Update()
        {
            if (!active) return;

            var input = controllerInput.input;
            for (int i = 0; i < engineCount; i++) {
                var power = Vector3.Dot(input, axises[i]);
                SetPower(i, power);
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

            Log("Activated");
        }

        public void Deactivate()
        {
            active = false;

            for (int i = 0; i < engineCount; i++) SetPower(i, 0.0f);

            Log("Deactivated");
        }
        #endregion

        #region Logger
        private void Log(string log) {
            Debug.Log($"[{gameObject.name}] {log}");
        }
        #endregion
    }
}
