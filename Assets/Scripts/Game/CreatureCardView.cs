// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using TMPro;
using CCGKit;
using GrandDevs.CZB.Common;

public class CreatureCardView : CardView
{
    [SerializeField]
    protected SpriteRenderer attackSprite;

    [SerializeField]
    protected TextMeshPro attackText;

    [SerializeField]
    protected SpriteRenderer defenseSprite;

	[SerializeField]
	protected SpriteRenderer typeSprite;

    [SerializeField]
    protected TextMeshPro defenseText;

    public Stat attackStat { get; protected set; }
    public Stat defenseStat { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(attackSprite);
        Assert.IsNotNull(attackText);
        Assert.IsNotNull(defenseSprite);
        Assert.IsNotNull(defenseText);
    }

    public override void PopulateWithInfo(RuntimeCard card, string setName)
    {
        base.PopulateWithInfo(card, setName);

        attackStat = new Stat();
        attackStat.statId = 0;
        attackStat.name = "DMG";
        attackStat.originalValue = libraryCard.damage;
        attackStat.baseValue = libraryCard.damage;
        attackStat.minValue = 0;
        attackStat.maxValue = 99;

        defenseStat = new Stat();
        defenseStat.statId = 1;
        defenseStat.name = "HP";
        defenseStat.originalValue = libraryCard.health;
        defenseStat.baseValue = libraryCard.health;
        defenseStat.minValue = 0;
        defenseStat.maxValue = 99;

        attackText.text = attackStat.effectiveValue.ToString();
        defenseText.text = defenseStat.effectiveValue.ToString();

        typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", (Enumerators.CardType)card.type + "_icon"));

		attackStat.onValueChanged += (oldValue, newValue) => { attackText.text = attackStat.effectiveValue.ToString(); };
        defenseStat.onValueChanged += (oldValue, newValue) => { defenseText.text = defenseStat.effectiveValue.ToString(); };
    }

    public override void PopulateWithLibraryInfo(GrandDevs.CZB.Data.Card card, string setName = "", int amount = 0)
    {
        base.PopulateWithLibraryInfo(card, setName, amount);

        attackText.text = card.damage.ToString();
        defenseText.text = card.health.ToString();

		typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", card.cardType + "_icon"));
    }
}