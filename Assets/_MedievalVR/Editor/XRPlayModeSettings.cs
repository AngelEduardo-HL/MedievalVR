using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using Object = UnityEngine.Object;

namespace MedievalVR.Editor
{
    [InitializeOnLoad]
    public static class XRPlayModeSettings
    {
        public enum PlayMode { Headset, Keyboard }

        const string MenuRoot = "MedievalVR/Pruebas/";
        const string SimulatorGuid = "58d0a4ac86f2348deb02f3880c71378e";
        const string OpenXRPath = "Assets/Unity/XR/Settings/OpenXRPackageSettings.asset";
        static string PreferenceKey => "MedievalVR.XRPlayMode." + Application.dataPath;
        public static PlayMode Mode => (PlayMode)EditorPrefs.GetInt(PreferenceKey, 0);

        static XRPlayModeSettings()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem(MenuRoot + "Visor VR")]
        static void Headset() => SetMode(PlayMode.Headset);

        [MenuItem(MenuRoot + "Teclado y ratón")]
        static void Keyboard() => SetMode(PlayMode.Keyboard);

        [MenuItem(MenuRoot + "Visor VR", true)]
        static bool ValidateHeadset() => ValidateMenu("Visor VR", PlayMode.Headset);

        [MenuItem(MenuRoot + "Teclado y ratón", true)]
        static bool ValidateKeyboard() => ValidateMenu("Teclado y ratón", PlayMode.Keyboard);

        static bool ValidateMenu(string name, PlayMode mode)
        {
            Menu.SetChecked(MenuRoot + name, Mode == mode);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        public static void SetMode(PlayMode mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorPrefs.SetInt(PreferenceKey, (int)mode);
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                EditorApplication.update -= SelectInput;
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            if (File.Exists(OpenXRPath) && File.ReadAllText(OpenXRPath).Contains("<<<<<<<"))
            {
                Debug.LogError("OpenXR tiene un conflicto de Git pendiente.");
                EditorApplication.ExitPlaymode();
                return;
            }

            EditorApplication.update += SelectInput;
        }

        public static bool HasRealHeadset()
        {
            if (XRSettings.isDeviceActive) return true;
            foreach (var device in InputSystem.devices)
                if (device is XRHMD && !(device is XRSimulatedHMD) && device.enabled) return true;
            return false;
        }

        static void SelectInput()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= SelectInput;
                return;
            }

            bool headset = HasRealHeadset();
            EditorApplication.update -= SelectInput;
            bool useSimulator = Mode == PlayMode.Keyboard;
            var existing = Object.FindFirstObjectByType<XRInteractionSimulator>();
            if (!useSimulator)
            {
                if (existing != null) Object.Destroy(existing.gameObject);
                if (!headset) Debug.LogWarning("Conecta Quest Link antes de Play.");
                return;
            }

            if (existing != null) return;
            string path = AssetDatabase.GUIDToAssetPath(SimulatorGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("Falta importar XR Interaction Simulator.");
                return;
            }

            GameObject simulator = Object.Instantiate(prefab);
            simulator.name = "XR Interaction Simulator";
            Object.DontDestroyOnLoad(simulator);
        }
    }
}
