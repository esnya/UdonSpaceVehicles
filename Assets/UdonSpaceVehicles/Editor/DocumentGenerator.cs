using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using UdonSharp;
using UdonToolkit;
using System.IO;
using System.Text.RegularExpressions;

namespace UdonSpaceVehicles {
    public class DocumentGenerator  {

        [MenuItem("UdonSpaceVehicles/Generate Documents")]
        private static void GenerateDocument() {
            string ns = typeof(DocumentGenerator).Namespace;

            var lines = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && t.Namespace == ns && t.IsSubclassOf(typeof(UdonSharpBehaviour)))
                .GroupBy(t => t.Name)
                .Select(g => g.First())
                .Select(t => $"### {t.Name}\n{t.GetCustomAttribute<HelpMessageAttribute>()?.helpMessage}");

            var prev = File.ReadAllText("README.md");
            var next = new Regex("<\\!-- _USV_COMPONENTS_ -->[.\r\n]*?<\\!-- /_USV_COMPONENTS_ -->")
                .Replace(prev, $"<!-- _USV_COMPONENTS_ -->\n{string.Join("\n\n", lines)}\n<!-- /_USV_COMPONENTS_ -->");
            File.WriteAllText("README.md", next);
        }
    }
}
