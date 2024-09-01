using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace NadeGimmick.Editor {
    internal sealed class Profiles : EditorWindow {
        [SerializeField]
        private AnimationClip defaultNadeAnim;

        [SerializeField]
        private AnimatorController originalAnimator;

        private AnimatorController _anim;

        private void CreateGUI() {
            var root = this.rootVisualElement;
            if(!Directory.Exists(Constants.PROFILE_PATH)) {
                Directory.CreateDirectory(Constants.PROFILE_PATH);
            }
            var profiles = (
                from file in Directory.EnumerateFiles(Constants.PROFILE_PATH, "*.controller")
                let basename = Path.GetFileNameWithoutExtension(file)
                orderby basename
                select basename
            ).ToList();
            var profile = new DropdownField(L10n.Tr("Profile")) {
                choices = profiles,
                style = {
                    marginBottom = 20
                },
                tooltip = L10n.Tr("A profile of Nade Gimmick.")
            };
            var name = new TextField(L10n.Tr("Profile name")) {
                multiline = false,
                tooltip = L10n.Tr("A profile name to create.")
            };
            var create = new Button {
                style = {
                    marginBottom = 20
                },
                text = L10n.Tr("Create profile"),
                tooltip = L10n.Tr("Create new Nade Gimmick profile.")
            };
            var nade = new ObjectField(L10n.Tr("Facial expression animation")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar is brushed gently.")
            };
            var reset = new Button {
                style = {
                    maxWidth = 100,
                    paddingRight = 10
                },
                text = L10n.Tr("Reset"),
                tooltip = L10n.Tr("Set facial expression animation to dummy.")
            };
            profile.RegisterValueChangedCallback(e => {
                this._anim =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(Constants.PROFILE_PATH + "/" + e.newValue +
                        ".controller");
                var nadeLayer = (from layer in this._anim.layers where layer.name == "Nade" select layer).First();
                var nadeState = (from state in nadeLayer.stateMachine.states
                    where state.state.name == "Nade"
                    select state.state).First();
                nade.value = nadeState.motion;
            });
            create.RegisterCallback<ClickEvent>(_ => {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(this.originalAnimator),
                    Constants.PROFILE_PATH + "/" + name.value + ".controller");
                profile.value = name.value;
                name.value = "";
            });
            nade.RegisterValueChangedCallback(e => {
                var nadeLayer = (from layer in this._anim.layers where layer.name == "Nade" select layer).First();
                var nadeState = (from state in nadeLayer.stateMachine.states
                    where state.state.name == "Nade"
                    select state.state).First();
                nadeState.motion = (AnimationClip)e.newValue;
            });
            reset.RegisterCallback<ClickEvent>(_ => {
                var nadeLayer = (from layer in this._anim.layers where layer.name == "Nade" select layer).First();
                var nadeState = (from state in nadeLayer.stateMachine.states
                    where state.state.name == "Nade"
                    select state.state).First();
                nadeState.motion = this.defaultNadeAnim;
                nade.value = this.defaultNadeAnim;
            });
            if(0 < profiles.Count) {
                this._anim =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(Constants.PROFILE_PATH + "/" + profiles.First() +
                        ".controller");
                var nadeLayer = (from layer in this._anim.layers where layer.name == "Nade" select layer).First();
                var nadeState = (from state in nadeLayer.stateMachine.states
                    where state.state.name == "Nade"
                    select state.state).First();
                nade.value = nadeState.motion;
            }
            root.Add(profile);
            root.Add(name);
            root.Add(create);
            root.Add(nade);
            root.Add(reset);
        }
    }
}
