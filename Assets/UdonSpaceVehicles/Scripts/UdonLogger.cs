
using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;

namespace UdonSpaceVehicles
{
    [CustomName("USV Udon Logger")]
    [HelpMessage("Formatted logger. Requires TMPro.TextMeshPro.")]
    [RequireComponent(typeof(TextMeshPro))]
    public class UdonLogger : UdonSharpBehaviour
    {
        #region Public Variables
        public int maxLines = 20;
        #endregion

        #region Unity Events
        private TextMeshPro text;
        private string logs = "";
        private bool initialized = false;
        private void Start()
        {
            text = GetComponent<TextMeshPro>();
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
