using System;
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
            var mode = Mode.Create;
            if(!Directory.Exists(Constants.PROFILE_PATH)) {
                Directory.CreateDirectory(Constants.PROFILE_PATH);
            }
            var profiles = (
                from file in Directory.EnumerateFiles(Constants.PROFILE_PATH, "*.controller")
                let basename = Path.GetFileNameWithoutExtension(file)
                where !basename.EndsWith(".backup")
                orderby basename
                select basename
            ).Prepend(L10n.Tr("(Create new profile)")).ToList();
            var profile = new DropdownField(L10n.Tr("Profile")) {
                choices = profiles,
                index = 0,
                style = {
                    marginBottom = 20
                },
                tooltip = L10n.Tr("A profile of Nade Gimmick.")
            };
            var createContainer = new VisualElement();
            var profileName = new TextField(L10n.Tr("Profile name")) {
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
            var editContainer = new VisualElement();
            var faceAnim = new ObjectField(L10n.Tr("Facial expression animation (Face)")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar's face is patted.")
            };
            var leftEarAnim = new ObjectField(L10n.Tr("Facial expression animation (Left ear)")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar's left ear is touched.")
            };
            var rightEarAnim = new ObjectField(L10n.Tr("Facial expression animation (Right ear)")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar's right ear is touched.")
            };
            var chestAnim = new ObjectField(L10n.Tr("Facial expression animation (Chest)")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar's chest is touched.")
            };
            var reset = new Button {
                style = {
                    maxWidth = 100,
                    paddingRight = 10
                },
                text = L10n.Tr("Reset"),
                tooltip = L10n.Tr("Set facial expression animation to dummy.")
            };
            var convertContainer = new VisualElement();
            var convertNotice = new Label(L10n.Tr("This profile must be converted to new style.")) {
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
            var convert = new Button {
                style = {
                    marginBottom = 20
                },
                text = L10n.Tr("Convert profile"),
                tooltip = L10n.Tr("Convert to new style profile.")
            };
            profile.RegisterValueChangedCallback(e => {
                var index = mode switch {
                    Mode.Create => root.IndexOf(createContainer),
                    Mode.Edit => root.IndexOf(editContainer),
                    Mode.Convert => root.IndexOf(convertContainer),
                    _ => throw new NotImplementedException()
                };
                root.RemoveAt(index);
                if(e.newValue == L10n.Tr("(Create new profile)")) {
                    mode = Mode.Create;
                    root.Insert(index, createContainer);
                    return;
                }
                this._anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    $"{Constants.PROFILE_PATH}/{e.newValue}.controller"
                );
                var hasOldLayer = (
                    from _layer in this._anim.layers
                    where _layer.name == "Nade"
                    select true
                ).Contains(true);
                if(hasOldLayer) {
                    mode = Mode.Convert;
                    root.Insert(index, convertContainer);
                    return;
                }
                mode = Mode.Edit;
                root.Insert(index, editContainer);
                foreach(
                    var (layerName, anim) in new[] {
                        Tuple.Create("Face", faceAnim),
                        Tuple.Create("LeftEar", leftEarAnim),
                        Tuple.Create("RightEar", rightEarAnim),
                        Tuple.Create("Chest", chestAnim)
                    }
                ) {
                    var layer = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        select _layer
                    ).First();
                    var state = (
                        from _state in layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First();
                    anim.value = state.motion;
                }
            });
            create.RegisterCallback<ClickEvent>(_ => {
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this.originalAnimator),
                    $"{Constants.PROFILE_PATH}/{profileName.value}.controller"
                );
                profile.value = profileName.value;
                profileName.value = "";
            });
            faceAnim.RegisterValueChangedCallback(e => {
                var layer = (
                    from _layer in this._anim.layers
                    where _layer.name == "Face"
                    select _layer
                ).First();
                var state = (
                    from _state in layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            leftEarAnim.RegisterValueChangedCallback(e => {
                var layer = (
                    from _layer in this._anim.layers
                    where _layer.name == "LeftEar"
                    select _layer
                ).First();
                var state = (
                    from _state in layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            rightEarAnim.RegisterValueChangedCallback(e => {
                var layer = (
                    from _layer in this._anim.layers
                    where _layer.name == "RightEar"
                    select _layer
                ).First();
                var state = (
                    from _state in layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            chestAnim.RegisterValueChangedCallback(e => {
                var layer = (
                    from _layer in this._anim.layers
                    where _layer.name == "Chest"
                    select _layer
                ).First();
                var state = (
                    from _state in layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            reset.RegisterCallback<ClickEvent>(_ => {
                foreach(
                    var (layerName, anim) in new[] {
                        Tuple.Create("Face", faceAnim),
                        Tuple.Create("LeftEar", leftEarAnim),
                        Tuple.Create("RightEar", rightEarAnim),
                        Tuple.Create("Chest", chestAnim)
                    }
                ) {
                    var layer = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        select _layer
                    ).First();
                    var state = (
                        from _state in layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First();
                    state.motion = this.defaultNadeAnim;
                    anim.value = this.defaultNadeAnim;
                }
            });
            convert.RegisterCallback<ClickEvent>(_ => {
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this._anim),
                    $"{Constants.PROFILE_PATH}/{profile.value}.backup.controller"
                );
                var nadeLayer = (
                    from _layer in this._anim.layers
                    where _layer.name == "Nade"
                    select _layer
                ).First();
                var nadeState = (
                    from _state in nadeLayer.stateMachine.states
                    where _state.state.name == "Nade"
                    select _state.state
                ).First();
                var anim = nadeState.motion;
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this.originalAnimator),
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                this._anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                var layer = (
                    from _layer in this._anim.layers
                    where _layer.name == "Face"
                    select _layer
                ).First();
                var state = (
                    from _state in layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = anim;
                var index = root.IndexOf(convertContainer);
                root.RemoveAt(index);
                mode = Mode.Edit;
                root.Insert(index, editContainer);
            });
            if(1 < profiles.Count) {
                this._anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    $"{Constants.PROFILE_PATH}/{profiles[1]}.controller"
                );
                foreach(
                    var (layerName, anim) in new[] {
                        Tuple.Create("Face", faceAnim),
                        Tuple.Create("LeftEar", leftEarAnim),
                        Tuple.Create("RightEar", rightEarAnim),
                        Tuple.Create("Chest", chestAnim)
                    }
                ) {
                    var layer = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        select _layer
                    ).First();
                    var state = (
                        from _state in layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First();
                    anim.value = state.motion;
                }
            }
            createContainer.Add(profileName);
            createContainer.Add(create);
            editContainer.Add(faceAnim);
            editContainer.Add(leftEarAnim);
            editContainer.Add(rightEarAnim);
            editContainer.Add(chestAnim);
            editContainer.Add(reset);
            convertContainer.Add(convertNotice);
            convertContainer.Add(convert);
            root.Add(profile);
            root.Add(createContainer);
        }

        private enum Mode {
            Create,
            Edit,
            Convert
        }
    }
}
