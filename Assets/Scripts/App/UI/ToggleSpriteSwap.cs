using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class ToggleSpriteSwap : MonoBehaviour
    {
        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _selectedSprite;

        [SerializeField]
        private Graphic _targetGraphic;

        private Toggle _targetToggle;

        private void Start()
        {
            _targetToggle = GetComponent<Toggle>();
            _targetToggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
            OnTargetToggleValueChanged(_targetToggle.isOn);
        }

        private void OnTargetToggleValueChanged(bool newValue) {
            Image targetImage = _targetGraphic as Image;
            if (targetImage == null)
                return;

            targetImage.overrideSprite = newValue ? _selectedSprite : _normalSprite;
        }
    }
}
