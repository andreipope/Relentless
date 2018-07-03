// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;
using TMPro;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB
{
    public class BoardWeapon
    {
        public event Action BoardWeaponStartedAttackEvent,
                            BoardWeaponAttackedEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayController;
        private AnimationsController _animationsController;
        private BattleController _battleController;
        private ActionsQueueController _actionsQueueController;

        private GameObject _selfObject,
                           _currentPlayerAvatar,
                           _playerAvatarShine,
                           _vfxObject;

        private SpriteRenderer _weaponIcon;


        private WeaponBoardArrow _targettingArrow;
        private Player _owner;

        private int _health,
                    _damage;

        private List<Enumerators.AbilityTargetType> _targets;

        private TextMeshPro _healthText,
                            _damageText;

        private GameObject _healthObject,
                            _damageObject,
                            _siloObject;

        private Player _player;
        private BoardUnit _creature;

        private OnBehaviourHandler _onMouseHandler;

        private bool _isOpponentWeapon = false;
        private Animator _siloAnimator;

        private Data.Card _weaponCard;


        public int Damage { get { return _damage; } }


        public bool CanAttack { get; set; }

        public BoardWeapon(GameObject objectOnBoard, Data.Card card)
        {
            _weaponCard = card;

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayController = GameClient.Get<IGameplayManager>();
            _animationsController = _gameplayController.GetController<AnimationsController>();
            _battleController = _gameplayController.GetController<BattleController>();
            _actionsQueueController = _gameplayController.GetController<ActionsQueueController>();

            _selfObject = objectOnBoard;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/ChainsawVFX");

            _weaponIcon = _selfObject.transform.Find("Icon").GetComponent<SpriteRenderer>();

            _healthObject = _selfObject.transform.Find("Health").gameObject;
            //_damageObject = _selfObject.transform.Find("Attack").gameObject;
            _damageObject = _selfObject.transform.parent.Find("Avatar/Weapon_Attack").gameObject;

            _siloObject = _selfObject.transform.Find("silo_mask").gameObject;
            _siloAnimator = _siloObject.GetComponent<Animator>();

            _healthText = _healthObject.transform.Find("Text").GetComponent<TextMeshPro>();
            _damageText = _damageObject.transform.Find("Text").GetComponent<TextMeshPro>();

            _currentPlayerAvatar = _selfObject.transform.parent.Find("Avatar").gameObject;
            _playerAvatarShine = _selfObject.transform.parent.Find("Shine").gameObject;

            _onMouseHandler = _currentPlayerAvatar.GetComponent<OnBehaviourHandler>();

            _weaponIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", 
                GameClient.Get<IGameplayManager>().GetCardSet(_weaponCard).ToLower(), _weaponCard.rarity.ToLower(), _weaponCard.picture.ToLower()));

            _healthObject.SetActive(true);
            _damageObject.SetActive(true);
            //_siloObject.SetActive(false);
            _siloAnimator.SetBool("Active", true);
        }

        public void InitWeapon(int damage, int health, Player owner, List<Enumerators.AbilityTargetType> targets)
        {
            _owner = owner;

            _owner.OnEndTurnEvent += OnEndTurnEventHandler;

            _health = health;
            _damage = damage;
            _targets = targets;

            UpdateUI();

            _selfObject.SetActive(true);
        }

        private void OnEndTurnEventHandler()
        {
            if (!_isOpponentWeapon)
            {
                _onMouseHandler.OnMouseDownEvent -= OnMouseDownEventHandler;
                _onMouseHandler.OnMouseUpEvent -= OnMouseUpEventHandler;
            }

            CanAttack = false;
            _owner.AlreadyAttackedInThisTurn = true;

            DisableTargettig();
            _playerAvatarShine.SetActive(false);
            _siloAnimator.SetBool("Active", false);
        }

        public void ActivateWeapon(bool opponent)
        {
            CanAttack = true;

            _isOpponentWeapon = opponent;

            if (!_isOpponentWeapon)
            {
                _onMouseHandler.OnMouseDownEvent += OnMouseDownEventHandler;
                _onMouseHandler.OnMouseUpEvent += OnMouseUpEventHandler;
            }

            _playerAvatarShine.SetActive(true);
            _siloAnimator.SetBool("Active", true);
        }

        public void ImmediatelyAttack(object target)
        {
            if (target == null)
                return;

            if (target is Player)
                _player = target as Player;
            else if (target is BoardUnit)
                _creature = target as BoardUnit;
            else
                return;

            if (!_isOpponentWeapon)
            {
                _onMouseHandler.OnMouseDownEvent += OnMouseDownEventHandler;
                _onMouseHandler.OnMouseUpEvent += OnMouseUpEventHandler;
            }

            Attack();
        }

        private void OnMouseDownEventHandler(GameObject obj)
        {
            EnableTargetting();
        }

        private void OnMouseUpEventHandler(GameObject obj)
        {
          //  DisableTargettig();
        }

        public void EnableTargetting()
        {
            if (_targettingArrow != null)
                DisableTargettig();

            _targettingArrow = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/WeaponTargettingArrow")).GetComponent<WeaponBoardArrow>();
            _targettingArrow.possibleTargets = _targets;

            _targettingArrow.OnPlayerSelectedEvent += OnPlayerSelectedEventHandler;
            _targettingArrow.OnPlayerUnselectedEvent += OnPlayerUnselectedEventHandler;
            _targettingArrow.OnCardSelectedEvent += OnCardSelectedEventHandler;
            _targettingArrow.OnCardUnselectedevent += OnCardUnselectedeventHandler;
            _targettingArrow.OnInputEndEvent += OnInputEndEventHandler;

            _targettingArrow.Begin(_currentPlayerAvatar.transform.position);
        }

        private void DisableTargettig()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.OnPlayerSelectedEvent -= OnPlayerSelectedEventHandler;
                _targettingArrow.OnPlayerUnselectedEvent -= OnPlayerUnselectedEventHandler;
                _targettingArrow.OnCardSelectedEvent -= OnCardSelectedEventHandler;
                _targettingArrow.OnCardUnselectedevent -= OnCardUnselectedeventHandler;
                _targettingArrow.OnInputEndEvent -= OnInputEndEventHandler;

                MonoBehaviour.Destroy(_targettingArrow.gameObject);
                _targettingArrow = null;
            }
        }

        private void UpdateUI()
        {
            _healthText.text = _health.ToString();
            _damageText.text = _damage.ToString();
        }

        private void PlayAttackAnimationOnTarget(GameObject target, Action onHitAction)
        {
            CreateVFX(target.transform.position + Vector3.forward * 10 - Vector3.up * 2);

            _animationsController.PlayFightAnimation(_currentPlayerAvatar, target, 0.5f, () =>
            {
                UpdateUI();
                if (onHitAction != null) onHitAction();
            }, () => { _siloAnimator.SetBool("Active", false); }, false, 1f);
        }

        private void Attack()
        {
            BoardWeaponStartedAttackEvent?.Invoke();

            _actionsQueueController.AddNewActionInToQueue((parameter, actionComplete) =>
            {
                if (_player != null)
                {
                    PlayAttackAnimationOnTarget(_player.AvatarObject, () =>
                    {
                        _battleController.AttackPlayerByWeapon(_owner, this, _player);
                    });
                    //CreateVFX(_player.transform.position);
                }
                else if (_creature != null)
                {
                    int damageToUs = _creature.Damage;

                    PlayAttackAnimationOnTarget(_creature.transform.gameObject, () =>
                    {
                        _battleController.AttackCreatureByWeapon(_owner, this, _creature);
                        _owner.HP -= damageToUs;
                    });
                    //CreateVFX(_creature.transform.position);     
                }
                else
                {
                    DisableTargettig();
                    actionComplete?.Invoke();
                    return;
                }

                _health--;

                CanAttack = false;

                _owner.AlreadyAttackedInThisTurn = true;

                if (!_isOpponentWeapon)
                {
                    _onMouseHandler.OnMouseDownEvent -= OnMouseDownEventHandler;
                    _onMouseHandler.OnMouseUpEvent -= OnMouseUpEventHandler;
                }

                DisableTargettig();

                _playerAvatarShine.SetActive(false);

                //UpdateUI();

                CheckIsDie();

                BoardWeaponAttackedEvent?.Invoke();

                actionComplete?.Invoke();
            });
        }

        private void CreateVFX(Vector3 position)
        {
            _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
            _vfxObject.transform.position = Utilites.CastVFXPosition((position - Constants.VFX_OFFSET) + Vector3.forward + Vector3.up*2);
        }

        private void OnInputEndEventHandler()
        {
            Attack();
        }

        private void CheckIsDie()
        {
            if (_health <= 0)
            {
                _owner.DestroyWeapon();
            }
        }

        private void OnPlayerSelectedEventHandler(Player obj)
        {
            _player = obj;
            _creature = null;
        }

        private void OnPlayerUnselectedEventHandler(Player obj)
        {
            _player = null;
        }

        private void OnCardSelectedEventHandler(BoardUnit obj)
        {
            _creature = obj;
            _player = null;
        }

        private void OnCardUnselectedeventHandler(BoardUnit obj)
        {
            _creature = null;
        }

        public void Destroy()
        {
            //_selfObject.SetActive(false);
            _healthObject.SetActive(false);
            _damageObject.SetActive(false);
            //_siloObject.SetActive(true);
            _siloAnimator.SetBool("Active", false);
        }
    }
}