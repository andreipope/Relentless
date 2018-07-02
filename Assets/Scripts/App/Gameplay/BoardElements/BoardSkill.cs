using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;

using GrandDevs.CZB.Common;
using GrandDevs.CZB;
using GrandDevs.CZB.Data;
using GrandDevs.Internal;
using System.Collections.Generic;

public class BoardSkill : MonoBehaviour
{
    private ILoadObjectsManager _loadObjectsManager;
    private IGameplayManager _gameplayManager;
    private ITutorialManager _tutorialManager;

    private PlayerController _playerController;
    private BattleController _battleController;
    private BattlegroundController _battlegroundController;


    [HideInInspector]
    public Player ownerPlayer;

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

    public int manaCost;
    private int _skillPower;
    public Enumerators.SetType skillType;




    private bool _isUsed;
    private bool _isOpponent = false;

    private SpriteRenderer _glow;

    public int SkillPower { get { return _skillPower; } }


    private void Awake()
    {
        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        _gameplayManager = GameClient.Get<IGameplayManager>();
        _tutorialManager = GameClient.Get<ITutorialManager>();

        _playerController = _gameplayManager.GetController<PlayerController>();
        _battleController = _gameplayManager.GetController<BattleController>();
        _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

        _fireDamageVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FIreBall_Impact");
        _healVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/HealVFX");
        _airPickUpCardVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
        _toxicVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Toxic_Impact");
        _frozenVFXprefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

        _glow = transform.Find("Glow").GetComponent<SpriteRenderer>();
        _glow.gameObject.SetActive(false);
    }

    public void SetSkill(Hero hero = null)
    {
        manaCost = hero.skill.cost;
        _skillPower = hero.skill.value;
        skillType = hero.heroElement;
    }

    public void OnStartTurn()
    {
        if (!_gameplayManager.WhoseTurn.Equals(ownerPlayer))
            return;

        if (ownerPlayer.Mana >= manaCost)
            SetHighlightingEnabled(true);
    }

    public void OnEndTurn()
    {
        if (!_gameplayManager.WhoseTurn.Equals(ownerPlayer))
            return;

        _isUsed = false;
        CancelTargetingArrows();
        SetHighlightingEnabled(false);
    }

    public void SetHighlightingEnabled(bool isActive)
    {
        _glow.gameObject.SetActive(isActive);
    }

    public void CancelTargetingArrows()
    {
        if (fightTargetingArrow != null)
        {
            Destroy(fightTargetingArrow.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnCardSelected(null);
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
                targetingArrow.OnCardUnselected(null);
            }
        }
    }

    private void OnMouseDown()
    {
        if (!ownerPlayer.IsLocalPlayer || !_gameplayManager.WhoseTurn.Equals(ownerPlayer))
            return;

        if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep != 29)
            return;

        if (_isUsed)
            return;

