using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace NadeGimmick.Editor {
    internal sealed class AvatarProcessor : IVRCSDKPreprocessAvatarCallback {
        public int callbackOrder { get; } = -2000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject) {
            var editorOnlyComponents = avatarGameObject.GetComponentsInChildren<EditorOnlyComponent>();
            foreach(var component in editorOnlyComponents) {
                Object.DestroyImmediate(component);
            }
            return true;
        }
    }
}
