using CellMenu;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
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
                if (GuiManager.MainMenuLayer == null) return;
                if (GuiManager.MainMenuLayer.PageLoadout == null) return;
                if (GuiManager.MainMenuLayer.PageLoadout.m_dropButton == null) return;
                var button = GuiManager.MainMenuLayer.PageLoadout.m_dropButton;
                var texts = button.GetComponentsInChildren<TextMeshPro>();
                if (texts.Length == 0) return;
                foreach (var text in texts) if (text.gameObject.name == "PressAndHold") text.SetText("<size=16>game may hang momentarily (lgtuner)", true);
            }
        }
    }
}
