using nadena.dev.modular_avatar.core;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace NadeGimmick.Editor {
    internal sealed class Menu : EditorWindow {
        [SerializeField]
        private AnimationClip defaultNadeAnim;

        [SerializeField]
        private GameObject nadeGimmickPrefab;

        [SerializeField]
        private AnimatorController originalAnimator;

        private Animator _anim;
        private bool _isLoaded;

        private void CreateGUI() {
            this.minSize = new Vector2(400, 220);
            this._isLoaded = false;
            var root = this.rootVisualElement;
            root.Add(new Label(L10n.Tr("Nade Gimmick Settings")) {
                name = "title",
                style = {
                    fontSize = 24
                }
            });
            root.Add(new ObjectField(L10n.Tr("Avatar to install")) {
                name = "avatar",
                objectType = typeof(GameObject),
                style = {
                    marginBottom = 20,
                    maxHeight = 40
                },
                tooltip = L10n.Tr(
                    "An avatar to install this.\nPlease put what contains VRC Avatar Descriptor in hierarchy.")
            });
            root.Add(new Label(L10n.Tr("Install this.\nIf the avatar is already installed, load from it.")) {
                name = "install-label",
                style = {
                    backgroundColor = new StyleColor(new Color32(0x55, 0x55, 0x55, 0xff)),
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5,
                    paddingTop = 5
                }
            });
            root.Add(new Button(() => {
                var parent = (GameObject)root.Q<ObjectField>("avatar").value;
                if(parent is null) {
                    root.Q<Label>("install-label").text =
                        L10n.Tr("Avatar is not specified.\nPlease specify avatar object.");
                    return;
                }
                GameObject obj;
                if(parent.transform.Find("NadeGimmick") is { } nadeGimmick) {
                    obj = nadeGimmick.gameObject;
                    root.Q<Label>("install-label").text = L10n.Tr("Loaded.");
                } else {
                    obj = (GameObject)PrefabUtility.InstantiatePrefab(this.nadeGimmickPrefab);
                    obj.transform.SetParent(parent.transform);
                    var savePath =
                        EditorUtility.SaveFilePanelInProject(L10n.Tr("Animator Controller"), "NadeGimmick",
                            "controller", "");
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(this.originalAnimator),
                        savePath);
                    obj.GetComponent<Animator>().runtimeAnimatorController =
                        AssetDatabase.LoadAssetAtPath<AnimatorController>(savePath);
                    obj.GetComponent<ModularAvatarMergeAnimator>().animator =
                        AssetDatabase.LoadAssetAtPath<AnimatorController>(savePath);
                    root.Q<Label>("install-label").text = L10n.Tr("Installed.");
                }
                this._anim = obj.GetComponent<Animator>();
                if(this._isLoaded) {
                    return;
                }
                root.Add(new ObjectField(L10n.Tr("Facial expression animation")) {
                    name = "nade",
                    objectType = typeof(AnimationClip),
                    style = {
                        maxHeight = 40,
                        marginBottom = 20
                    },
                    tooltip = L10n.Tr("An animation of facial expression that is used when avatar is brushed gently.")
                });
                var buttons = new Box {
                    name = "buttons",
                    style = {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexEnd,
                        paddingRight = 10
                    }
                };
                buttons.Add(new Button(() => {
                    var nadeLayer = (from layer in ((AnimatorController)this._anim.runtimeAnimatorController).layers
                            where layer.name == "Nade"
                            select layer)
                        .First();
                    var nadeState = (from state in nadeLayer.stateMachine.states
                        where state.state.name == "Nade"
                        select state.state).First();
                    nadeState.motion = this.defaultNadeAnim;
                    root.Q<ObjectField>("nade").value = this.defaultNadeAnim;
                }) {
                    name = "reset",
                    style = {
                        maxWidth = 100
                    },
                    text = L10n.Tr("Reset"),
                    tooltip = L10n.Tr("Set facial expression animation to dummy.")
                });
                buttons.Add(new Button(() => {
                    var nadeLayer = (from layer in ((AnimatorController)this._anim.runtimeAnimatorController).layers
                            where layer.name == "Nade"
                            select layer)
                        .First();
                    var nadeState = (from state in nadeLayer.stateMachine.states
                        where state.state.name == "Nade"
                        select state.state).First();
                    nadeState.motion = (AnimationClip)root.Q<ObjectField>("nade").value;
                }) {
                    name = "save",
                    style = {
                        maxWidth = 100
                    },
                    text = L10n.Tr("Save"),
                    tooltip = L10n.Tr("Save to animator.")
                });
                root.Add(buttons);
                var nadeLayer = (from layer in ((AnimatorController)this._anim.runtimeAnimatorController).layers
                        where layer.name == "Nade"
                        select layer)
                    .First();
                var nadeState = (from state in nadeLayer.stateMachine.states
                    where state.state.name == "Nade"
                    select state.state).First();
                root.Q<ObjectField>("nade").value = nadeState.motion;
                this._isLoaded = true;
            }) {
                name = "install",
                style = {
                    marginBottom = 20
                },
                text = L10n.Tr("Install / Load"),
                tooltip = L10n.Tr("Install this to the avatar or load this from it.")
            });
        }

        [MenuItem("Tools/NadeGimmick")]
        private static void OpenMenu() {
            var window = GetWindow<Menu>("UIElements");
            window.titleContent = new GUIContent(L10n.Tr("Nade Gimmick Settings"));
            window.Show();
        }
    }
}
