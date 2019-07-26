using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground.Editor {
    [Serializable]
    public class PreviewRenderUtility : IDisposable {
        [SerializeField]
        private Camera _camera;
        private RenderTexture _renderTexture;

        public Camera Camera => _camera;

        public RenderTexture RenderTexture => _renderTexture;

        public void StartStaticPreview(int width, int height) {
            if (_camera == null) {
                GameObject cameraGameObject = new GameObject("_PreviewCamera");
                //cameraGameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                _camera = cameraGameObject.AddComponent<Camera>();
                _camera.aspect = width / (float) height;
                _camera.enabled = false;
            }

            if (_renderTexture != null)
            {
                RenderTexture.ReleaseTemporary(_renderTexture);
            }

            _renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _camera.targetTexture = _renderTexture;
        }

        public Texture2D EndStaticPreview() {
            if (_renderTexture == null)
                throw new InvalidOperationException("Call StartStaticPreview first");

            RenderTexture temporary = null;
            Texture2D texture2D;
            try {
                temporary = RenderTexture.GetTemporary(_renderTexture.width, _renderTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                RenderTexture.active = temporary;
                GL.Clear(true, true, Camera.backgroundColor);
                Graphics.Blit(_renderTexture, temporary);
                texture2D = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false, true);
                texture2D.ReadPixels(new Rect(0f, 0f, _renderTexture.width, _renderTexture.height), 0, 0);
                texture2D.Apply();
            } finally {
                RenderTexture.ReleaseTemporary(temporary);
                RenderTexture.ReleaseTemporary(_renderTexture);
                _renderTexture = null;
            }

            return texture2D;
        }

        public void Cleanup() {
            if (_camera != null) {
                GameObject cameraGameObject = _camera.gameObject;
                Object.DestroyImmediate(_camera);
                Object.DestroyImmediate(cameraGameObject);
            }

            Dispose();
        }

        public void Dispose() {
            if (_renderTexture != null)
            {
                RenderTexture.ReleaseTemporary(_renderTexture);
            }
        }
    }
}
