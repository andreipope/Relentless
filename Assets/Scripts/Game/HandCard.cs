// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using GrandDevs.CZB;
using GrandDevs.CZB.Common;
using UnityEngine;

[RequireComponent(typeof(CardView))]
public class HandCard : MonoBehaviour
{
    public DemoHumanPlayer ownerPlayer;
    public GameObject boardZone;

    protected CardView cardView;

    protected bool startedDrag;
    protected Vector3 initialPos;

    private bool _isHandCard = true;

    private bool _isReturnToHand = false;
    private bool _alreadySelected = false;
                

    private void Awake()
    {
        cardView = GetComponent<CardView>();
    }

    private void Start()
    {
        if (cardView.CanBePlayed(ownerPlayer))
        {
            cardView.SetHighlightingEnabled(true);
        }
        else
        {
            cardView.SetHighlightingEnabled(false);
        }
    }

    private void Update()
    {
        if (startedDrag)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var newPos = transform.position;
            newPos.z = 0;
            transform.position = newPos;
        }
    }

    public void OnSelected()
    {
        if (ownerPlayer.isActivePlayer &&
            cardView.CanBePlayed(ownerPlayer) && !_isReturnToHand && !_alreadySelected)
        {
            startedDrag = true;
            initialPos = transform.position;
            ownerPlayer.isCardSelected = true;
            _alreadySelected = true;
        }
    }

    public void OnMouseUp()
    {
        if (!startedDrag)
        {
            return;
        }
        _alreadySelected = false;
        startedDrag = false;
        ownerPlayer.isCardSelected = false;

        bool playable = true;

        if (!cardView.CanBeBuyed(ownerPlayer) || (cardView.libraryCard.cardKind == GrandDevs.CZB.Common.Enumerators.CardKind.CREATURE &&
                                                     ownerPlayer.boardZone.cards.Count >= Constants.MAX_BOARD_CREATURES))
            playable = false;
        
        if(playable)
        {
            if (boardZone.GetComponent<BoxCollider2D>().bounds.Contains(transform.position) && _isHandCard)
            {
                _isHandCard = false;
                ownerPlayer.PlayCard(cardView);
                cardView.SetHighlightingEnabled(false);
            }
            else
            {
                transform.position = initialPos;
                if (GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    GameClient.Get<ITutorialManager>().ActivateSelectTarget();
                }
            }
        }
        else
        {
            _isReturnToHand = true;

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            transform.DOMove(initialPos, 0.5f).OnComplete(() => 
            {
                transform.position = initialPos;
                _isReturnToHand = false;
            });
        }
    }

    public void ResetToInitialPosition()
    {
        transform.position = initialPos;
    }
}
