using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("textPressedPosition")]
    private Vector3 _textPressedPosition = new Vector3(0, -0.12f, 0);

    [SerializeField]
    [FormerlySerializedAs("textDefaultPosition")]
    private Vector3 _textDefaultPosition = new Vector3(0, -0.00f, 0);

    [SerializeField]
    [FormerlySerializedAs("defaultSprite")]
    private Sprite _defaultSprite;

    [SerializeField]
    [FormerlySerializedAs("pressedSprite")]
    private Sprite _pressedSprite;

    [SerializeField]
    [FormerlySerializedAs("buttonText")]
    private TextMeshPro _buttonText;

    [SerializeField] private GameObject _endButtonGlowObject;


    private IGameplayManager _gameplayManager;

    private bool _hovering;

    private bool _active;

    private bool _wasClicked;

    private SpriteRenderer _thisRenderer;

    public void SetEnabled(bool enabled)
    {
        _active = enabled;
        _buttonText.text = enabled ? "END\nTURN" : "\nWAIT";
        _thisRenderer.sprite = enabled ? _defaultSprite : _pressedSprite;
        _endButtonGlowObject.SetActive(enabled);
    }

    private void Awake()
    {
        Assert.IsNotNull(_defaultSprite);
        Assert.IsNotNull(_pressedSprite);
        _thisRenderer = GetComponent<SpriteRenderer>();

        _gameplayManager = GameClient.Get<IGameplayManager>();
    }

    private void OnMouseEnter()
    {
        _hovering = true;
    }

    private void OnMouseExit()
    {
        if (!_active)
            return;

        _hovering = false;
        _thisRenderer.sprite = _defaultSprite;
        _buttonText.transform.localPosition = _textDefaultPosition;
    }

    private void OnMouseDown()
    {
        if (_gameplayManager.IsGameplayInputBlocked ||
            !_active ||
            _gameplayManager.IsGameEnded /* ||
            _gameplayManager.GetController<AbilitiesController>().BlockEndTurnButton*/)
            return;

        _wasClicked = true;

        _thisRenderer.sprite = _pressedSprite;
        _buttonText.transform.localPosition = _textPressedPosition;
        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.END_TURN, 128,
            Constants.EndTurnClickSoundVolume, dropOldBackgroundMusic: false);
    }

    private void OnMouseUp()
    {
        if (!_wasClicked ||
           _gameplayManager.IsGameplayInputBlocked ||
           _gameplayManager.IsGameEnded ||
            (GameClient.Get<ITutorialManager>().IsTutorial &&
             (!GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToGameplayStep().CanEndTurn ||
             !GameClient.Get<ITutorialManager>().IsCompletedActivitiesForThisTurn())) /*||
             _gameplayManager.GetController<AbilitiesController>().BlockEndTurnButton*/)
        {
            GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.TapOnEndTurnButtonWhenItsLimited);
            return;
        }

        if (_active && _hovering)
        {
            _gameplayManager.GetController<BattlegroundController>().StopTurn();
            SetEnabled(false);
        }

        _buttonText.transform.localPosition = _textDefaultPosition;

        _wasClicked = false;
    }
}
