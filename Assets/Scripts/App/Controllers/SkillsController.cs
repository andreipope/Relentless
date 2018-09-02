using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SkillsController : IController
    {
        public BoardSkill OpponentPrimarySkill, OpponentSecondarySkill;

        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private ITimerManager _timerManager;

        private ISoundManager _soundManager;

        private VfxController _vfxController;

        private BattleController _battleController;

        private ActionsQueueController _actionsQueueController;

        private CardsController _cardsController;

        private BoardSkill _playerPrimarySkill, _playerSecondarySkill;

        private bool _skillsInitialized;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _vfxController = _gameplayManager.GetController<VfxController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _cardsController = _gameplayManager.GetController<CardsController>();

            _gameplayManager.OnGameEndedEvent += _gameplayManager_OnGameEndedEvent;
        }

        public void Update()
        {
            if (_skillsInitialized)
            {
                _playerPrimarySkill.Update();
                _playerSecondarySkill.Update();
                OpponentPrimarySkill.Update();
                OpponentSecondarySkill.Update();
            }
        }

        public void ResetAll()
        {
        }

        public void InitializeSkills()
        {
            GameplayPage rootPage = _uiManager.GetPage<GameplayPage>();

            rootPage.PlayerPrimarySkillHandler.OnMouseDownEvent += PrimarySkillHandlerOnMouseDownEventHandler;
            rootPage.PlayerPrimarySkillHandler.OnMouseUpEvent += PrimarySkillHandlerOnMouseUpEventHandler;

            rootPage.PlayerSecondarySkillHandler.OnMouseDownEvent += SecondarySkillHandlerOnMouseDownEventHandler;
            rootPage.PlayerSecondarySkillHandler.OnMouseUpEvent += SecondarySkillHandlerOnMouseUpEventHandler;

            rootPage.OpponentPrimarySkillHandler.OnMouseDownEvent += OpponentPrimarySkillHandlerOnMouseDownEventHandler;
            rootPage.OpponentPrimarySkillHandler.OnMouseUpEvent += OpponentPrimarySkillHandlerOnMouseUpEventHandler;

            rootPage.OpponentSecondarySkillHandler.OnMouseDownEvent += OpponentSecondarySkillHandlerOnMouseDownEventHandler;
            rootPage.OpponentSecondarySkillHandler.OnMouseUpEvent += OpponentSecondarySkillHandlerOnMouseUpEventHandler;

            int primary = _gameplayManager.CurrentPlayer.SelfHero.PrimarySkill;
            int secondary = _gameplayManager.CurrentPlayer.SelfHero.SecondarySkill;

            if ((primary < _gameplayManager.CurrentPlayer.SelfHero.Skills.Count) && (secondary < _gameplayManager.CurrentPlayer.SelfHero.Skills.Count))
            {
                SetPlayerSkills(rootPage, _gameplayManager.CurrentPlayer.SelfHero.Skills[primary], _gameplayManager.CurrentPlayer.SelfHero.Skills[secondary]);
            }

            primary = _gameplayManager.OpponentPlayer.SelfHero.PrimarySkill;
            secondary = _gameplayManager.OpponentPlayer.SelfHero.SecondarySkill;

            if ((primary < _gameplayManager.OpponentPlayer.SelfHero.Skills.Count) && (secondary < _gameplayManager.OpponentPlayer.SelfHero.Skills.Count))
            {
                SetOpponentSkills(rootPage, _gameplayManager.OpponentPlayer.SelfHero.Skills[primary], _gameplayManager.OpponentPlayer.SelfHero.Skills[secondary]);
            }

            _skillsInitialized = true;
        }

        public void DisableSkillsContent(Player player)
        {
            if (player.IsLocalPlayer)
            {
                _playerPrimarySkill.Hide();
                _playerSecondarySkill.Hide();
            }
            else
            {
                OpponentPrimarySkill.Hide();
                OpponentSecondarySkill.Hide();
            }
        }

        public void BlockSkill(Player player, Enumerators.SkillType type)
        {
            if (player.IsLocalPlayer)
            {
                _playerPrimarySkill.BlockSkill();
                _playerSecondarySkill.BlockSkill();
            }
            else
            {
                OpponentPrimarySkill.BlockSkill();
                OpponentSecondarySkill.BlockSkill();
            }
        }

        public void SetPlayerSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            _playerPrimarySkill = new BoardSkill(rootPage.PlayerPrimarySkillHandler.gameObject, _gameplayManager.CurrentPlayer, primary, true);
            _playerSecondarySkill = new BoardSkill(rootPage.PlayerSecondarySkillHandler.gameObject, _gameplayManager.CurrentPlayer, secondary, false);
        }

        public void SetOpponentSkills(GameplayPage rootPage, HeroSkill primary, HeroSkill secondary)
        {
            OpponentPrimarySkill = new BoardSkill(rootPage.OpponentPrimarySkillHandler.gameObject, _gameplayManager.OpponentPlayer, primary, true);
            OpponentSecondarySkill = new BoardSkill(rootPage.OpponentSecondarySkillHandler.gameObject, _gameplayManager.OpponentPlayer, secondary, false);
        }

        public void DoSkillAction(BoardSkill skill, object target = null)
        {
            if (skill == null)

                return;

            if (skill.IsUsing)
            {
                if (skill.FightTargetingArrow != null)
                {
                    if (skill.FightTargetingArrow.SelectedPlayer != null)
                    {
                        Player targetPlayer = skill.FightTargetingArrow.SelectedPlayer;

                        _vfxController.CreateSkillVfx(
                            GetVfxPrefabBySkill(skill),
                            skill.SelfObject.transform.position,
                            targetPlayer,
                            x =>
                            {
                                skill.UseSkill(targetPlayer);
                                DoActionByType(skill, targetPlayer);
                                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.UseAbility);
                            });
                    }
                    else if (skill.FightTargetingArrow.SelectedCard != null)
                    {
                        BoardUnit targetUnit = skill.FightTargetingArrow.SelectedCard;

                        _vfxController.CreateSkillVfx(
                            GetVfxPrefabBySkill(skill),
                            skill.SelfObject.transform.position,
                            targetUnit,
                            x =>
                            {
                                DoActionByType(skill, targetUnit);
                                skill.UseSkill(targetUnit);
                                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.UseAbility);
                            });
                    }

                    skill.CancelTargetingArrows();
                    skill.FightTargetingArrow = null;
                }
                else if (target != null)
                {
                    _vfxController.CreateSkillVfx(
                        GetVfxPrefabBySkill(skill),
                        skill.SelfObject.transform.position,
                        target,
                        x =>
                        {
                            DoActionByType(skill, target);
                            skill.UseSkill(target);
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.UseAbility);
                        });
                }
            }
        }

        private void _gameplayManager_OnGameEndedEvent(Enumerators.EndGameType obj)
        {
            _skillsInitialized = false;
        }

        private void PrimarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (_playerPrimarySkill != null)
            {
                _playerPrimarySkill.OnMouseDownEventHandler();
            }
        }

        private void PrimarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerPrimarySkill != null)
            {
                _playerPrimarySkill.OnMouseUpEventHandler();
            }
        }

        private void SecondarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
            {
                _playerSecondarySkill.OnMouseDownEventHandler();
            }
        }

        private void SecondarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (_playerSecondarySkill != null)
            {
                _playerSecondarySkill.OnMouseUpEventHandler();
            }
        }

        private void OpponentPrimarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (OpponentPrimarySkill != null)
            {
                OpponentPrimarySkill.OnMouseDownEventHandler();
            }
        }

        private void OpponentPrimarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (OpponentPrimarySkill != null)
            {
                OpponentPrimarySkill.OnMouseUpEventHandler();
            }
        }

        private void OpponentSecondarySkillHandlerOnMouseDownEventHandler(GameObject obj)
        {
            if (OpponentSecondarySkill != null)
            {
                OpponentSecondarySkill.OnMouseDownEventHandler();
            }
        }

        private void OpponentSecondarySkillHandlerOnMouseUpEventHandler(GameObject obj)
        {
            if (OpponentSecondarySkill != null)
            {
                OpponentSecondarySkill.OnMouseUpEventHandler();
            }
        }

        private void SkillParticleActionCompleted(object target)
        {
            // switch (skillType)
            // {
            // case Enumerators.SetType.WATER:
            // FreezeAction(target);
            // break;
            // case Enumerators.SetType.TOXIC:
            // ToxicDamageAction(target);
            // break;
            // case Enumerators.SetType.FIRE:
            // FireDamageAction(target);
            // break;
            // case Enumerators.SetType.LIFE:
            // HealAnyAction(target);
            // break;
            // case Enumerators.SetType.AIR:
            // //   CardReturnAction(target);
            // break;
            // default:
            // break;
            // }
        }

        private GameObject GetVfxPrefabBySkill(BoardSkill skill)
        {
            GameObject prefab = null;

            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.IceBolt:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");
                    break;
                case Enumerators.OverlordSkill.Freeze:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX");
                    break;
                case Enumerators.OverlordSkill.PoisonDart:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
                    break;
                case Enumerators.OverlordSkill.Fireball:
                case Enumerators.OverlordSkill.FireBolt:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX");
                    break;
                case Enumerators.OverlordSkill.HealingTouch:
                    prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX");
                    break;
                case Enumerators.OverlordSkill.ToxicPower:
                case Enumerators.OverlordSkill.Mend:
                case Enumerators.OverlordSkill.Harden:
                case Enumerators.OverlordSkill.StoneSkin:
                case Enumerators.OverlordSkill.Push:
                case Enumerators.OverlordSkill.Draw:
                default:
                    prefab = new GameObject();
                    break;
            }

            return prefab;
        }

        private void DoActionByType(BoardSkill skill, object target)
        {
            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.Freeze:
                    FreezeAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.IceBolt:
                    IceBoltAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.PoisonDart:
                    PoisonDartAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.ToxicPower:
                    ToxicPowerAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.HealingTouch:
                    HealingTouchAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.Mend:
                    MendAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.FireBolt:
                    FireBoltAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.Rabies:
                    RabiesAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.Harden:
                    HardenAction(skill.Owner, skill, skill.Skill);
                    break;
                case Enumerators.OverlordSkill.StoneSkin:
                    StoneskinAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.Push:
                    PushAction(skill.Owner, skill, skill.Skill, target);
                    break;
                case Enumerators.OverlordSkill.Draw:
                    DrawAction(skill.Owner, skill, skill.Skill, target);
                    break;
            }
        }

        #region actions

        private void FreezeAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;
                unit.Stun(Enumerators.StunType.Freeze, skill.Value);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FreezeVFX"), unit);

                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.StunUnitBySkill, new object[] { owner, unit }));
            }
            else if (target is Player)
            {
                Player player = target as Player;

                player.Stun(Enumerators.StunType.Freeze, skill.Value);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/Freeze_ImpactVFX"), player);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower() + "_Impact", Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.StunPlayerBySkill, new object[] { owner, player }));
            }
        }

        private void PoisonDartAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.Toxic, Enumerators.SetType.Life);
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX"), target);
            _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower() + "_Impact", Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
        }

        private void FireBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, boardSkill, skill, target, Enumerators.SetType.Fire, Enumerators.SetType.Toxic);
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/FireBoltVFX"), target);
            _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
        }

        private void HealingTouchAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if (target is Player)
            {
                Player player = target as Player;

                _battleController.HealPlayerBySkill(owner, skill, player);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), player);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
            }
            else
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.HealUnitBySkill(owner, skill, unit);

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
            }
        }

        private void HardenAction(Player owner, BoardSkill boardSkill, HeroSkill skill)
        {
            _battleController.HealPlayerBySkill(owner, skill, owner);

            // TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position -= Vector3.up * 3.3f;

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform);
        }

        private void AttackWithModifiers(
            Player owner,
            BoardSkill boardSkill,
            HeroSkill skill,
            object target,
            Enumerators.SetType attackType,
            Enumerators.SetType setType)
        {
            if (target is Player)
            {
                Player player = target as Player;

                // TODO additional damage to heros
                _battleController.AttackPlayerBySkill(owner, skill, player);
            }
            else
            {
                BoardUnit creature = target as BoardUnit;
                int attackModifier = 0;

                // if (creature.Card.libraryCard.cardSetType == setType)
                // attackModifier = 1;
                _battleController.AttackUnitBySkill(owner, skill, creature, attackModifier);
            }
        }

        private void PushAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            BoardUnit targetUnit = target as BoardUnit;
            Player unitOwner = targetUnit.OwnerPlayer;
            WorkingCard returningCard = targetUnit.Card;

            returningCard.InitialCost = returningCard.LibraryCard.Cost;
            returningCard.RealCost = returningCard.InitialCost;

            Vector3 unitPosition = targetUnit.Transform.position;

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX"), targetUnit);

            _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);

            _timerManager.AddTimer(
                x =>
                {
                    // STEP 1 - REMOVE UNIT FROM BOARD
                    unitOwner.BoardCards.Remove(targetUnit);

                    // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE;
                    targetUnit.Die(true);
                    Object.Destroy(targetUnit.GameObject);

                    // STEP 3 - REMOVE WORKING CARD FROM BOARD
                    unitOwner.RemoveCardFromBoard(returningCard);

                    // STEP 4 - RETURN CARD TO HAND
                    _cardsController.ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                    // STEP 4 - REARRANGE HANDS
                    _gameplayManager.RearrangeHands();

                    _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ReturnToHandCardSkill, new object[] { owner, skill, targetUnit }));

                    // _gameplayManager.GetController<RanksController>().UpdateRanksBuffs(unitOwner);
                },
                null,
                2f);
        }

        private void DrawAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            _cardsController.AddCardToHand(owner);

            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/DrawCardVFX"), owner);
            _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.DrawCardSkill, new object[] { owner, skill }));
        }

        private void StoneskinAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if ((target != null) && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.BuffedHp += skill.Value;
                unit.CurrentHp += skill.Value;

                // TODO: remove this empty gameobject logic
                Transform transform = new GameObject().transform;
                transform.position = unit.Transform.position;
                transform.position -= Vector3.up * 3.3f;

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/StoneskinVFX"), transform);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
            }
        }

        private void RabiesAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if ((target != null) && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                unit.SetAsFeralUnit();

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/RabiesVFX"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
            }
        }

        private void ToxicPowerAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if ((target != null) && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, skill, unit, 0);

                unit.BuffedDamage += skill.Attack;
                unit.CurrentDamage += skill.Attack;

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/ToxicAttackVFX"), unit);
            }
        }

        private void IceBoltAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            if ((target != null) && target is BoardUnit)
            {
                BoardUnit unit = target as BoardUnit;

                _battleController.AttackUnitBySkill(owner, skill, unit, 0);

                if (unit.CurrentHp > 0)
                {
                    unit.Stun(Enumerators.StunType.Freeze, 1);
                }

                _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBolt_Impact"), unit);
                _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
            }
        }

        private void MendAction(Player owner, BoardSkill boardSkill, HeroSkill skill, object target)
        {
            owner.Hp = Mathf.Clamp(owner.Hp + skill.Value, 0, owner.MaxCurrentHp);

            // TODO: remove this empty gameobject logic
            Transform transform = new GameObject().transform;
            transform.position = owner.AvatarObject.transform.position;
            transform.position += Vector3.up * 2;
            _vfxController.CreateVfx(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/MendVFX"), transform);
            _soundManager.PlaySound(Enumerators.SoundType.OverlordAbilities, skill.Title.Trim().ToLower(), Constants.OverlordAbilitySoundVolume, Enumerators.CardSoundType.None);
        }

        #endregion
    }
}
