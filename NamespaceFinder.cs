// Test file to find correct namespace
using System;
using System.Reflection;
using BepInEx;

namespace KeyOverlay
{
    public static class NamespaceFinder
    {
        public static void FindTypes()
        {
            try
            {
                var asm = Assembly.Load("Assembly-CSharp");
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name.Contains("Configurable") || type.Name.Contains("OptionInterface"))
                    {
                        KeyOverlayPlugin.Log?.LogInfo($"Found: {type.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                KeyOverlayPlugin.Log?.LogError($"Error: {ex.Message}");
            }
        }
    }
}