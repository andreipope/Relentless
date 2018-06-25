using System;

using UnityEngine;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;
using GrandDevs.CZB.Common;
using System.Collections.Generic;
using GrandDevs.CZB.Helpers;
using GrandDevs.Internal;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class BoardCreature : MonoBehaviour
    {
        public event Action CreatureOnDieEvent;
        public event Action<object> CreatureOnAttackEvent;

        public event Action<int, int> CreatureHPChangedEvent;
        public event Action<int, int> CreatureDamageChangedEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private PlayerController _playerController;
        private BattlegrdController _battlegroundController;

        private GameObject _fightTargetingArrowPrefab;

        private GameObject _selfObject;

        private SpriteRenderer pictureSprite;
        private SpriteRenderer frozenSprite;
        private SpriteRenderer glowSprite;

        private TextMeshPro attackText;
        private TextMeshPro healthText;

        private ParticleSystem sleepingParticles;

        public Player ownerPlayer;

        private TargetingArrow abilitiesTargetingArrow;
        private FightTargetingArrow fightTargetingArrow;


        public bool hasImpetus;
        public bool hasProvoke;
        public int numTurnsOnBoard;

        protected Action<int, int> onAttackStatChangedDelegate;
        protected Action<int, int> onHealthStatChangedDelegate;


        private int _stunTurns = 0;

        private AnimationEventTriggering arrivalAnimationEventHandler;

        private GameObject creatureContentObject;

        private Animator creatureAnimator;

        public List<CreatureAnimatorInfo> animatorControllers;

        public int Damage { get; protected set; }
        public int HP { get; protected set; }

        public int initialDamage;
        public int initialHP;

        public bool IsPlayable { get; set; }

        public Card Card { get; private set; }

        public int InstanceId { get; private set; }

        public bool IsStun
        {
            get { return (_stunTurns > 0 ? true : false); }
        }

        public BoardCreature(Transform parent)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _battlegroundController = _gameplayManager.GetController<BattlegrdController>();
            _playerController = _gameplayManager.GetController<PlayerController>();

            _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>(""));
            _selfObject.transform.SetParent(parent, false);

            _fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/FightTargetingArrow");

            pictureSprite = _selfObject.transform.Find("").GetComponent<SpriteRenderer>();
            frozenSprite = _selfObject.transform.Find("").GetComponent<SpriteRenderer>();
            glowSprite = _selfObject.transform.Find("").GetComponent<SpriteRenderer>();

            attackText = _selfObject.transform.Find("").GetComponent<TextMeshPro>();
            healthText = _selfObject.transform.Find("").GetComponent<TextMeshPro>();

            sleepingParticles = _selfObject.transform.Find("").GetComponent<ParticleSystem>();

            creatureAnimator = _selfObject.transform.Find("").GetComponent<Animator>();

            creatureContentObject = _selfObject.transform.Find("").gameObject;

            arrivalAnimationEventHandler = _selfObject.transform.Find("").GetComponent<AnimationEventTriggering>();

            arrivalAnimationEventHandler.OnAnimationEvent += ArrivalAnimationEventHandler;

            animatorControllers = new List<CreatureAnimatorInfo>();
            for (int i = 0; i < Enum.GetNames(typeof(Enumerators.CardType)).Length; i++)
            {
                animatorControllers.Add(new CreatureAnimatorInfo()
                {
                    animator = _loadObjectsManager.GetObjectByPath<RuntimeAnimatorController>(""),
                    cardType = (Enumerators.CardType)i
                });
            }
        }

        public void Die()
        {
            CreatureHPChangedEvent -= onHealthStatChangedDelegate;
            CreatureDamageChangedEvent -= onAttackStatChangedDelegate;

            CreatureOnDieEvent?.Invoke();
        }

        public void ArrivalAnimationEventHandler(string param)
        {
            if (param.Equals("ArrivalAnimationDone"))
            {
                Debug.Log("hasImpetus = " + hasImpetus);
                creatureContentObject.SetActive(true);
                if (hasImpetus)
                {
                    //  frameSprite.sprite = frameSprites[1];
                    StopSleepingParticles();
                    if (ownerPlayer != null)
                        SetHighlightingEnabled(true);
                }


                InternalTools.SetLayerRecursively(gameObject, 0);

                if (Card.cardRarity == Enumerators.CardRarity.EPIC)
                {
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, Card.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY + "1", Constants.ZOMBIES_SOUND_VOLUME, false, true);
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, Card.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY + "2", Constants.ZOMBIES_SOUND_VOLUME / 2f, false, true);
                }
                else
                {
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, Card.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY, Constants.ZOMBIES_SOUND_VOLUME, false, true);
                }


                if (Card.name.Equals("Freezzee"))
                {
                    var freezzees = GetEnemyCreaturesList(this).FindAll(x => x.Card.id == Card.id);

                    if (freezzees.Count > 0)
                    {
                        foreach (var creature in freezzees)
                        {
                            creature.Stun(1);
                            CreateFrozenVFX(creature.transform.position);
                        }
                    }
                }
            }
        }

        private void CreateFrozenVFX(Vector3 pos)
        {
            var _frozenVFX = MonoBehaviour.Instantiate(GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX"));
            _frozenVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
            DestroyCurrentParticle(_frozenVFX);
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

        private List<BoardCreature> GetEnemyCreaturesList(BoardCreature creature)
        {
            if (_gameplayManager.GetLocalPlayer().BoardCards.Contains(creature))
                return _gameplayManager.GetAIPlayer().BoardCards;
            return _gameplayManager.GetLocalPlayer().BoardCards;
        }

        public virtual void PopulateWithInfo(Card card, string setName = "")
        {
            Card = card;



            var rarity = Enum.GetName(typeof(Enumerators.CardRarity), Card.cardRarity);

            pictureSprite.sprite = Resources.Load<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), Card.picture.ToLower()));

            pictureSprite.transform.localPosition = MathLib.FloatVector3ToVector3(Card.cardViewInfo.position);
            pictureSprite.transform.localScale = MathLib.FloatVector3ToVector3(Card.cardViewInfo.scale);

            creatureAnimator.runtimeAnimatorController = animatorControllers.Find(x => x.cardType == Card.cardType).animator;
            if (Card.cardType == Enumerators.CardType.WALKER)
            {
                sleepingParticles.transform.position += Vector3.up * 0.7f;
            }

            Damage = card.damage;
            HP = card.health;

            initialDamage = Damage;
            initialHP = HP;

            attackText.text = Damage.ToString();
            healthText.text = HP.ToString();


            onAttackStatChangedDelegate = (oldValue, newValue) =>
            {
                UpdateStatText(attackText, Damage, initialDamage);
            };

            CreatureDamageChangedEvent += onAttackStatChangedDelegate;

            onHealthStatChangedDelegate = (oldValue, newValue) =>
            {
                UpdateStatText(healthText, HP, initialHP);
            };

            CreatureHPChangedEvent += onHealthStatChangedDelegate;


            switch (Card.cardType)
            {
                case Enumerators.CardType.FERAL:
                    Debug.Log("hasImpetus = true");
                    hasImpetus = true;
                    IsPlayable = true;
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.FERAL_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    break;
                case Enumerators.CardType.HEAVY:
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.HEAVY_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    hasProvoke = true;
                    break;
                default:
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.WALKER_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    break;
            }

            if (hasProvoke)
            {
                //   glowSprite.gameObject.SetActive(false);
                //  pictureMaskTransform.localScale = new Vector3(50, 55, 1);
                // frameSprite.sprite = frameSprites[2];
            }
            SetHighlightingEnabled(false);

            creatureAnimator.StopPlayback();
            creatureAnimator.Play(0);
        }

        public void PlayArrivalAnimation()
        {
            creatureAnimator.SetTrigger("Active");
        }

        public void OnStartTurn()
        {
            numTurnsOnBoard += 1;
            StopSleepingParticles();

            if (ownerPlayer != null && IsPlayable)
                SetHighlightingEnabled(true);
        }

        public void OnEndTurn()
        {
            if (_stunTurns > 0)
                _stunTurns--;
            if (_stunTurns == 0)
            {
                IsPlayable = true;
                frozenSprite.DOFade(0, 1);
            }

            CancelTargetingArrows();
        }

        public void Stun(int turns)
        {
            Debug.Log("WAS STUNED");
            if (turns > _stunTurns)
                _stunTurns = turns;
            IsPlayable = false;

            frozenSprite.DOFade(1, 1);
            //sleepingParticles.Play();
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

        private void UpdateStatText(TextMeshPro text, int stat, int initialStat)
        {
            if (text == null || !text)
                return;

            text.text = stat.ToString();
            if (stat > initialStat)
            {
                text.color = Color.green;
            }
            else if (stat < initialStat)
            {
                text.color = Color.red;
            }
            else
            {
                // if (stat.statId == 1)
                text.color = Color.white;
                //  else
                //    text.color = Color.black;
            }
            var sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOScale(new Vector3(1.4f, 1.4f, 1.0f), 0.4f));
            sequence.Append(text.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f));
            sequence.Play();
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            glowSprite.enabled = enabled;
        }

        public void StopSleepingParticles()
        {
            if (sleepingParticles != null)
                sleepingParticles.Stop();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var targetingArrow = collider.transform.parent.parent.GetComponent<TargetingArrow>();
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
                var targetingArrow = collider.transform.parent.parent.GetComponent<TargetingArrow>();
                if (targetingArrow != null)
                {
                    targetingArrow.OnCardUnselected(this);
                }
            }
        }

        private void OnMouseDown()
        {
            //if (fightTargetingArrowPrefab == null)
            //    return;

            //Debug.LogError(IsPlayable + " | " + ownerPlayer.isActivePlayer + " | " + ownerPlayer);

            if (ownerPlayer != null && _playerController.IsActive && IsPlayable)
            {
                fightTargetingArrow = Instantiate(_fightTargetingArrowPrefab).GetComponent<FightTargetingArrow>();
                fightTargetingArrow.targetType = EffectTarget.OpponentOrOpponentCreature;
                fightTargetingArrow.BoardCards = ownerPlayer.BoardCards;
                fightTargetingArrow.Begin(transform.position);

                // WARNING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ONLY FOR PLAYER!!!!! IMPROVE IT
                if (ownerPlayer.Equals(_gameplayManager.GetLocalPlayer()))
                {
                    _battlegroundController.DestroyCardPreview();
                    _playerController.IsCardSelected = true;

                    if (GameClient.Get<ITutorialManager>().IsTutorial)
                    {
                        GameClient.Get<ITutorialManager>().DeactivateSelectTarget();
                    }
                }
            }
        }

        private void OnMouseUp()
        {
            if (fightTargetingArrow != null)
            {
                fightTargetingArrow.End(this);

                if (ownerPlayer.Equals(_gameplayManager.GetLocalPlayer()))
                {
                    _playerController.IsCardSelected = false;
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
                    SetHighlightingEnabled(false);
                    IsPlayable = false;


                    //         GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                    //sortingGroup.sortingOrder = 100;
                    CombatAnimation.PlayFightAnimation(gameObject, targetPlayer.gameObject, 0.1f, () =>
                    {
                        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, Card.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                        Vector3 positionOfVFX = targetPlayer.transform.position;
                        positionOfVFX.y = 4.45f;

                        PlayAttackVFX(Card.cardType, positionOfVFX, Damage);

                        ownerPlayer.FightPlayer(card);
                        CreatureOnAttackEvent?.Invoke(targetPlayer);
                    },
                    () =>
                    {
                    //sortingGroup.sortingOrder = 0;
                    fightTargetingArrow = null;
                    });
                }
                if (fightTargetingArrow.selectedCard != null)
                {
                    var targetCard = fightTargetingArrow.selectedCard;
                    SetHighlightingEnabled(false);
                    IsPlayable = false;


                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, Card.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                    //sortingGroup.sortingOrder = 100;
                    if (targetCard != GetComponent<BoardCreature>() &&
                        targetCard.GetComponent<HandCard>() == null)
                    {

                        // play sound when target creature attack more than our
                        if (targetCard.Damage > Damage)
                        {
                            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, targetCard.Card.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);
                        }

                        CombatAnimation.PlayFightAnimation(gameObject, targetCard.gameObject, 0.5f, () =>
                        {
                            Debug.Log("CreatureOnAttackEvent?.Invoke(targetCard)");
                            PlayAttackVFX(Card.cardType, targetCard.transform.position, Damage);

                            ownerPlayer.FightCreature(Card, targetCard.Card);
                            CreatureOnAttackEvent?.Invoke(targetCard);
                        },
                        () =>
                        {
                        //sortingGroup.sortingOrder = 0;
                        fightTargetingArrow = null;
                        });
                    }
                }
                if (fightTargetingArrow.selectedCard == null && fightTargetingArrow.selectedPlayer == null)
                {
                    if (GameClient.Get<ITutorialManager>().IsTutorial)
                    {
                        GameClient.Get<ITutorialManager>().ActivateSelectTarget();
                    }
                }
            }
        }

        public void CreatureOnAttack(object target)
        {
            CreatureOnAttackEvent?.Invoke(target);
        }
    }

    [Serializable]
    public class CreatureAnimatorInfo
    {
        public Enumerators.CardType cardType;
        public RuntimeAnimatorController animator;
    }
}