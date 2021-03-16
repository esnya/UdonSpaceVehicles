
using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    public class UdonLogger : UdonSharpBehaviour
    {
        #region Public Variables
        public TextMeshPro text;
        public int maxLines = 20;
        #endregion

        #region Unity Events
        private string logs = "";
        private bool initialized = false;
        private void Start()
        {
            initialized = true;
            Log("Info", gameObject.name, "Initialized");
        }
        #endregion

        void AppendLine(string line)
        {
            logs += string.IsNullOrEmpty(logs) ? line : $"\n{line}";
        }

        #region Custom Events
        public void Log(string level, string module, string message)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var logLine = $"{level} {time} [{module}] {message}";
            Debug.Log(logLine);

            if (!initialized) return;

            AppendLine(logLine);

            var lines = logs.Split('\n');
            if (lines.Length > maxLines)
            {
                logs = "";
                for (int i = lines.Length - maxLines; i < lines.Length; i++) AppendLine(lines[i]);
            }
            if (text != null) text.text = logs;
        }
        #endregion
    }
}
