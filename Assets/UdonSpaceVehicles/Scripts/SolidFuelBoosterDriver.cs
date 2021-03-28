
using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [CustomName("USV Solid Fuel Booster Driver")]
    [HelpMessage("Used-up acceleration booster. Send custom event \"Trigger\" to ignite. Updates an animatior float parameter \"Power\" on children with sync.")]
    public class SolidFuelBoosterDriver : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("References")]
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        public float force = 5000;
        public float burningTime = 5;
        public int ignitionCount = 1;
        #endregion

        #region Logics
        private void SetAnimation()
        {
            if (animator == null) return;
            animator.SetFloat("Power", burning ? 1 : 0);
        }
        #endregion

        #region Unity Events
        [UdonSynced] private bool burning;
        private Animator animator;
        private int ignished;
        private void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
        }

        private void FixedUpdate() {
            if (!active || !burning) return;

            target.AddForceAtPosition(transform.forward * force, transform.position);
        }
        #endregion

        #region Udon Events
        private bool prevBurning;
        public override void OnDeserialization()
        {
            if (burning != prevBurning) SetAnimation();
            prevBurning = burning;
        }
        #endregion

        #region Custom Events
        public void _Respawned()
        {
            burning = false;
            SetAnimation();
            ignished = 0;
        }

        public void _Extinguish()
        {
            burning = false;
            SetAnimation();
            Log("Info", "Extinguished");
        }

        public void Trigger()
        {
            if (!active || ignished >= ignitionCount) return;

            SendCustomEventDelayedSeconds(nameof(_Extinguish), burningTime);
            burning = true;
            SetAnimation();
            ignished++;
            Log("Info", "Ignished");
        }
        #endregion

        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            active = false;
            Log("Info", "Deactivated");
        }
        #endregion

        #region Logger
        [SectionHeader("Udon Logger")] public bool useGlobalLogger = false;
        [HideIf("@useGlobalLogger")] public UdonLogger logger;

        private void Log(string level, string message)
        {
            if (logger == null && useGlobalLogger) logger = (UdonLogger)GameObject.Find("_USV_Global_Logger_").GetComponent(typeof(UdonBehaviour));

            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion
    }
}
