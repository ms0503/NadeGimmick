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
                tooltip = L10n.Tr("A profile of Nade Face Gimmick.")
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
                tooltip = L10n.Tr("Create new Nade Face Gimmick profile.")
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
            var crotchAnim = new ObjectField(L10n.Tr("Facial expression animation (Crotch)")) {
                objectType = typeof(AnimationClip),
                style = {
                    maxHeight = 40,
                    marginBottom = 20
                },
                tooltip = L10n.Tr("An animation of facial expression that is used when avatar's crotch is touched.")
            };
            var reset = new Button {
                style = {
                    maxWidth = 100,
                    paddingRight = 10
                },
                text = L10n.Tr("Reset"),
                tooltip = L10n.Tr("Set facial expression animation to dummy.")
            };
            var convertV4Container = new VisualElement();
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
            var convertV4 = new Button {
                style = {
                    marginBottom = 20
                },
                text = L10n.Tr("Convert profile"),
                tooltip = L10n.Tr("Convert to new style profile.")
            };
            var convertV50Container = new VisualElement();
            var convertV50 = new Button {
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
                    Mode.ConvertV4 => root.IndexOf(convertV4Container),
                    Mode.ConvertV50 => root.IndexOf(convertV50Container),
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
                var isV4Animator = (
                    from _layer in this._anim.layers
                    where _layer.name == "Nade"
                    select true
                ).Contains(true);
                if(isV4Animator) {
                    mode = Mode.ConvertV4;
                    root.Insert(index, convertV4Container);
                    return;
                }
                var isV50Animator = !(
                    from _layer in this._anim.layers
                    where _layer.name == "Crotch"
                    select true
                ).Contains(true);
                if(isV50Animator) {
                    mode = Mode.ConvertV50;
                    root.Insert(index, convertV50Container);
                    return;
                }
                mode = Mode.Edit;
                root.Insert(index, editContainer);
                foreach(
                    var (layerName, anim) in new[] {
                        Tuple.Create("Face", faceAnim),
                        Tuple.Create("LeftEar", leftEarAnim),
                        Tuple.Create("RightEar", rightEarAnim),
                        Tuple.Create("Chest", chestAnim),
                        Tuple.Create("Crotch", crotchAnim)
                    }
                ) {
                    var state = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        from _state in _layer.stateMachine.states
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
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "Face"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            leftEarAnim.RegisterValueChangedCallback(e => {
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "LeftEar"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            rightEarAnim.RegisterValueChangedCallback(e => {
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "RightEar"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            chestAnim.RegisterValueChangedCallback(e => {
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "Chest"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = (AnimationClip)e.newValue;
            });
            crotchAnim.RegisterValueChangedCallback(e => {
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "Crotch"
                    from _state in _layer.stateMachine.states
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
                        Tuple.Create("Chest", chestAnim),
                        Tuple.Create("Crotch", crotchAnim)
                    }
                ) {
                    var state = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        from _state in _layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First();
                    state.motion = this.defaultNadeAnim;
                    anim.value = this.defaultNadeAnim;
                }
            });
            convertV4.RegisterCallback<ClickEvent>(_ => {
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this._anim),
                    $"{Constants.PROFILE_PATH}/{profile.value}.backup.controller"
                );
                var anim = (
                    from _layer in this._anim.layers
                    where _layer.name == "Nade"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Nade"
                    select _state.state.motion
                ).First();
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this.originalAnimator),
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                this._anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                var state = (
                    from _layer in this._anim.layers
                    where _layer.name == "Face"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                state.motion = anim;
                faceAnim.value = anim;
                var index = root.IndexOf(convertV4Container);
                root.RemoveAt(index);
                mode = Mode.Edit;
                root.Insert(index, editContainer);
            });
            convertV50.RegisterCallback<ClickEvent>(_ => {
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this._anim),
                    $"{Constants.PROFILE_PATH}/{profile.value}.backup.controller"
                );
                var anims = (
                    from t in new[] {
                        Tuple.Create("Face", faceAnim),
                        Tuple.Create("LeftEar", leftEarAnim),
                        Tuple.Create("RightEar", rightEarAnim),
                        Tuple.Create("Chest", chestAnim)
                    }
                    let state = (
                        from _layer in this._anim.layers
                        where _layer.name == t.Item1
                        from _state in _layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First()
                    select state.motion
                ).ToArray();
                AssetDatabase.CopyAsset(
                    AssetDatabase.GetAssetPath(this.originalAnimator),
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                this._anim = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    $"{Constants.PROFILE_PATH}/{profile.value}.controller"
                );
                foreach(
                    var (i, layerName, anim) in new[] {
                        Tuple.Create(0, "Face", faceAnim),
                        Tuple.Create(1, "LeftEar", leftEarAnim),
                        Tuple.Create(2, "RightEar", rightEarAnim),
                        Tuple.Create(3, "Chest", chestAnim)
                    }
                ) {
                    var state = (
                        from _layer in this._anim.layers
                        where _layer.name == layerName
                        from _state in _layer.stateMachine.states
                        where _state.state.name == "Touched"
                        select _state.state
                    ).First();
                    state.motion = anims[i];
                    anim.value = anims[i];
                }
                var crotchState = (
                    from _layer in this._anim.layers
                    where _layer.name == "Crotch"
                    from _state in _layer.stateMachine.states
                    where _state.state.name == "Touched"
                    select _state.state
                ).First();
                crotchState.motion = this.defaultNadeAnim;
                crotchAnim.value = this.defaultNadeAnim;
                var index = root.IndexOf(convertV50Container);
                root.RemoveAt(index);
                mode = Mode.Edit;
                root.Insert(index, editContainer);
            });
            createContainer.Add(profileName);
            createContainer.Add(create);
            editContainer.Add(faceAnim);
            editContainer.Add(leftEarAnim);
            editContainer.Add(rightEarAnim);
            editContainer.Add(chestAnim);
            editContainer.Add(crotchAnim);
            editContainer.Add(reset);
            convertV4Container.Add(convertNotice);
            convertV4Container.Add(convertV4);
            convertV50Container.Add(convertNotice);
            convertV50Container.Add(convertV50);
            root.Add(profile);
            root.Add(createContainer);
            if(1 < profiles.Count) {
                profile.value = profiles[1];
            }
        }

        private enum Mode {
            Create,
            Edit,
            ConvertV4,
            ConvertV50
        }
    }
}
