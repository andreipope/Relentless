// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;

using CCGKit;

using GrandDevs.CZB.Common;
using GrandDevs.CZB;

public class BoardCreature : MonoBehaviour
{
    public RuntimeCard card { get; private set; }

    [HideInInspector]
    public GameObject fightTargetingArrowPrefab;

    [SerializeField]
    protected SpriteRenderer glowSprite;

    [SerializeField]
    protected SpriteRenderer shadowSprite;

    [SerializeField]
    protected SpriteRenderer shieldGlowSprite;

    [SerializeField]
    protected SpriteRenderer shieldShadowSprite;

    [SerializeField]
    protected SpriteRenderer shieldSprite;

    [SerializeField]
    protected SpriteRenderer pictureSprite;

    [SerializeField]
    protected TextMeshPro nameText;

    [SerializeField]
    protected TextMeshPro attackText;

    [SerializeField]
    protected TextMeshPro healthText;

    [SerializeField]
    protected ParticleSystem sleepingParticles;

    [HideInInspector]
    public DemoHumanPlayer ownerPlayer;
    [HideInInspector]
    public TargetingArrow abilitiesTargetingArrow;
    [HideInInspector]
    public FightTargetingArrow fightTargetingArrow;

    public Stat attackStat { get; protected set; }
    public Stat healthStat { get; protected set; }

    [HideInInspector]
    public bool hasImpetus;
    [HideInInspector]
    public bool hasProvoke;

    [HideInInspector]
    public int numTurnsOnBoard;

    protected Action<int, int> onAttackStatChangedDelegate;
    protected Action<int, int> onHealthStatChangedDelegate;
	public event Action CreatureOnDieEvent;
	public event Action<object> CreatureOnAttackEvent;

    private int _stunTurns = 0;

    private Server _server;

    public bool IsPlayable
    {
        set{
            card.isPlayable = value;

            var netCard = GameClient.Get<IPlayerManager>().playerInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == card.instanceId);
            if(netCard == null)
                netCard = GameClient.Get<IPlayerManager>().opponentInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == card.instanceId);

