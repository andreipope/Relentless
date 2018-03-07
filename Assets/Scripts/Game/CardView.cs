// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using TMPro;

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

    public Card libraryCard;

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

        var gameConfig = GameManager.Instance.config;
        /*var cardType = gameConfig.cardTypes.Find(x => x.id == card.cardId);
        Assert.IsNotNull(cardType);
        pictureSprite.sprite = Resources.Load<Sprite>(cardType.GetStringProperty("Picture"));*/

        var libraryCard = gameConfig.GetCard(card.cardId);
        Assert.IsNotNull(libraryCard);
        nameText.text = libraryCard.name;
        bodyText.text = libraryCard.GetStringProperty("Text");

        var cost = libraryCard.costs.Find(x => x is PayResourceCost);
        if (cost != null)
        {
            var payResourceCost = cost as PayResourceCost;
            manaCost = payResourceCost.value;
            costText.text = manaCost.ToString();
        }

        if (setName.Length > 0)
        {
            backgroundSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, libraryCard.GetStringProperty("Rarity")));
            pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, libraryCard.GetStringProperty("Picture")));
        }

        var material = libraryCard.GetStringProperty("Material");
        if (!string.IsNullOrEmpty(material))
        {
            pictureSprite.material = Resources.Load<Material>(string.Format("Materials/{0}", material));
        }
        amountText.transform.parent.gameObject.SetActive(false);
    }

    public virtual void PopulateWithLibraryInfo(Card card, string setName = "")
    {
        libraryCard = card;
        nameText.text = card.name;
        bodyText.text = card.GetStringProperty("Text");
        amountText.text = card.GetIntProperty("Amount").ToString();

        var cost = card.costs.Find(x => x is PayResourceCost);
        if (cost != null)
        {
            var payResourceCost = cost as PayResourceCost;
            manaCost = payResourceCost.value;
            costText.text = manaCost.ToString();
        }

		backgroundSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, card.GetStringProperty("Rarity")));
		pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, card.GetStringProperty("Picture")));
        var material = card.GetStringProperty("Material");
        if (!string.IsNullOrEmpty(material))
        {
            pictureSprite.material = Resources.Load<Material>(string.Format("Materials/{0}", material));
        }
    }


	public virtual void UpdateAmount(Card card)
	{
		libraryCard = card;
		amountText.text = card.GetIntProperty("Amount").ToString();
	}
    public virtual bool CanBePlayed(DemoHumanPlayer owner)
    {
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