        if (skillType != Enumerators.SetType.EARTH)
        {
            if (manaCost <= ownerPlayer.Mana)
            {
                fightTargetingArrow = Instantiate(fightTargetingArrowPrefab).GetComponent<FightTargetingArrow>();
                fightTargetingArrow.BoardCards = _gameplayManager.PlayersInGame.Find(x => x != ownerPlayer).BoardCards;

                fightTargetingArrow.targetsType = new List<Enumerators.SkillTargetType>()
                {
                    Enumerators.SkillTargetType.OPPONENT_CARD,
                    Enumerators.SkillTargetType.PLAYER_CARD,
                    Enumerators.SkillTargetType.PLAYER,
                    Enumerators.SkillTargetType.OPPONENT
                };

                if (skillType == Enumerators.SetType.AIR)
                {
                    fightTargetingArrow.targetsType = new List<Enumerators.SkillTargetType>()
                    {
                        Enumerators.SkillTargetType.OPPONENT_CARD,
                        Enumerators.SkillTargetType.PLAYER_CARD,
                    };
                }

                fightTargetingArrow.Begin(transform.position);

                if (_tutorialManager.IsTutorial)
                    _tutorialManager.DeactivateSelectTarget();
            }
        }
    }

    private void OnMouseUp()
    {
        if (!ownerPlayer.IsLocalPlayer || !_gameplayManager.WhoseTurn.Equals(ownerPlayer))
            return;

        DoOnUpSkillAction();
    }

    public void DoOnUpSkillAction()
    {
        if (!_gameplayManager.WhoseTurn.Equals(ownerPlayer))
            return;

        if (_isUsed)
            return;

        if (manaCost <= ownerPlayer.Mana)
        {
            if (!_isOpponent)
            {
                if (GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    GameClient.Get<ITutorialManager>().ActivateSelectTarget();
                }
            }

            if (skillType == Enumerators.SetType.EARTH)
            {
                DoSkillAction(null);
            }
            else
            {
                if (!_isOpponent)
                {
                    if (fightTargetingArrow != null)
                    {
                        ResolveCombat();
                        _playerController.IsCardSelected = false;
                    }
                }
                else
                {
                    ResolveCombat();
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

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.USE_ABILITY);
            }

            else if (fightTargetingArrow.selectedCard != null)
            {
                var targetCard = fightTargetingArrow.selectedCard;
                if (targetCard.gameObject != gameObject &&
                    targetCard.gameObject.GetComponent<HandCard>() == null)
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
            ownerPlayer.Mana -= manaCost;

        CreateVFX(target);
        SetHighlightingEnabled(false);
        _isUsed = true;
    }

    private void FreezeAction(object target)
    {
        Debug.Log("FREEZE HIM");
        if (target is BoardCreature)
        {
            var creature = target as BoardCreature;


            //for (int i = 0; i < ownerPlayer.opponentBoardCardsList.Count; i++)
            //{
            //    if (ownerPlayer.opponentBoardCardsList[i] == creature)
            //    {
            //        creature = ownerPlayer.opponentBoardCardsList[i];
            //        break;
            //    }
            //}

            creature.Stun(_skillPower);
            CreateFrozenVFX(creature.transform.position);
        }
        //TODO for heroes
    }
    private void ToxicDamageAction(object target)
    {
        Debug.Log("POISON HIM");
        AttackWithModifiers(target, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
    }
    private void FireDamageAction(object target)
    {
        Debug.Log("BURN HIM");
        AttackWithModifiers(target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
    }
    private void HealAnyAction(object target)
    {
        Debug.Log("HEAL ANY");
        if (target is Player)
        {
            var player = target as Player;

            _battleController.HealPlayerBySkill(ownerPlayer, this, player);

             CreateHealVFX(player.AvatarObject.transform.position);
        }
        else
        {
            var creature = target as BoardCreature;

            _battleController.HealCreatureBySkill(ownerPlayer, this, creature);

            CreateHealVFX(creature.transform.position);
        }
    }

    /*
    private void CardReturnAction(object target)
    {
        var targetCreature = target as BoardCreature;

        Debug.Log("RETURN CARD");

        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

        Player creatureOwner = GetOwnerOfCreature(targetCreature);
        RuntimeCard returningCard = targetCreature.card;
        Vector3 creaturePosition = targetCreature.transform.position;

        // Debug.LogError("<color=white>------return card of " + creatureOwner.GetType() + "; human " + creatureOwner.isHuman + "; to hand-------</color>");
        // Debug.LogError("<color=white>------returning card " + returningCard.instanceId + " to hand-------</color>");

        // STEP 1 - REMOVE CREATURE FROM BOARD
        if (creatureOwner.playerBoardCardsList.Contains(targetCreature)) // hack
            creatureOwner.playerBoardCardsList.Remove(targetCreature);

        // STEP 2 - DESTROY CREATURE ON THE BOARD OR ANIMATE
        CreateVFX(creaturePosition);
        MonoBehaviour.Destroy(targetCreature.gameObject);

        // STEP 3 - REMOVE RUNTIME CARD FROM BOARD
        creatureOwner.playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(returningCard);
        creatureOwner.boardZone.RemoveCard(returningCard);

        var serverCurrentPlayer = creatureOwner.Equals(ownerPlayer) ? creatureOwner.GetServer().gameState.currentPlayer : creatureOwner.GetServer().gameState.currentOpponent;

        // STEP 4 - REMOVE CARD FROM SERVER BOARD
        var boardRuntimeCard = serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == returningCard.instanceId);
        serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);

        // STEP 5 - CREATE AND ADD TO SERVER NEW RUNTIME CARD FOR HAND
        var card = CreateRuntimeCard(targetCreature, creatureOwner.playerInfo, returningCard.instanceId);
        serverCurrentPlayer.namedZones[Constants.ZONE_HAND].cards.Add(card);

        // STEP 6 - CREATE NET CARD AND SIMULATE ANIMATION OF RETURNING CARD TO HAND
        var netCard = CreateNetCard(card);
        creatureOwner.ReturnToHandRuntimeCard(netCard, creatureOwner.playerInfo, creaturePosition);

        // STEP 7 - REARRANGE CREATURES ON THE BOARD
        GameClient.Get<IGameplayManager>().RearrangeHands();
    }

    private WorkingCard CreateRuntimeCard(BoardCreature targetCreature, PlayerInfo playerInfo, int instanceId)
    {
        var card = new RuntimeCard();
        card.cardId = targetCreature.card.cardId;
        card.instanceId = instanceId;// playerInfo.currentCardInstanceId++;
        card.ownerPlayer = playerInfo;
        card.stats[0] = targetCreature.card.stats[0];
        card.stats[1] = targetCreature.card.stats[1];
        card.namedStats[Constants.STAT_DAMAGE] = targetCreature.card.namedStats[Constants.STAT_DAMAGE].Clone();
        card.namedStats[Constants.STAT_HP] = targetCreature.card.namedStats[Constants.STAT_HP].Clone();
        card.namedStats[Constants.STAT_DAMAGE].baseValue = card.namedStats[Constants.STAT_DAMAGE].originalValue;
        card.namedStats[Constants.STAT_HP].baseValue = card.namedStats[Constants.STAT_HP].originalValue;
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
            netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);

        netCard.keywords = new NetKeyword[card.keywords.Count];

        idx = 0;

        foreach (var entry in card.keywords)
            netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);

        netCard.connectedAbilities = card.connectedAbilities.ToArray();

        return netCard;
    } 
     */

    private Player GetOwnerOfCreature(BoardCreature creature)
    {
        if (_battlegroundController.playerBoardCards.Contains(creature))
            return ownerPlayer;
        else
        {
            if (ownerPlayer.IsLocalPlayer)
                return _gameplayManager.GetOpponentPlayer();
            else
                return ownerPlayer;
        }
    }

    private void HealAction()
	{
		Debug.Log("HEAL HIM");

        _battleController.HealPlayerBySkill(ownerPlayer, this, ownerPlayer);

        CreateHealVFX(transform.position - Vector3.right * 2.3f);
    }
	

    private void AttackWithModifiers(object target, Enumerators.SetType attackType, Enumerators.SetType setType)
    {
        if (target is Player)
        {
            var player = target as Player;
            //TODO additional damage to heros

            _battleController.AttackPlayerBySkill(ownerPlayer, this, player);

            CreateAttackVFXByType(attackType, player.AvatarObject.transform.position);
        }
        else
        {
            var creature = target as BoardCreature;
            var attackModifier = 0;

            if (creature.Card.libraryCard.cardSetType == setType)
                attackModifier = 1;

            _battleController.AttackCreatureBySkill(ownerPlayer, this, creature);

            CreateAttackVFXByType(attackType, creature.transform.position);
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
        _fireDamageVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
        DestroyCurrentParticle(_fireDamageVFX);
    }
    private void CreateHealVFX(Vector3 pos)
    {
        _healVFX = MonoBehaviour.Instantiate(_healVFXprefab);
        _healVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
        DestroyCurrentParticle(_healVFX);
    }

    private void CreateAirVFX(Vector3 pos)
    {
        _airPickUpCardVFX = MonoBehaviour.Instantiate(_airPickUpCardVFXprefab);
        _airPickUpCardVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
        DestroyCurrentParticle(_airPickUpCardVFX);
    }

    private void CreateToxicAttackVFX(Vector3 pos)
    {
        _toxicVFX = MonoBehaviour.Instantiate(_toxicVFXprefab);
        _toxicVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
        DestroyCurrentParticle(_toxicVFX);
    }

    private void CreateFrozenVFX(Vector3 pos)
    {
        _frozenVFX = MonoBehaviour.Instantiate(_frozenVFXprefab);
        _frozenVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
        DestroyCurrentParticle(_frozenVFX);
    }

    private GameObject _firstParticle;
    private void CreateVFX(object target)
    {
        GameObject prefab = null;

        switch (skillType)
        {
            case Enumerators.SetType.WATER:
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetFrozenAttack");
                break;
            case Enumerators.SetType.TOXIC:
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/Toxic_Bullet_08");//SpellTargetToxicAttack
                break;
            case Enumerators.SetType.FIRE:
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/fireBall_03");
                break;
            case Enumerators.SetType.LIFE:
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");
                break;
            case Enumerators.SetType.EARTH:
             //   HealAction();
                break;
            case Enumerators.SetType.AIR:
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                break;
            default:
                break;
        }

        if (target == null)
            return;

        _firstParticle = MonoBehaviour.Instantiate(prefab);
        _firstParticle.transform.position = Utilites.CastVFXPosition(transform.position + Vector3.forward);

        if (target is Player)
        {
            var player = target as Player;
            //_firstParticle.transform.localEulerAngles = new Vector3(-90,0,AngleBetweenVector2(_firstParticle.transform.position, player.transform.position));
            _firstParticle.transform.DOMove(Utilites.CastVFXPosition(player.AvatarObject.transform.position), .5f).OnComplete(() => ActionCompleted(target));
        }
        else if (target is BoardCreature)
        {
            var cruature = target as BoardCreature;

            //_firstParticle.transform.localEulerAngles = new Vector3(-90, 0, AngleBetweenVector2(_firstParticle.transform.position, cruature.transform.position));
            _firstParticle.transform.DOMove(Utilites.CastVFXPosition(cruature.transform.position), .5f).OnComplete(() => ActionCompleted(target));
        }
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.x > vec1.x) ? -1.0f : 1.0f;
        return (180 + Vector2.Angle(Vector2.up, diference)) * sign;
    }

    private void ActionCompleted(object target)
    {
        if (_firstParticle != null)
            DestroyCurrentParticle(_firstParticle, true);

        switch (skillType)
        {
            case Enumerators.SetType.WATER:
                FreezeAction(target);
                break;
            case Enumerators.SetType.TOXIC:
                ToxicDamageAction(target);
                break;
            case Enumerators.SetType.FIRE:
                FireDamageAction(target);
                break;
            case Enumerators.SetType.LIFE:
                HealAnyAction(target);
                break;
            case Enumerators.SetType.AIR:
             //   CardReturnAction(target);
                break;
            default:
                break;
        }
    }

    private void DestroyCurrentParticle(GameObject currentParticle, bool isDirectly = false, float time = 5f)
    {
        if (isDirectly)
            DestroyParticle(new object[] { currentParticle });
        else
            GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, new object[] { currentParticle }, time, false);
    }

    private void DestroyParticle(object[] param)
    {
        GameObject particleObj = param[0] as GameObject;
        MonoBehaviour.Destroy(particleObj);
    }
}
