
using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class CustomDeckUI
    {
        private TextMeshProUGUI _deckNameText;
        private TextMeshProUGUI _cardsCountText;

        private Button _buttonRename;
        private Button _buttonViewDeck;
        private Button _buttonSave;

        private Image _overlordImage;
        private Image _overlordPrimarySkillImage;
        private Image _overlordSecondarySkillImage;

        private GameObject _deckCardPrefab;
        private GameObject _indicatorPrefab;

        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _deckCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/DeckCard_UI");
            _indicatorPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/Indicator");
        }

        public void Load(GameObject obj)
        {
            GameObject selfObject = obj;

            _deckNameText = selfObject.transform.Find("Top_Panel/Panel_Image/Deck_Name").GetComponent<TextMeshProUGUI>();
            _cardsCountText = selfObject.transform.Find("Bottom_Panel/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            _buttonRename = selfObject.transform.Find("Top_Panel/Panel_Image/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);

            _buttonViewDeck = selfObject.transform.Find("Top_Panel/Panel_Image/Button_ViewDeck").GetComponent<Button>();
            _buttonViewDeck.onClick.AddListener(ButtonViewDeckHandler);

            _buttonSave = selfObject.transform.Find("Bottom_Panel/Button_SaveDeck").GetComponent<Button>();
            _buttonSave.onClick.AddListener(ButtonSaveHandler);

            _overlordImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Image/Image").GetComponent<Image>();
            _overlordPrimarySkillImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Skill_Primary/Image").GetComponent<Image>();
            _overlordSecondarySkillImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Skill_Secondary/Image").GetComponent<Image>();
        }

        public void ShowDeck(int deckId)
        {
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(deck => deck.Id.Id == deckId);
            if (selectedDeck == null)
                return;

            _deckNameText.text = selectedDeck.Name;

            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(selectedDeck.OverlordId);
            _overlordImage.sprite = GetOverlordThumbnailSprite(overlord.Prototype.Faction);
        }


        private void ButtonSaveHandler()
        {

        }

        private void ButtonViewDeckHandler()
        {

        }

        private void ButtonRenameHandler()
        {

        }

        private Sprite GetOverlordThumbnailSprite(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait";
            switch(overlordFaction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air");
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire");
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth");
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic");
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water");
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life");
                default:
                    return null;
            }
        }
    }
}



