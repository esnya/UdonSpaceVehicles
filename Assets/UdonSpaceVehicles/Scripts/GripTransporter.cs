using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Common;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV Grip Transporter")]
    [HelpMessage("Linear personal transporter with a grip.")]
    [RequireComponent(typeof(Rigidbody))]
    public class GripTransporter : UdonSharpBehaviour
    {
        [Tooltip("m/s")] public float speed = 5.0f;
        public float speedCurve = 2.0f;
        [Tooltip("m")] public float length = 2.0f;
        public AudioSource audioSource;
        public float audioVolume = 1.0f;
        public float audioVolumeCurve = 0.5f;
        [Range(-3, 3)] public float audioPitch = 1.0f;
        [Range(-3, 3)] public float audioPitchStart = 0.5f;
        public float audioPitchCurve = 0.5f;

        private int state;
        private VRC_Pickup grip;
        private void Start()
        {
            grip = (VRC_Pickup)GetComponentInChildren(typeof(VRC_Pickup));

            if (audioSource != null)
            {
                audioSource.loop = true;
                audioSource.volume = 0;
                audioSource.pitch = audioPitchStart;
            }
        }


        private float Gain(float x, float k)
        {
            var a = 0.5f * Mathf.Pow(2.0f * (x < 0.5f ? x : 1.0f - x), k);
            return (x < 0.5f) ? a : 1.0f - a;
        }
        private float Parabola(float x, float k)
        {
            return Mathf.Pow(4.0f * x * (1.0f - x), k);
        }

        private float GetScaledTime(float time)
        {
            return (time - startTime) / (length / speed);
        }

        private Vector3 GetPosition(float t)
        {
            return Vector3.forward * Gain(t, speedCurve) * length;;
        }

        private float startTime;
        private void Update()
        {
            if (state == 0) return;

            var t = GetScaledTime(Time.time);

            if (t < 1.0f)
            {
                transform.localPosition = Vector3.forward * Gain(t, speedCurve) * length;
            }
            else if (t < 2.0f)
            {
                if (state == 1)
                {
                    state = 2;
                    _Exit();
                }
                transform.localPosition = Vector3.forward * Gain(2.0f - t, speedCurve) * length;
            }
            else
            {
                state = 0;
                if (audioSource != null) audioSource.Stop();
            }

            if (audioSource != null)
            {
                audioSource.volume = audioVolume * Parabola(t % 1.0f, audioVolumeCurve);
                audioSource.pitch = Parabola(t % 1.0f, audioPitchCurve) * (audioPitch - audioPitchStart) + audioPitchStart;
            }
        }

        private Vector3 GetVelocity()
        {
            var p1 = GetPosition(GetScaledTime(Time.time));
            var p2 = GetPosition(GetScaledTime(Time.time - 1.0f));
            return transform.TransformVector(p1 - p2);
        }

        void LateUpdate()
        {
            if (!gripped) return;

            var player = Networking.LocalPlayer;
            player.SetVelocity((transform.position - grip.transform.position) / Time.deltaTime + GetVelocity());
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (gripped && value)
            {
                _Exit();

                var player = Networking.LocalPlayer;
                player.SetVelocity(player.GetVelocity() + Vector3.up * player.GetJumpImpulse());
            }
        }

        private bool gripped;
        public void _Enter()
        {
            gripped = true;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayAnimation));
        }

        public void _Exit()
        {
            gripped = false;
            grip.Drop();
            grip.transform.localPosition = Vector3.zero;
            grip.transform.localRotation = Quaternion.identity;
        }

        public void PlayAnimation()
        {
            startTime = Time.time;
            state = 1;
            if (audioSource != null) audioSource.Play();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos() {
            this.UpdateProxy();
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.parent.position, transform.forward * length);
        }
#endif
    }
}
