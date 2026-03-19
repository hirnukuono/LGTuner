using CellMenu;
using HarmonyLib;
using TMPro;

namespace LGTuner.Inject
{
    internal class Inject_dropButton
    {
        [HarmonyPatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.SetPageActive))]
        internal static class buttonpatch
        {
            private static void Prefix(CM_PageLoadout __instance)
            {
                var button = GuiManager.MainMenuLayer.PageLoadout.m_dropButton;
                var texts = button.GetComponentsInChildren<TextMeshPro>();
                foreach (var text in texts) if (text.gameObject.name == "PressAndHold") text.SetText("<size=16>game may hang momentarily (lgtuner)", true);
            }
        }
    }
}
