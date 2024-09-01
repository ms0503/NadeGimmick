using nadena.dev.modular_avatar.core;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NadeGimmick.Editor {
    internal sealed class AvatarSettings : EditorWindow {
        private void CreateGUI() {
            this.minSize = new Vector2(400, 220);
            var root = this.rootVisualElement;
            if(!Directory.Exists("Assets/NadeGimmick")) {
                Directory.CreateDirectory("Assets/NadeGimmick");
            }
            var title = new Label(L10n.Tr("Nade Gimmick Avatar Settings")) {
                style = {
                    fontSize = 24
                }
            };
            var avatar = new ObjectField(L10n.Tr("Avatar")) {
                objectType = typeof(GameObject),
                style = {
                    marginBottom = 20,
                    maxHeight = 40
                },
                tooltip = L10n.Tr(
                    "An avatar object to configure.\nPlease put what contains VRC Avatar Descriptor in hierarchy.")
            };
            var status = new Label("==========[Nade Gimmick]==========") {
                style = {
                    backgroundColor = new StyleColor(new Color32(0x55, 0x55, 0x55, 0xff)),
                    marginBottom = 25,
                    marginLeft = 5,
                    marginRight = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5,
                    paddingTop = 5
                }
            };
            var profile = new DropdownField(L10n.Tr("Profile")) {
                choices = (
                    from file in Directory.EnumerateFiles(Constants.PROFILE_PATH, "*.controller")
                    let basename = Path.GetFileNameWithoutExtension(file)
                    orderby basename
                    select basename
                ).ToList(),
                style = {
                    marginBottom = 20
                },
                tooltip = L10n.Tr("A profile of Nade Gimmick.")
            };
            avatar.RegisterValueChangedCallback(e => {
                status.text = "==========[Nade Gimmick]==========";
                var parent = (GameObject)e.newValue;
                if(parent is null) {
                    status.text = L10n.Tr("Avatar is not specified.\nPlease specify avatar object.");
                    return;
                }
                GameObject obj;
                if(parent.transform.Find("NadeGimmick") is { } nadeGimmick) {
                    obj = nadeGimmick.gameObject;
                } else {
                    status.text = L10n.Tr("Nade Gimmick is not installed in the avatar.\nPlease install it.");
                    return;
                }
                var name = obj.GetComponent<NadeGimmickProfile>().name;
                if(name == "") {
                    profile.value = "";
                    status.text =
                        L10n.Tr("This avatar is not set Nade Gimmick profile.\nPlease set in the following field.");
                    return;
                }
                var anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(Constants.PROFILE_PATH + "/" + name +
                    ".controller");
                obj.GetComponent<Animator>().runtimeAnimatorController = anim;
                obj.GetComponent<ModularAvatarMergeAnimator>().animator = anim;
                profile.value = name;
            });
            profile.RegisterValueChangedCallback(e => {
                var parent = (GameObject)avatar.value;
                if(parent is null) {
                    status.text = L10n.Tr("Avatar is not specified.\nPlease specify avatar object.");
                    return;
                }
                GameObject obj;
                if(parent.transform.Find("NadeGimmick") is { } nadeGimmick) {
                    obj = nadeGimmick.gameObject;
                } else {
                    status.text = L10n.Tr("Nade Gimmick is not installed in the avatar.\nPlease install it.");
                    return;
                }
                var anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(Constants.PROFILE_PATH + "/" + e.newValue +
                    ".controller");
                obj.GetComponent<Animator>().runtimeAnimatorController = anim;
                obj.GetComponent<ModularAvatarMergeAnimator>().animator = anim;
                obj.GetComponent<NadeGimmickProfile>().name = e.newValue;
            });
            root.Add(title);
            root.Add(avatar);
            root.Add(status);
            root.Add(profile);
        }
    }
}
