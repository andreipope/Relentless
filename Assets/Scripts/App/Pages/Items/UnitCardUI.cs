
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Localization;
#if UNITY_EDITOR
using System.Collections.Generic;
using ZombieBattleground.Editor.Runtime;
#endif

public class UnitCardUI
{
    private static readonly ILog Log = Logging.GetLog(nameof(UnitCardUI));

    private GameObject _selfObj;
    private GameObject _cardAmountTray;

    private Image _frameImage;
    private Image _unitImage;
    private Image _rankImage;
    private Image _setImage;
    private Image _cardCountTrayImage;

    private TextMeshProUGUI _gooText;
    private TextMeshProUGUI _attackText;
    private TextMeshProUGUI _defenseText;
    private TextMeshProUGUI _bodyText;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _cardCountText;

    private ILoadObjectsManager _loadObjectsManager;

    private Card _card;

    private Material _grayScaleMaterial;

    public void Init(GameObject obj)
    {
        _selfObj = obj;
        _frameImage = _selfObj.transform.Find("Frame").GetComponent<Image>();
        _unitImage = _selfObj.transform.Find("Viewport/Picture").GetComponent<Image>();
        _rankImage = _selfObj.transform.Find("RankIcon").GetComponent<Image>();
        _setImage = _selfObj.transform.Find("SetIcon").GetComponent<Image>();
        _cardCountTrayImage = _selfObj.transform.Find("AmountWithCounterTray/Tray").GetComponent<Image>();

        _gooText = _selfObj.transform.Find("GooText").GetComponent<TextMeshProUGUI>();
        _attackText = _selfObj.transform.Find("AttackText").GetComponent<TextMeshProUGUI>();
        _defenseText = _selfObj.transform.Find("DefenseText").GetComponent<TextMeshProUGUI>();
        _bodyText = _selfObj.transform.Find("BodyText").GetComponent<TextMeshProUGUI>();
        _titleText = _selfObj.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        _cardCountText = _selfObj.transform.Find("AmountWithCounterTray/Text").GetComponent<TextMeshProUGUI>();

        _cardAmountTray = _selfObj.transform.Find("AmountWithCounterTray").gameObject;

        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

        _grayScaleMaterial = _loadObjectsManager.GetObjectByPath<Material>("Materials/UI-Default-Grayscale");

#if UNITY_EDITOR
        MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
    }

    public void FillCardData(Card card, int cardCount = 0)
    {
        _card = card;
        _titleText.text = Enum.TryParse($"GameData_Cards_Name_{card.CardKey.MouldId.Id.ToString()}", out LocalizationTerm nameTerm) ?
            LocalizationUtil.GetLocalizedString(nameTerm, card.Name) :
            card.Name;
        _bodyText.text = Enum.TryParse($"GameData_Cards_Description_{card.CardKey.MouldId.Id.ToString()}", out LocalizationTerm descriptionTerm) ?
            LocalizationUtil.GetLocalizedString(descriptionTerm, card.Description) :
            card.Description;
        _gooText.text = card.Cost.ToString();

        _attackText.text = card.Damage != 0 ? card.Damage.ToString() : string.Empty;
        _defenseText.text = card.Defense != 0 ? card.Defense.ToString() : string.Empty;
        _cardCountText.text = cardCount.ToString();

        string frameName = $"Images/Cards/Frames/frame_{card.Faction}";
        _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);

        string rarity = Enum.GetName(typeof(Enumerators.CardRank), card.Rank);
        string rankName = $"Images/IconsRanks/rank_icon_{rarity.ToLowerInvariant()}";
        _rankImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(rankName);

        string imagePath = $"{Constants.PathToCardsIllustrations}{card.Picture.ToLowerInvariant()}";
        _unitImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);

        _cardAmountTray.SetActive(cardCount != 0);

        string setName = $"Images/IconsSets/set_icon_{card.CardKey.Variant.ToString().ToLowerInvariant()}";
        _setImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(setName);
    }

    public void UpdateCardAmount(int cardCount)
    {
        _cardCountText.text = cardCount.ToString();
        _cardAmountTray.SetActive(cardCount != 0);
    }

    public Card GetCard()
    {
        return _card;
    }

    public GameObject GetGameObject()
    {
        return _selfObj != null ? _selfObj : null;
    }

    public IReadOnlyCard GetCardInterface()
    {
        return _card;
    }

    public bool IsActive()
    {
        if (_selfObj == null)
            return false;

        return _selfObj.activeSelf;
    }

    public void SetActive(bool active)
    {
        if (_selfObj == null)
            return;

        _selfObj.SetActive(active);
    }



    public RectTransform GetFrameRectTransform()
    {
        return _frameImage.GetComponent<RectTransform>();
    }

    public void EnableRenderer(bool enable)
    {
        _frameImage.enabled = enable;
        _unitImage.enabled = enable;
        _rankImage.enabled = enable;
        _cardCountTrayImage.enabled = enable;

        _gooText.enabled = enable;
        _attackText.enabled = enable;
        _defenseText.enabled = enable;
        _bodyText.enabled = enable;
        _titleText.enabled = enable;
        _cardCountText.enabled = enable;
    }

    public void EnableObject(bool enable)
    {
        _selfObj.SetActive(enable);
    }

    public void GrayScaleCard(bool selected)
    {
        Material material = selected ? null : _grayScaleMaterial;

        _frameImage.material = material;
        _unitImage.material = material;
        _rankImage.material = material;
        _setImage.material = material;
    }

    public void EnableSetImage(bool enable)
    {
        _setImage.enabled = enable;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_selfObj == null)
        {
            MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
            return;
        }

        if (_card == null || !_selfObj.activeInHierarchy)
            return;

        List<string> lines = new List<string>();
        lines.Add("Mould Id: " + _card.CardKey.MouldId.Id);
        if (_card.CardKey.Variant != Enumerators.CardVariant.Standard)
        {
            lines.Add("Variant: " + _card.CardKey.Variant);
        }
        lines.Add("Name: " + _card.Name);

        DebugCardInfoDrawer.DrawCustom(
            _selfObj.transform.position,
            lines,
            false);
    }
#endif
}
