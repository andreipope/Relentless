using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

using Language = Loom.ZombieBattleground.Localization.LocalizationUtil.Language;

namespace Loom.ZombieBattleground.Localization
{   
    public class LocalizationFontSettings : MonoBehaviour
    {
        private readonly Dictionary<Language, float> CharacterSpacingDictionary = new Dictionary<Language, float>
        {
            {
                Language.None, 0f
            },
            {
                Language.English, -6.7f
            },
            {
                Language.Chinese, -3f
            },
            {
                Language.Korean, 0f
            },
            {
                Language.Japanese, 0f
            },
            {
                Language.Spanish, 0f
            },
            {
                Language.Thai, 0f
            }
        };
        
        private readonly Dictionary<Language, FontStyles> FontStylesDictionary = new Dictionary<Language, FontStyles>
        {
            {
                Language.None, FontStyles.Normal
            },
            {
                Language.English, FontStyles.Normal
            },
            {
                Language.Chinese, FontStyles.Bold
            },
            {
                Language.Korean, FontStyles.Normal
            },
            {
                Language.Japanese, FontStyles.Normal
            },
            {
                Language.Spanish, FontStyles.Normal
            },
            {
                Language.Thai, FontStyles.Normal
            }
        };
        
        private Localize _localize;

        private TextMeshProUGUI _text;
        
        void Awake()
        {
            _localize = this.gameObject.GetComponent<Localize>();
            _text = this.gameObject.GetComponent<TextMeshProUGUI>();

            _localize?.LocalizeEvent.AddListener(OnApplyFontSettings);

            OnApplyFontSettings();
        }
        
        private void OnApplyFontSettings()
        {
            if (_text == null)
                return;
                
            _text.fontStyle = FontStylesDictionary[LocalizationUtil.CurrentLanguage];
            _text.characterSpacing = CharacterSpacingDictionary[LocalizationUtil.CurrentLanguage];
        }
    }
}