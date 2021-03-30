
using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [CustomName("USV Udon Logger")]
    [HelpMessage("Formatted logger. Requires TMPro.TextMeshPro.")]
    [RequireComponent(typeof(TextMeshPro))]
    public class UdonLogger : UdonSharpBehaviour
    {
        #region Public Variables
        public int maxLines = 20;
        [ListView("Log Levels")] public string[] levels = {
            "Debug",
            "Info",
            "Warn",
            "Error",
            "Notice",
            "Fatal",
        };
        [ListView("Log Levels")] public string[] colors = {
            "gray",
            "green",
            "yellow",
            "red",
            "blue",
            "red",
        };

        [Popup("@levels")] public string consoleLevel;
        [Popup("@levels")] public string textLevel;

        [ListView("Relay Targets")] public UdonLogger[] relayTargets = {};
        [ListView("Relay Targets")][Popup("@levels")] public string[] relayLevels = {};

        public bool relayToGlobalLogger = false;
        [HideIf("@!relayToGlobalLogger")][Popup("@levels")] public string relayToGlobalLoggerLevel;
        #endregion

        #region Unity Events
        private TextMeshPro text;
        private string logs = "";
        private bool initialized = false;
        private int consoleLevelIndex, textLevelIndex, levelCount, relayTargetCount, relayToGlobalLoggerLevelIndex;
        private int[] relayLevelIndices;
        private UdonLogger globalLogger;
        private void Start()
        {
            text = GetComponent<TextMeshPro>();

            levelCount = Mathf.Min(levels.Length, colors.Length);
            relayTargetCount = Mathf.Min(relayTargets.Length, relayLevels.Length);
            relayLevelIndices = new int[relayTargetCount];
            if (relayToGlobalLogger) globalLogger = (UdonLogger)GameObject.Find("_USV_Global_Logger_").GetComponent(typeof(UdonBehaviour));
            relayToGlobalLoggerLevelIndex = int.MaxValue;

            for (int i = 0; i < levelCount; i++)
            {
                var level = levels[i];
                if (level == consoleLevel) consoleLevelIndex = i;
                if (level == textLevel) textLevelIndex = i;

                for (int j = 0; j < relayTargetCount; j++)
                {
                    if (levels[j] == relayLevels[i])
                    {
                        relayLevelIndices[j] = i;
                    }
                }

                if (globalLogger != null && level == relayToGlobalLoggerLevel) relayToGlobalLoggerLevelIndex = i;
            }

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
            int levelIndex;
            for (levelIndex = 0; levelIndex < levelCount && levels[levelIndex] != level; levelIndex++) {}
            var color = colors[levelIndex];

            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var logLine = $"<color={color}>{level}</color> {time} [{module}] {message}";
            if (levelIndex >= consoleLevelIndex) Debug.Log(logLine);

            if (!initialized) return;

            for (int i = 0; i < relayTargetCount; i++)
            {
                if (levelIndex <= relayLevelIndices[i] && relayTargets[i]) relayTargets[i].Log(level, module, name);
            }
            if (levelIndex >= relayToGlobalLoggerLevelIndex) globalLogger.Log(level, module, message);

            if (levelIndex < textLevelIndex) return;

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
