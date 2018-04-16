// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System;
using GrandDevs.CZB.Common;
using GrandDevs.CZB;
using CCGKit;

public class CardView : MonoBehaviour
{
    public RuntimeCard card { get; private set; }

    [SerializeField]
    protected SpriteRenderer glowSprite;

    [SerializeField]
    protected SpriteRenderer pictureSprite;

	[SerializeField]
	protected SpriteRenderer backgroundSprite;

    [SerializeField]
    protected TextMeshPro costText;

    [SerializeField]
    protected TextMeshPro nameText;

    [SerializeField]
    protected TextMeshPro bodyText;

    [SerializeField]
    protected TextMeshPro amountText;

    protected GameObject previewCard;

    public GrandDevs.CZB.Data.Card libraryCard;

    public int manaCost { get; protected set; }

    [HideInInspector]
    public bool isPreview;

    protected virtual void Awake()
    {
        Assert.IsNotNull(glowSprite);
        Assert.IsNotNull(pictureSprite);
        Assert.IsNotNull(costText);
        Assert.IsNotNull(nameText);
        Assert.IsNotNull(bodyText);
    }

    public virtual void PopulateWithInfo(RuntimeCard card, string setName = "")
    {
        this.card = card;

        libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

        nameText.text = libraryCard.name;
        bodyText.text = libraryCard.description;
        costText.text = libraryCard.cost.ToString();

        manaCost = libraryCard.cost;

       var backgroundPicture = "Rarity_" + Enum.GetName(typeof(Enumerators.CardRarity), libraryCard.rarity);

        backgroundSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, backgroundPicture));
        pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, libraryCard.picture));

        amountText.transform.parent.gameObject.SetActive(false);
    }

    public virtual void PopulateWithLibraryInfo(GrandDevs.CZB.Data.Card card, string setName = "", int amount = 0)
    {
        libraryCard = card;
        nameText.text = card.name;
        bodyText.text = card.description;
        amountText.text = amount.ToString();
        costText.text = card.cost.ToString();

        manaCost = libraryCard.cost;

        var backgroundPicture = "Rarity_" + Enum.GetName(typeof(Enumerators.CardRarity), card.rarity);

        backgroundSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, backgroundPicture));
		pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, card.picture));
    }

    public virtual void UpdateAmount(int amount)
	{
		amountText.text = amount.ToString();
	}
    public virtual bool CanBePlayed(DemoHumanPlayer owner)
    {
        if (Constants.DEV_MODE)
            return true;
        return owner.isActivePlayer && owner.manaStat.effectiveValue >= manaCost;
    }

    public bool IsHighlighted()
    {
        return glowSprite.enabled;
    }

    public void SetHighlightingEnabled(bool enabled)
    {
        glowSprite.enabled = enabled;
    }
}
