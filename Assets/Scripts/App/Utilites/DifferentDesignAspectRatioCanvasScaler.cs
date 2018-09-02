using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    [RequireComponent(typeof(Canvas))]
    [ExecuteInEditMode]
    [AddComponentMenu("Layout/Different Design Aspect Ration Canvas Scaler", 101)]
    public class DifferentDesignAspectRatioCanvasScaler : CanvasScaler
    {
        public Vector2 ReferenceScreenResolution = new Vector2(1920, 1080);

        private Canvas _mCanvas2;

        protected override void HandleScaleWithScreenSize()
        {
            if (_mCanvas2 == null)
            {
                _mCanvas2 = GetComponent<Canvas>();
            }

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            int displayIndex = _mCanvas2.targetDisplay;
            if ((displayIndex > 0) && (displayIndex < Display.displays.Length))
            {
                Display disp = Display.displays[displayIndex];
                screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
            }

            float scaleFactor = 0;
            switch (m_ScreenMatchMode)
            {
                case ScreenMatchMode.MatchWidthOrHeight:
                {
                    scaleFactor = Mathf.Min(screenSize.x / ReferenceScreenResolution.x, screenSize.y / ReferenceScreenResolution.y);
                    scaleFactor *= ReferenceScreenResolution.y / m_ReferenceResolution.y;

                    break;
                }

                case ScreenMatchMode.Expand:
                {
                    scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }

                case ScreenMatchMode.Shrink:
                {
                    scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }
            }

            SetScaleFactor(scaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }
    }
}
