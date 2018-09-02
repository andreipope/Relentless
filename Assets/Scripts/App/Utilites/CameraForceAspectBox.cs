using UnityEngine;

namespace LoomNetwork.CZB
{
    public class CameraForceAspectBox : MonoBehaviour
    {
        [SerializeField]
        private float _targetAspectRatio = 16f / 9f;

        private Camera _camera;

        public float TargetAspectRatio
        {
            get => _targetAspectRatio;
            set => _targetAspectRatio = value;
        }

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = GetComponentInParent<Camera>();
            }

            Camera.onPreRender += OnCameraPreRender;
        }

        private void OnDisable()
        {
            _camera.rect = new Rect(0, 0, 1, 1);
            Camera.onPreRender -= OnCameraPreRender;
        }

        private void OnCameraPreRender(Camera camera)
        {
            if (camera != _camera)

                return;

            UpdateBox();
        }

        private void UpdateBox()
        {
            // determine the game window's current aspect ratio
            float windowAspect = Screen.width / (float)Screen.height;

            // current viewport height should be scaled by this amount
            float scaleHeight = windowAspect / _targetAspectRatio;

            // if scaled height is less than current height, add letterbox
            if (scaleHeight < 1.0f)
            {
                Rect rect = _camera.rect;

                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;

                _camera.rect = rect;
            }
            else
            {
                // add pillarbox
                float scaleWidth = 1.0f / scaleHeight;

                Rect rect = _camera.rect;

                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;

                _camera.rect = rect;
            }
        }
    }
}
