using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Localization;
using Loom.ZombieBattleground.Gameplay;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class PreparingForBattlePopup : IUIPopup
    {
        private const float PrepareForBattleSoundVolumeKoef = 0.25f;

        private const int NumberOfBattleFlavorText = 12;

        private const string FallbackBattleFlavorText = "Feeding the creatures";

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        private ISoundManager _soundManager;
        private TextMeshProUGUI _flavorText;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            GameClient.Get<ICameraManager>().FadeOut(null, 1, true);

            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            _soundManager.StopPlaying(Enumerators.SoundType.PREPARING_FOR_BATTLE);
            _soundManager.StopPlaying(Enumerators.SoundType.PREPARING_FOR_BATTLE_LOOP);
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PreparingForBattlePopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);
            _flavorText = Self.transform.Find("Flavor_Text").GetComponent<TextMeshProUGUI>();

            ShowRandomFlavorText();

            _soundManager.PlaySound(Enumerators.SoundType.PREPARING_FOR_BATTLE, Constants.SfxSoundVolume * PrepareForBattleSoundVolumeKoef, false, false, true);
            _soundManager.PlaySound(Enumerators.SoundType.PREPARING_FOR_BATTLE_LOOP, Constants.SfxSoundVolume * PrepareForBattleSoundVolumeKoef, false, false, true);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void ShowRandomFlavorText()
        {
            int randomVal = Random.Range(0, NumberOfBattleFlavorText);
            if (Enum.TryParse($"GameData_BattleFlavor_{randomVal}", out LocalizationTerm term))
            {
                _flavorText.text = LocalizationUtil.GetLocalizedString(term);
            }
            else
            {
                _flavorText.text = FallbackBattleFlavorText;
            }
        }
    }
}
