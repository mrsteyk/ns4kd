using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ns4kd;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    internal static Harmony Harmony { get; } = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private static ConfigEntry<int> MSAA;
    private static ConfigEntry<int> SMAA;
    private static ConfigEntry<bool> NoVSync;
    private static ConfigEntry<int> Aniso;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        MSAA = Config.Bind("Quality", "MSAA", -1, "Override MSAA levels. -1 - Don't override.");
        SMAA = Config.Bind("Quality", "SMAA", -1, "Override SMAA quality. -1 - Don't override, 0 - disable, 1 - low, 2 - medium, 3 - high");
        NoVSync = Config.Bind("Quality", "NoVSync", false);
        Aniso = Config.Bind("Quality", "ForceAniso", -1, "Force this anisoLevel");

        // TODO

        Harmony.PatchAll(typeof(Plugin));
    }

    [HarmonyPatch(typeof(Game.LauncherArgs), nameof(Game.LauncherArgs.OnRuntimeMethodLoad))]
    [HarmonyPostfix]
    public static void SetResolution()
    {
        //Log.LogInfo($"Current pipeline is {GraphicsSettings.renderPipelineAsset}");
        var urpAsset = new UniversalRenderPipelineAsset(GraphicsSettings.renderPipelineAsset.Pointer);
        if (MSAA.Value >= 0)
        {
            Log.LogInfo($"Unity had AA {UnityEngine.QualitySettings.antiAliasing}, URP had MSAA {urpAsset.msaaSampleCount}, override to {MSAA.Value}");
            UnityEngine.QualitySettings.antiAliasing = MSAA.Value;
            urpAsset.msaaSampleCount = MSAA.Value;
        }

        UnityEngine.QualitySettings.anisotropicFiltering = UnityEngine.AnisotropicFiltering.ForceEnable;
        UnityEngine.QualitySettings.shadowResolution = UnityEngine.ShadowResolution.VeryHigh;
        UnityEngine.QualitySettings.skinWeights = UnityEngine.SkinWeights.Unlimited;
        UnityEngine.QualitySettings.softParticles = true;

        if (NoVSync.Value)
            UnityEngine.QualitySettings.vSyncCount = 0;

        Log.LogInfo($"URP asset had softShadowQuality {urpAsset.softShadowQuality}, mainLightShadowmapResolution {urpAsset.mainLightShadowmapResolution}, additionalLightsShadowmapResolution {urpAsset.additionalLightsShadowmapResolution}");

        urpAsset.softShadowQuality = SoftShadowQuality.High;
        urpAsset.mainLightShadowmapResolution = 8192;
        urpAsset.additionalLightsShadowmapResolution = 4096;
    }

    [HarmonyPatch(typeof(Game.CameraController), nameof(Game.CameraController.OnEnable))]
    [HarmonyPostfix]
    public static void CameraQualityFix(Game.CameraController __instance)
    {
        if (SMAA.Value >= 0)
        {
            var URPD = __instance._camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            var old_aa = URPD.antialiasing;
            var old_aaq = URPD.antialiasingQuality;
            URPD.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            switch (SMAA.Value)
            {
                case 0:
                    URPD.antialiasing = AntialiasingMode.None;
                    break;
                case 1:
                    URPD.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.Low;
                    break;
                case 2:
                    URPD.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.Medium;
                    break;
                default:
                    URPD.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.High;
                    break;
            }
            Log.LogInfo($"Camera had AA set to {old_aa} {old_aaq}, override with {URPD.antialiasing} {URPD.antialiasingQuality}");
        }
    }

    // TODO(mrsteyk): is this even called?
    [HarmonyPatch(typeof(UnityEngine.Texture), "get_anisoLevel")]
    [HarmonyPrefix]
    public static void Texture2Dctor(UnityEngine.Texture __instance)
    {
        if (Aniso.Value >= 0)
            __instance.anisoLevel = Aniso.Value;
    }
}
