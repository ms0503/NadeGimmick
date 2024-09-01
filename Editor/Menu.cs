using UnityEditor;
using UnityEngine;

namespace NadeGimmick.Editor {
    internal static class Menu {
        [MenuItem(Constants.MENU_AVATAR_SETTINGS)]
        private static void AvatarSettingsMenu() {
            var window = EditorWindow.GetWindow<AvatarSettings>("UIElements");
            window.titleContent = new GUIContent(L10n.Tr("Nade Gimmick Avatar Settings"));
            window.Show();
        }

        [MenuItem(Constants.MENU_PROFILES)]
        private static void ProfilesMenu() {
            var window = EditorWindow.GetWindow<Profiles>("UIElements");
            window.titleContent = new GUIContent(L10n.Tr("Nade Gimmick Profiles"));
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
