// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using TMPro;

using CCGKit;

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
        attackStat = card.namedStats["DMG"];
        defenseStat = card.namedStats["HP"];
        attackText.text = attackStat.effectiveValue.ToString();
        defenseText.text = defenseStat.effectiveValue.ToString();

        attackStat.onValueChanged += (oldValue, newValue) => { attackText.text = attackStat.effectiveValue.ToString(); };
        defenseStat.onValueChanged += (oldValue, newValue) => { defenseText.text = defenseStat.effectiveValue.ToString(); };
    }

    public override void PopulateWithLibraryInfo(Card card, string setName)
    {
        base.PopulateWithLibraryInfo(card, setName);
        attackText.text = card.stats[0].effectiveValue.ToString();
        defenseText.text = card.stats[1].effectiveValue.ToString();
		typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", card.GetStringProperty("Type") + "_icon"));
	}
}