			netCard.isPlayable = value;

		}
        get{
            return card.isPlayable;
        }
    }

    public bool IsStun
    {
        get{ return (_stunTurns > 0 ? true : false); }
    }

    protected virtual void Awake()
    {
        Assert.IsNotNull(glowSprite);
        Assert.IsNotNull(shadowSprite);
        Assert.IsNotNull(shieldGlowSprite);
        Assert.IsNotNull(shieldShadowSprite);
        Assert.IsNotNull(shieldSprite);
        Assert.IsNotNull(pictureSprite);
        Assert.IsNotNull(nameText);
        Assert.IsNotNull(attackText);
        Assert.IsNotNull(healthText);
        Assert.IsNotNull(sleepingParticles);
    }

    protected virtual void OnDestroy()
    {
        healthStat.onValueChanged -= onHealthStatChangedDelegate;
        attackStat.onValueChanged -= onAttackStatChangedDelegate;
        CreatureOnDieEvent?.Invoke();
    }

    public virtual void PopulateWithInfo(RuntimeCard card, string setName = "")
    {
        this.card = card;
          
        var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

        nameText.text = libraryCard.name;

        var backgroundPicture = "Rarity_" + Enum.GetName(typeof(Enumerators.CardRarity), libraryCard.rarity);

        pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Elements/{0}/{1}", setName, libraryCard.picture));

		attackStat = card.namedStats["DMG"];
		healthStat = card.namedStats["HP"];

        attackText.text = attackStat.effectiveValue.ToString();
        healthText.text = healthStat.effectiveValue.ToString();


        onAttackStatChangedDelegate = (oldValue, newValue) =>
        {
            UpdateStatText(attackText, attackStat);
        };
        attackStat.onValueChanged += onAttackStatChangedDelegate;

        onHealthStatChangedDelegate = (oldValue, newValue) =>
        {
			UpdateStatText(healthText, healthStat);
        };
        healthStat.onValueChanged += onHealthStatChangedDelegate;

        if(libraryCard.type == Enumerators.CardType.FERAL)
            hasImpetus = true;
        else if (libraryCard.type == Enumerators.CardType.HEAVY)
			hasProvoke = true;
          
        if (hasProvoke)
        {
            glowSprite.gameObject.SetActive(false);
            shadowSprite.gameObject.SetActive(false);
            shieldGlowSprite.gameObject.SetActive(true);
            shieldShadowSprite.gameObject.SetActive(true);
            shieldSprite.gameObject.SetActive(true);
        }
        SetHighlightingEnabled(false);
        if (hasImpetus)
        {
            StopSleepingParticles();
            if (ownerPlayer != null)
            {
                SetHighlightingEnabled(true);
                card.isPlayable = true;

			}
        }
    }

    public void OnStartTurn()
    {
        numTurnsOnBoard += 1;

		if (_stunTurns == 0)
        {
            StopSleepingParticles();
			IsPlayable = true;


			if (ownerPlayer != null)
                SetHighlightingEnabled(true);

		}
        else
            _stunTurns--;
    }

    public void OnEndTurn()
    {
        CancelTargetingArrows();
    }

	public void Stun(int turns)
	{
        if(turns > _stunTurns)
            _stunTurns = turns;
        IsPlayable = false;

		sleepingParticles.Play();

	}

    public void CancelTargetingArrows()
    {
        if (abilitiesTargetingArrow != null)
        {
            Destroy(abilitiesTargetingArrow.gameObject);
        }
        if (fightTargetingArrow != null)
        {
            Destroy(fightTargetingArrow.gameObject);
        }
    }

    private void UpdateStatText(TextMeshPro text, Stat stat)
    {
        text.text = stat.effectiveValue.ToString();
        if (stat.effectiveValue > stat.originalValue)
        {
            text.color = Color.green;
        }
        else if (stat.effectiveValue < stat.originalValue)
        {
            text.color = Color.red;
        }
        else
        {
            text.color = Color.black;
        }
        var sequence = DOTween.Sequence();
        sequence.Append(text.transform.DOScale(new Vector3(1.4f, 1.4f, 1.0f), 0.4f));
        sequence.Append(text.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f));
        sequence.Play();
    }

    public void SetHighlightingEnabled(bool enabled)
    {
        if (hasProvoke)
        {
            shieldGlowSprite.enabled = enabled;
            shieldShadowSprite.enabled = !enabled;
        }
        else
        {
            glowSprite.enabled = enabled;
            shadowSprite.enabled = !enabled;
        }
    }

    public void StopSleepingParticles()
    {
        sleepingParticles.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnCardSelected(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnCardUnselected(this);
            }
        }
    }

    private void OnMouseDown()
    {
        if (ownerPlayer != null && ownerPlayer.isActivePlayer && card.isPlayable)
        {
            fightTargetingArrow = Instantiate(fightTargetingArrowPrefab).GetComponent<FightTargetingArrow>();
            fightTargetingArrow.targetType = EffectTarget.OpponentOrOpponentCreature;
            fightTargetingArrow.opponentBoardZone = ownerPlayer.opponentBoardZone;
            fightTargetingArrow.Begin(transform.position);
            ownerPlayer.DestroyCardPreview();
            ownerPlayer.isCardSelected = true;
        }
    }

    private void OnMouseUp()
    {
        if (fightTargetingArrow != null)
        {
            fightTargetingArrow.End(this);
            ownerPlayer.isCardSelected = false;
        }
    }

    public void ResolveCombat()
    {
        var sortingGroup = GetComponent<SortingGroup>();
        if (fightTargetingArrow != null)
        {	
            if (fightTargetingArrow.selectedPlayer != null)
            {
                var targetPlayer = fightTargetingArrow.selectedPlayer;
                SetHighlightingEnabled(false);
                IsPlayable = false;

				sortingGroup.sortingOrder = 100;
                CombatAnimation.PlayFightAnimation(gameObject, targetPlayer.gameObject, 0.1f, () =>
                {
                    CreatureOnAttackEvent?.Invoke(targetPlayer);
                    ownerPlayer.FightPlayer(card);
                },
                () =>
                {
                    sortingGroup.sortingOrder = 0;
                    fightTargetingArrow = null;
                });
            }
            if (fightTargetingArrow.selectedCard != null)
            {
                var targetCard = fightTargetingArrow.selectedCard;
                SetHighlightingEnabled(false);
                IsPlayable = false;

				sortingGroup.sortingOrder = 100;
                if (targetCard != GetComponent<BoardCreature>() &&
                    targetCard.GetComponent<HandCard>() == null)
                {
                    CombatAnimation.PlayFightAnimation(gameObject, targetCard.gameObject, 0.5f, () =>
                    {
                        CreatureOnAttackEvent?.Invoke(targetCard);
                        ownerPlayer.FightCreature(card, targetCard.card);
                    },
                    () =>
                    {
                        sortingGroup.sortingOrder = 0;
                        fightTargetingArrow = null;
                    });
                }
            }
        }
    }
}
