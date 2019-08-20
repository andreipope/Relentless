using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using log4net;
using I2.Loc;
using TMPro;
using Loom.ZombieBattleground.Common;

using Language = Loom.ZombieBattleground.Common.Enumerators.Language;

namespace Loom.ZombieBattleground.Localization
{   
    public class LocalizationFontSettings : MonoBehaviour
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LocalizationFontSettings));
        
        private readonly Dictionary<Language, float> CharacterSpacingMap = new Dictionary<Language, float>
        {
            {
                Language.NONE, 0f
            },
            {
                Language.EN, 0f
            },
            {
                Language.ZH_CN, -3f
            },
            {
                Language.KO, 0f
            },
            {
                Language.JA, 0f
            },
            {
                Language.ES, 0f
            },
            {
                Language.TH, 0f
            }
        };
        
        private readonly Dictionary<Language, FontStyles> FontStylesMap = new Dictionary<Language, FontStyles>
        {
            {
                Language.NONE, FontStyles.Normal
            },
            {
                Language.EN, FontStyles.Normal
            },
            {
                Language.ZH_CN, FontStyles.Bold
            },
            {
                Language.KO, FontStyles.Bold
            },
            {
                Language.JA, FontStyles.Normal
            },
            {
                Language.ES, FontStyles.Normal
            },
            {
                Language.TH, FontStyles.Normal
            }
        };
        
        private Localize _localize;

        private TextMeshProUGUI _text;

        private TextMeshPro _tmpText;
        
        void Awake()
        {
            _localize = this.gameObject.GetComponent<Localize>();
            _text = this.gameObject.GetComponent<TextMeshProUGUI>();
            _tmpText = this.gameObject.GetComponent<TextMeshPro>();

            _localize?.LocalizeEvent.AddListener(OnApplyFontSettings);

            OnApplyFontSettings();
        }
        
        private void OnApplyFontSettings()
        {
            if (_text != null)
            {
                try
                {
                    _text.fontStyle = FontStylesMap
                    [
                        GameClient.Get<ILocalizationManager>().CurrentLanguage
                    ];
                    _text.characterSpacing = CharacterSpacingMap
                    [
                        GameClient.Get<ILocalizationManager>().CurrentLanguage
                    ];
                }
                catch
                {
                    Log.Info($"Error applying font settings with current language");
                }
            }
            else if(_tmpText != null)
            {
                try
                {
                    _tmpText.fontStyle = FontStylesMap
                    [
                        GameClient.Get<ILocalizationManager>().CurrentLanguage
                    ];
                    _tmpText.characterSpacing = CharacterSpacingMap
                    [
                        GameClient.Get<ILocalizationManager>().CurrentLanguage
                    ];
                }
                catch
                {
                    Log.Info($"Error applying font settings with current language");
                }
            }
        }
    }
}