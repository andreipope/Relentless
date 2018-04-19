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
                            _airPickUpCardVFXprefab,
                            _frozenVFXprefab,
                            _toxicVFXprefab,
                            _fireDamageVFX,
                            _healVFX,
                            _airPickUpCardVFX,
                            _frozenVFX,
                            _toxicVFX;

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
        _airPickUpCardVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
        _toxicVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/ToxicAttackVFX");
        _frozenVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
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
                    if (_skillType == Enumerators.SkillType.CARD_RETURN)
                        fightTargetingArrow.targetType = EffectTarget.TargetCard;

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
        if (!Constants.DEV_MODE)
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
            CreateFrozenVFX(creature.transform.position);
        }
        //TODO for heroes
    }
	private void ToxicDamageAction(object target)
	{
		Debug.Log("POISON HIM");
        AttackWithModifiers(target,Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
    }
	private void FireDamageAction(object target)
	{
		Debug.Log("BURN HIM");
        AttackWithModifiers(target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
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

            CreateHealVFX(player.transform.position);
        }
        else
        {
            var cruature = target as BoardCreature;
            ownerPlayer.HealCreatureBySkill(_skillPower, cruature.card);
            CreateHealVFX(cruature.transform.position);
        }
    }
	private void CardReturnAction(object target)
	{
        var cruature = target as BoardCreature;

        Debug.Log("RETURN CARD");

        //if (cardCaller.playerInfo.netId == cardCaller.netId)

        PlayerInfo playerInfo = ownerPlayer.playerInfo;
        if (ownerPlayer == null)
            playerInfo = ownerPlayer.opponentInfo;

        //Get server access
        Server _server = null;
        if (_server == null)
        {
            var server = GameObject.Find("Server");
            if (server != null)
            {
                _server = server.GetComponent<Server>();
            }
        }

        //create RuntimeCard
        var card = CreateRuntimeCard(playerInfo, cruature);

        //Add RuntimeCard to hand on server
        _server.gameState.currentPlayer.namedZones[Constants.ZONE_HAND].cards.Add(card);

        //Create Visual process of creating new card at the hand (simulation turn back)
        var netCard = CreateNetCard(card);

        //Put netCard to hand
        ownerPlayer.CreateAndPutToHandRuntimeCard(netCard, playerInfo);

        //MAYBE use that on future
        /*playerInfo.namedZones[Constants.ZONE_HAND].AddCard(card);
        cardCaller.EffectSolver.SetDestroyConditions(card);
        cardCaller.EffectSolver.SetTriggers(card);*/

        //Remove RuntimeCard on server
        var boardRuntimeCard = _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == cruature.card.instanceId);
        _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);
        //Remove RuntimeCard from hand
        playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(cruature.card);

        GameObject.Destroy(cruature.gameObject);
        CreateAirVFX(cruature.transform.position);
    }

    private RuntimeCard CreateRuntimeCard(PlayerInfo playerInfo, BoardCreature cruature)
    {
        var card = new RuntimeCard();
        card.cardId = cruature.card.cardId;
        card.instanceId = playerInfo.currentCardInstanceId++;
        card.ownerPlayer = playerInfo;
        card.stats[0] = cruature.card.stats[0];
        card.stats[1] = cruature.card.stats[1];
        card.namedStats["DMG"] = cruature.card.namedStats["DMG"];
        card.namedStats["HP"] = cruature.card.namedStats["HP"];

        card.namedStats["DMG"].baseValue = card.namedStats["DMG"].originalValue;
        card.namedStats["HP"].baseValue = card.namedStats["HP"].originalValue;
        return card;
    }

    private NetCard CreateNetCard(RuntimeCard card)
    {
        var netCard = new NetCard();
        netCard.cardId = card.cardId;
        netCard.instanceId = card.instanceId;
        netCard.stats = new NetStat[card.stats.Count];
        var idx = 0;
        foreach (var entry in card.stats)
        {
            netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);
        }
        netCard.keywords = new NetKeyword[card.keywords.Count];
        idx = 0;
        foreach (var entry in card.keywords)
        {
            netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);
        }
        netCard.connectedAbilities = card.connectedAbilities.ToArray();
        return netCard;
    }

    private void HealAction()
	{
		Debug.Log("HEAL HIM");
        CreateHealVFX(transform.position - Vector3.right * 2.3f);
        ownerPlayer.HealPlayerBySkill(_skillPower, false, false);
    }
	

    private void AttackWithModifiers(object target, Enumerators.SetType attackType, Enumerators.SetType setType)
    {
        if (target is PlayerAvatar)
        {
            var player = target as PlayerAvatar;
            //TODO additional damage to heros
            if (player.playerInfo.netId == ownerPlayer.netId)
                ownerPlayer.FightPlayerBySkill(_skillPower, false);
            else
                ownerPlayer.FightPlayerBySkill(_skillPower);


            CreateAttackVFXByType(attackType, player.transform.position);
        }
        else
        {
            var cruature = target as BoardCreature;
            var attackModifier = 0;
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(cruature.card.cardId);
            if (libraryCard.cardSetType == setType)
                attackModifier = 1;
            ownerPlayer.FightCreatureBySkill(_skillPower + attackModifier, cruature.card);
            CreateAttackVFXByType(attackType, cruature.transform.position);
        }
    }

    private void CreateAttackVFXByType(Enumerators.SetType type, Vector3 pos)
    {
        switch (type)
        {
            case Enumerators.SetType.FIRE:
                CreateFireAttackVFX(pos);
                break;
            case Enumerators.SetType.TOXIC:
                CreateToxicAttackVFX(pos);
                break;
            default:
                break;
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

    private void CreateAirVFX(Vector3 pos)
    {
        _airPickUpCardVFX = MonoBehaviour.Instantiate(_airPickUpCardVFXprefab);
        _airPickUpCardVFX.transform.position = pos + Vector3.forward;
    }

    private void CreateToxicAttackVFX(Vector3 pos)
    {
        _toxicVFX = MonoBehaviour.Instantiate(_toxicVFXprefab);
        _toxicVFX.transform.position = pos + Vector3.forward;
    }

    private void CreateFrozenVFX(Vector3 pos)
    {
        _frozenVFX = MonoBehaviour.Instantiate(_frozenVFXprefab);
        _frozenVFX.transform.position = pos + Vector3.forward;
    }
}
