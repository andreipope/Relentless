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

public class BoardSkill : MonoBehaviour
{

    [HideInInspector]
    public DemoHumanPlayer ownerPlayer;

    public GameObject fightTargetingArrowPrefab,
                            _fireDamageVFXprefab,
                            _healVFXprefab,
                           _fireDamageVFX,
                            _healVFX;

    [SerializeField]
    protected ParticleSystem sleepingParticles;

    [HideInInspector]
    public TargetingArrow abilitiesTargetingArrow;
    [HideInInspector]
    public FightTargetingArrow fightTargetingArrow;

    private int _manaCost;
    private int _skillPower;
    private Enumerators.SkillType _skillType;
    private ILoadObjectsManager _loadObjectsManager;

    private bool _used;

    private void Start()
    {
        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        _fireDamageVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
        _healVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        int deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
        int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].heroId;
        var skill = GameClient.Get<IDataManager>().CachedHeroesData.heroes[heroId].skill;

        _manaCost = skill.cost;
        _skillType = skill.skillType;
        _skillPower = skill.value;
    }

    public void OnEndTurn()
    {
        _used = false;
        CancelTargetingArrows();
    }

    public void CancelTargetingArrows()
    {
        if (fightTargetingArrow != null)
        {
            Destroy(fightTargetingArrow.gameObject);
        }
    }

   /* private void OnTriggerEnter2D(Collider2D collider)
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
    }              */

    private void OnMouseDown()
    {
        if (GameClient.Get<ITutorialManager>().IsTutorial && (GameClient.Get<ITutorialManager>().CurrentStep != 15))
			return;
        if (_used)
            return;
        if(_skillType != Enumerators.SkillType.HEAL)
        if (_manaCost <= ownerPlayer.manaStat.effectiveValue)
        {
            if (ownerPlayer != null && ownerPlayer.isActivePlayer/* && isPlayable*/)
            {
                fightTargetingArrow = Instantiate(fightTargetingArrowPrefab).GetComponent<FightTargetingArrow>();
                fightTargetingArrow.targetType = EffectTarget.AnyPlayerOrCreature;
                fightTargetingArrow.opponentBoardZone = ownerPlayer.opponentBoardZone;
                fightTargetingArrow.Begin(transform.position);
            }
        }
    }

    private void OnMouseUp()
    {
        if (_used)
            return;
        if (_manaCost <= ownerPlayer.manaStat.effectiveValue)
        {
            if (_skillType == Enumerators.SkillType.HEAL)
            {
                if (ownerPlayer != null && ownerPlayer.isActivePlayer/* && isPlayable*/)
                    DoSkillAction(null);
            }
            else
            {
                if (fightTargetingArrow != null)
                {
                    ResolveCombat();
                    ownerPlayer.isCardSelected = false;
                }
            }
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
                DoSkillAction(targetPlayer);
                
                GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
            }

            else if (fightTargetingArrow.selectedCard != null)
            {
                var targetCard = fightTargetingArrow.selectedCard;
                if (targetCard != GetComponent<BoardCreature>() &&
                    targetCard.GetComponent<HandCard>() == null)
                {
					DoSkillAction(targetCard);
                }
            }
            CancelTargetingArrows();
            fightTargetingArrow = null;
        }       
    }

    private void DoSkillAction(object target)
    {
        ownerPlayer.manaStat.baseValue -= _manaCost;
        switch (_skillType)
        {
            case Enumerators.SkillType.FREEZE:
                FreezeAction(target);
                break;
            case Enumerators.SkillType.TOXIC_DAMAGE:
				ToxicDamageAction(target);
                break;
            case Enumerators.SkillType.FIRE_DAMAGE:
				FireDamageAction(target);
                break;
            case Enumerators.SkillType.HEAL_ANY:
				HealAnyAction(target);
                break;
            case Enumerators.SkillType.CARD_RETURN:
				CardReturnAction(target);
                break;
            case Enumerators.SkillType.HEAL:
                HealAction();
                break;
            default:
                break;
        }
        _used = true;
    }

    private void FreezeAction(object target)
    {
        Debug.Log("FREEZE HIM");
        if (target is BoardCreature)
        {
            var creature = target as BoardCreature;


            for (int i = 0; i < ownerPlayer.opponentBoardCardsList.Count; i++)
            {
                if (ownerPlayer.opponentBoardCardsList[i] == creature)
                {
                    creature = ownerPlayer.opponentBoardCardsList[i];
                    break;
                }
            }

            creature.Stun(_skillPower);
            CreateFireAttackVFX(creature.transform.position);
        }
        //TODO for heroes
    }
	private void ToxicDamageAction(object target)
	{
		Debug.Log("POISON HIM");
        AttackWithModifiers(target, Enumerators.SetType.LIFE);
    }
	private void FireDamageAction(object target)
	{
		Debug.Log("BURN HIM");
        AttackWithModifiers(target, Enumerators.SetType.TOXIC);
	}
	private void HealAnyAction(object target)
	{
		Debug.Log("HEAL ANY");
        if (target is PlayerAvatar)
        {
            var player = target as PlayerAvatar;
            if (player.playerInfo.netId == ownerPlayer.netId)
                ownerPlayer.HealPlayerBySkill(_skillPower, false);
            else
                ownerPlayer.HealPlayerBySkill(_skillPower);
            //TODO ????? QuestionPopup about when we damage ourselves

            CreateFireAttackVFX(player.transform.position);
        }
        else
        {
            var cruature = target as BoardCreature;
            ownerPlayer.HealCreatureBySkill(_skillPower, cruature.card);
            CreateFireAttackVFX(cruature.transform.position);
        }
    }
	private void CardReturnAction(object target)
	{
		Debug.Log("RETURN CARD");
	}


	private void HealAction()
	{
		Debug.Log("HEAL HIM");
        CreateHealVFX(transform.position - Vector3.right * 2.3f);
        ownerPlayer.HealPlayerBySkill(_skillPower, false, false);
    }
	

    private void AttackWithModifiers(object target, Enumerators.SetType setType)
    {
        if (target is PlayerAvatar)
        {
            var player = target as PlayerAvatar;
            //TODO additional damage to heros
            if (player.playerInfo.netId == ownerPlayer.netId)
                ownerPlayer.FightPlayerBySkill(_skillPower, false);
            else
                ownerPlayer.FightPlayerBySkill(_skillPower);


            CreateFireAttackVFX(player.transform.position);
        }
        else
        {
            var cruature = target as BoardCreature;
            var attackModifier = 0;
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(cruature.card.cardId);
            if (libraryCard.cardSetType == setType)
                attackModifier = 1;
            ownerPlayer.FightCreatureBySkill(_skillPower + attackModifier, cruature.card);
            CreateFireAttackVFX(cruature.transform.position);
        }
    }

    private void CreateFireAttackVFX(Vector3 pos)
    {
        _fireDamageVFX = MonoBehaviour.Instantiate(_fireDamageVFXprefab);
        _fireDamageVFX.transform.position = pos + Vector3.forward;
    }
    private void CreateHealVFX(Vector3 pos)
    {
        _healVFX = MonoBehaviour.Instantiate(_healVFXprefab);
        _healVFX.transform.position = pos + Vector3.forward;
    }
}
