using UnityEditor;
using UnityEngine;

namespace NadeGimmick.Editor {
    internal static class Menu {
        [MenuItem(Constants.MENU_SETTINGS)]
        private static void SettingsMenu() {
            var window = EditorWindow.GetWindow<Settings>("UIElements");
            window.titleContent = new GUIContent(L10n.Tr("Nade Gimmick Settings"));
            window.Show();
        }

        [MenuItem(Constants.MENU_VERSION)]
        private static void VersionMenu() {
        }

        [MenuItem(Constants.MENU_VERSION, true)]
        private static bool VersionMenuValidation() {
            return false;
        }
    }
}
