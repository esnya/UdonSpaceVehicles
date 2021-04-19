
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [RequireComponent(typeof(VRCSpatialAudioSource))]
    public class TouchButton : UdonSharpBehaviour
    {
        public bool interactable = true;
        [ListView("Event Target")] public UdonSharpBehaviour[] targets = {};
        [ListView("Event Target")][Popup("behaviour", "@targets")] public string[] events = {};
        public AudioClip onPressed, onReleased;
        public float interval = 0.5f;

        private int targetCount;
        private AudioSource audioSource;
        private void Start()
        {
            targetCount = Mathf.Min(targets.Length, events.Length);
            audioSource = GetComponent<AudioSource>();
        }

        private bool isVR;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) {
                isVR = player.IsUserInVR();
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip);
        }

        public void SendEvents()
        {
            for (int i = 0; i < targetCount; i++)
            {
                var target = targets[i];
                if (target != null) target.SendCustomEvent(events[i]);
            }
        }

        [HideInInspector] public TouchSource touchSource;

        private float sleepUntil = 0.0f;
        public void TouchEnter()
        {
            if (!isVR) return;

            var time = Time.time;
            if (time < sleepUntil) return;
            sleepUntil = time + interval;

            PlayClip(onPressed);
            if (touchSource != null) touchSource.PlayHaptic(1.0f);
            SendEvents();
        }

        public void TouchEnd()
        {
            if (!isVR) return;
            PlayClip(onReleased);
            if (touchSource != null) touchSource.PlayHaptic(0.6f);
        }

        public override void Interact()
        {
            if (!interactable) return;
            PlayClip(onPressed);
            SendEvents();
        }
    }
}
