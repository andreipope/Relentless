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
        if(_skillType == Enumerators.SkillType.FIREBALL)
        if (_manaCost <= ownerPlayer.manaStat.effectiveValue)
        {
            if (ownerPlayer != null && ownerPlayer.isActivePlayer/* && isPlayable*/)
            {
                fightTargetingArrow = Instantiate(fightTargetingArrowPrefab).GetComponent<FightTargetingArrow>();
                fightTargetingArrow.targetType = EffectTarget.OpponentOrOpponentCreature;
                fightTargetingArrow.opponentBoardZone = ownerPlayer.opponentBoardZone;
                fightTargetingArrow.Begin(transform.position);
            }
        }
    }

    private void OnMouseUp()
    {
        if (_manaCost <= ownerPlayer.manaStat.effectiveValue)
        {
            
            if (_skillType == Enumerators.SkillType.FIREBALL)
            {
                if (fightTargetingArrow != null)
                {
                    ResolveCombat();
                    ownerPlayer.isCardSelected = false;
                }
            }
            else
            {
                if (ownerPlayer != null && ownerPlayer.isActivePlayer/* && isPlayable*/)
                {
                    ownerPlayer.manaStat.baseValue -= _manaCost;
					//var lifeBuff = new Modifier(_skillPower);
                    //ownerPlayer.lifeStat.modifiers.Add(lifeBuff);
                    CreateHealVFX(transform.position - Vector3.right*2.3f);

                    ownerPlayer.HealPlayerBySkill(2);
                    _used = true;
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
                Debug.Log(_manaCost);
                ownerPlayer.manaStat.baseValue -= _manaCost;
                var targetPlayer = fightTargetingArrow.selectedPlayer;
                ownerPlayer.FightPlayerBySkill(_skillPower);
                CreateFireAttackVFX(targetPlayer.transform.position);
                GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
                _used = true;
            }

            else if (fightTargetingArrow.selectedCard != null)
            {
                var targetCard = fightTargetingArrow.selectedCard;
                if (targetCard != GetComponent<BoardCreature>() &&
                    targetCard.GetComponent<HandCard>() == null)
                {
                    ownerPlayer.manaStat.baseValue -= _manaCost;
                    ownerPlayer.FightCreatureBySkill(2, targetCard.card);
                    CreateFireAttackVFX(targetCard.transform.position);
                    _used = true;
                }
            }
            CancelTargetingArrows();
            fightTargetingArrow = null;
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
