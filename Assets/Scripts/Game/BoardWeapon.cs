using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;
using TMPro;

namespace GrandDevs.CZB
{
    public class BoardWeapon
    {
        public event Action BoardWeaponStartedAttackEvent,
                            BoardWeaponAttackedEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _selfObject,
                           _currentPlayerAvatar,
                           _playerAvatarShine,
                           _vfxObject;


        private WeaponTargettingArrow _targettingArrow;
        private Player _owner;

        private int _health,
                    _damage;

        private List<Enumerators.AbilityTargetType> _targets;

        private TextMeshPro _healthText,
                            _damageHealth;

        private PlayerAvatar _player;
        private BoardCreature _creature;

        private OnMouseHandler _onMouseHandler;

        private bool _isOpponentWeapon = false;


        public bool CanAttack { get; set; }

        public BoardWeapon(GameObject objectOnBoard)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _selfObject = objectOnBoard;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");

            _healthText = _selfObject.transform.Find("Health/Text").GetComponent<TextMeshPro>();
            _damageHealth = _selfObject.transform.Find("Attack/Text").GetComponent<TextMeshPro>();

            _currentPlayerAvatar = _selfObject.transform.parent.Find("Avatar").gameObject;
            _playerAvatarShine = _currentPlayerAvatar.transform.Find("Shine").gameObject;

            _onMouseHandler = _currentPlayerAvatar.GetComponent<OnMouseHandler>();
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
        }

        public void ImmediatelyAttack(object target)
        {
            if (target == null)
                return;

            if (target is PlayerAvatar)
                _player = target as PlayerAvatar;
            else if (target is BoardCreature)
                _creature = target as BoardCreature;
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

        }

        public void EnableTargetting()
        {
            if (_targettingArrow != null)
                DisableTargettig();

            _targettingArrow = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/WeaponTargettingArrow")).GetComponent<WeaponTargettingArrow>();
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
            _damageHealth.text = _damage.ToString();
        }

        private void Attack()
        {
            BoardWeaponStartedAttackEvent?.Invoke();

            Debug.Log("ATTACK!!!!");

            //todo change on animated attack and other attack type
            if (_player != null)
            {
                CreateVFX(_player.transform.position);

                if (_player.playerInfo.netId == _owner.netId)
                    _owner.FightPlayerBySkill(_damage, false);
                else
                    _owner.FightPlayerBySkill(_damage);
            }
            else if (_creature != null)
            {
                CreateVFX(_creature.transform.position);
                _owner.FightCreatureBySkill(_damage, _creature.card);
            }
            else
            {
                DisableTargettig();
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

            UpdateUI();

            CheckIsDie();

            BoardWeaponAttackedEvent?.Invoke();
        }

        private void CreateVFX(Vector3 position)
        {
            _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
            _vfxObject.transform.position = (position - Constants.VFX_OFFSET) + Vector3.forward;
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

        private void OnPlayerSelectedEventHandler(PlayerAvatar obj)
        {
            _player = obj;
            _creature = null;
        }

        private void OnPlayerUnselectedEventHandler(PlayerAvatar obj)
        {
            _player = null;
        }

        private void OnCardSelectedEventHandler(BoardCreature obj)
        {
            _creature = obj;
            _player = null;
        }

        private void OnCardUnselectedeventHandler(BoardCreature obj)
        {
            _creature = null;
        }

        public void Destroy()
        {
            _selfObject.SetActive(false);
        }
    }
}