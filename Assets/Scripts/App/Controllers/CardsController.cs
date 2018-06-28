using DG.Tweening;
using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandDevs.CZB
{
    public class CardsController : IController
    {
        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;
        private ILoadObjectsManager _loadObjectsManager;

        private BattlegroundController _battlgroundController;
        private VFXController _vfxController;
        private AbilitiesController _abilitiesController;

        private GameObject _playerBoard;
        private GameObject _opponentBoard;


        private int _cardInstanceId = 0;

        public GameObject creatureCardViewPrefab,
                           opponentCardPrefab,
                           spellCardViewPrefab;




        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _battlgroundController = _gameplayManager.GetController<BattlegroundController>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            creatureCardViewPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            spellCardViewPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            opponentCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/OpponentCard");

            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public int GetNewCardInstanceId()
        {
            return _cardInstanceId++;
        }

        private void OnGameStartedEventHandler()
        {
            _cardInstanceId = 0;

            _playerBoard = GameObject.Find("PlayerBoard");
            _opponentBoard = GameObject.Find("OpponentBoard");
        }


        public void AddCardToHand(Player player, WorkingCard card)
        {
            player.RemoveCardFromDeck(card);
            player.AddCardToHand(card);
        }

        public GameObject AddCardToHand(WorkingCard card)
        {
            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            GameObject go = null;
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                go = MonoBehaviour.Instantiate(creatureCardViewPrefab);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                go = MonoBehaviour.Instantiate(spellCardViewPrefab);
            }

            var cardView = go.GetComponent<CardView>();
            cardView.PopulateWithInfo(card, cardSetName);

            cardView.CurrentTurn = _battlgroundController.currentTurn;

            if (_battlgroundController.currentTurn == 0)
            {
                cardView.SetDefaultAnimation(card.owner.CardsInHand.Count);
                //if(playerHandCards.Count == 4)
                //    RearrangeHand();
            }

            var handCard = go.AddComponent<HandCard>();
            handCard.ownerPlayer = card.owner;
            handCard.boardZone = _playerBoard;

            cardView.transform.localScale = Vector3.one * .3f;
            // card.owner.CardsInHand.Add(card);

            //go.GetComponent<SortingGroup>().sortingOrder = playerHandCards.Count;

            _battlgroundController.playerHandCards.Add(cardView);

            return go;
        }

        public GameObject AddCardToOpponentHand(WorkingCard card)
        {
            var opponent = _gameplayManager.GetOpponentPlayer();
            var go = MonoBehaviour.Instantiate(opponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;

            _battlgroundController.opponentHandCards.Add(go);

            return go;
        }


        public void RemoveCard(object[] param)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            CardView card = param[0] as CardView;
            //BoardCreature currentCreature = null;
            //if (param.Length > 1)
            //    currentCreature = param[1] as BoardCreature;

            var go = card.gameObject;

            //if (!go.transform.Find("BackgroundBack").gameObject.activeSelf)
            //    return;

            var sortingGroup = card.GetComponent<SortingGroup>();



            Sequence animationSequence3 = DOTween.Sequence();
            //animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, 90), .2f));
            animationSequence3.Append(go.transform.DORotate(new Vector3(0, 90, 90), .3f));
            //go.transform.DOScale(new Vector3(.19f, .19f, .19f), .2f);
            go.transform.DOScale(new Vector3(.195f, .195f, .195f), .2f);
            animationSequence3.OnComplete(() =>
            {

                go.transform.Find("Back").gameObject.SetActive(true);
                Sequence animationSequence4 = DOTween.Sequence();
                //animationSequence4.Append(go.transform.DORotate(new Vector3(40f, 180, 90f), .3f));
                animationSequence4.Append(go.transform.DORotate(new Vector3(0, 180, 0f), .45f));
                //animationSequence4.AppendInterval(2f);

                //Changing layers to all child objects to set them Behind the Graveyard Card
                sortingGroup.sortingLayerName = "Foreground";
                sortingGroup.sortingOrder = 7;

                sortingGroup.gameObject.layer = 0;

                for (int i = 0; i < sortingGroup.transform.childCount; i++)
                {
                    Transform child = sortingGroup.transform.GetChild(i);

                    if (child.name != "Back")
                    {
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        child.gameObject.layer = 0;
                    }
                }
            });

            Sequence animationSequence2 = DOTween.Sequence();
            //animationSequence2.Append(go.transform.DOMove(new Vector3(-4.1f, -1, 0), .3f));
            animationSequence2.Append(go.transform.DOMove(new Vector3(-6.57f, -1, 0), 0.7f));


            animationSequence2.OnComplete(() =>
            {


                for (int i = 0; i < sortingGroup.transform.childCount; i++)
                {
                    Transform child = sortingGroup.transform.GetChild(i);

                    if (child.name == "Back")
                    {
                        child.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    }
                }


                Sequence animationSequence5 = DOTween.Sequence();
                animationSequence5.Append(go.transform.DOMove(new Vector3(-6.57f, -4.352f, 0), .5f));
                animationSequence5.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(go);
                });
            });
        }

        public void RemoveOpponentCard(object[] param)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            GameObject go = param[0] as GameObject;
            BoardCreature currentCreature = null;
            if (param.Length > 1)
                currentCreature = param[1] as BoardCreature;

            //if (!go.transform.Find("BackgroundBack").gameObject.activeSelf)
            //    return;
            var sortingGroup = go.GetComponent<SortingGroup>();

            Sequence animationSequence3 = DOTween.Sequence();
            //animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, 90), .2f));
            animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, -30f), .4f));
            go.transform.DOScale(new Vector3(1, 1, 1), .2f);
            animationSequence3.OnComplete(() =>
            {

                //    if (go.transform.Find("BackgroundBack") != null)
                //        go.transform.Find("BackgroundBack").gameObject.SetActive(true);
                //    //Sequence animationSequence4 = DOTween.Sequence();
                //    //animationSequence4.Append(go.transform.DORotate(new Vector3(40f, 180, 90f), .3f));
                //    //animationSequence4.AppendInterval(2f);
            });

            Sequence animationSequence2 = DOTween.Sequence();
            //animationSequence2.Append(go.transform.DOMove(new Vector3(-4.85f, 6.3f, 0), .3f));
            animationSequence2.Append(go.transform.DOMove(new Vector3(6.535f, 14f, 0), .6f));

            animationSequence2.OnComplete(() =>
            {
                go.layer = 0;
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    go.transform.GetChild(i).gameObject.layer = 0;
                }

                //sortingGroup.sortingLayerName = "Default";
                sortingGroup.sortingOrder = 7; // Foreground layer

                Sequence animationSequence4 = DOTween.Sequence();
                animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0f, 0f), .2f));

                Sequence animationSequence5 = DOTween.Sequence();
                animationSequence5.Append(go.transform.DOMove(new Vector3(6.535f, 6.306f, 0), .5f));
                animationSequence5.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(go);
                });
            });
        }

        public void PlayCard(Player player, CardView card, HandCard handCard)
        {
            if (card.CanBePlayed(card.WorkingCard.owner))
            {
                GameClient.Get<IUIManager>().GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

                var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.WorkingCard.libraryCard.id);

                string cardSetName = string.Empty;
                foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
                {
                    if (cardSet.cards.IndexOf(libraryCard) > -1)
                        cardSetName = cardSet.name;
                }

                card.transform.DORotate(Vector3.zero, .1f);
                card.GetComponent<HandCard>().enabled = false;

                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
                // GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.CREATURE)
                {
                    int indexOfCard = 0;
                    float newCreatureCardPosition = card.transform.position.x;

                    // set correct position on board depends from card view position
                    for (int i = 0; i < player.BoardCards.Count; i++)
                    {
                        if (newCreatureCardPosition > player.BoardCards[i].transform.position.x)
                            indexOfCard = i + 1;
                        else break;
                    }

                    var boardCreature = new BoardCreature(_playerBoard.transform);
                    boardCreature.transform.tag = Constants.TAG_PLAYER_OWNED;
                    boardCreature.transform.parent = _playerBoard.transform;
                    boardCreature.transform.position = new Vector2(1.9f * player.BoardCards.Count, 0);
                    boardCreature.ownerPlayer = card.WorkingCard.owner;
                    boardCreature.PopulateWithInfo(card.WorkingCard, cardSetName);

                    player.CardsInHand.Remove(card.WorkingCard);

                    _battlgroundController.RearrangeHand();

                    player.BoardCards.Insert(indexOfCard, boardCreature);

                    GameClient.Get<ITimerManager>().AddTimer((creat) =>
                    {
                        card.WorkingCard.owner.GraveyardCardsCount++;
                    }, null, 1f);

                    //Destroy(card.gameObject);
                    card.removeCardParticle.Play();


                    Sequence animationSequence = DOTween.Sequence();
                    animationSequence.Append(card.transform.DOScale(new Vector3(.27f, .27f, .27f), 1f));
                    animationSequence.OnComplete(() =>
                    {
                        RemoveCard(new object[] { card });
                        _timerManager.AddTimer(_vfxController.PlayArrivalAnimationDelay, new object[] { boardCreature }, 0.1f, false);
                    });

                    //GameClient.Get<ITimerManager>().AddTimer(RemoveCard, new object[] {card}, 0.5f, false);
                    //_timerManager.AddTimer(PlayArrivalAnimationDelay, new object[] { currentCreature }, 0.7f, false);

                    _battlgroundController.RearrangeBottomBoard(() =>
                    {
                        _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard, Enumerators.CardKind.CREATURE, boardCreature, CallCardPlay, true);
                    });

                    //Debug.Log("<color=green> Now type: " + libraryCard.cardType + "</color>" + boardCreature.transform.position + "  " + currentCreature.transform.position);
                    //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                }
                else if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL)
                {
                    //var spellsPivot = GameObject.Find("PlayerSpellsPivot");
                    //var sequence = DOTween.Sequence();
                    //sequence.Append(card.transform.DOMove(spellsPivot.transform.position, 0.5f));
                    //sequence.Insert(0, card.transform.DORotate(Vector3.zero, 0.2f));
                    //sequence.Play().OnComplete(() =>
                    //{ 
                    card.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";
                    card.GetComponent<SortingGroup>().sortingOrder = 1000;

                    var boardSpell = card.gameObject.AddComponent<BoardSpell>();

                    Debug.Log(card.name);

                    _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard, Enumerators.CardKind.SPELL, boardSpell, CallSpellCardPlay, true, handCard: handCard);
                    //});
                }
            }
            else
            {
                card.GetComponent<HandCard>().ResetToInitialPosition();
            }
        }

        private void CallCardPlay(CardView card)
        {
           // PlayCreatureCard(card.WorkingCard);
            GameClient.Get<IUIManager>().GetPage<GameplayPage>().SetEndTurnButtonStatus(true);
        }

        private void CallSpellCardPlay(CardView card)
        {
          //  PlaySpellCard(card.WorkingCard);
            GameClient.Get<IUIManager>().GetPage<GameplayPage>().SetEndTurnButtonStatus(true);
        }

    }
}
