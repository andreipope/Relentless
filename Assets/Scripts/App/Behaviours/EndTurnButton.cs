using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("textPressedPosition")]
    private readonly Vector3 _textPressedPosition = new Vector3(0, -0.12f, 0);

    [SerializeField]
    [FormerlySerializedAs("textDefaultPosition")]
    private readonly Vector3 _textDefaultPosition = new Vector3(0, -0.00f, 0);

    [SerializeField]
    [FormerlySerializedAs("defaultSprite")]
    private Sprite _defaultSprite;

    [SerializeField]
    [FormerlySerializedAs("pressedSprite")]
    private Sprite _pressedSprite;

    [SerializeField]
    [FormerlySerializedAs("buttonText")]
    private TextMeshPro _buttonText;

    private bool _hovering;

    private bool _active;

    private SpriteRenderer _thisRenderer;

    public void SetEnabled(bool enabled)
    {
        _active = enabled;
        _buttonText.text = enabled ? "END\nTURN" : "\nWAIT";
        _thisRenderer.sprite = enabled ? _defaultSprite : _pressedSprite;
    }

    private void Awake()
    {
        Assert.IsNotNull(_defaultSprite);
        Assert.IsNotNull(_pressedSprite);
        _thisRenderer = GetComponent<SpriteRenderer>();
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
        if (!_active)
            return;

        _thisRenderer.sprite = _pressedSprite;
        _buttonText.transform.localPosition = _textPressedPosition;
        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.END_TURN, 128,
            Constants.EndTurnClickSoundVolume, dropOldBackgroundMusic: false);
    }

    // was OnMouseDown
    private void OnMouseUp()
    {
        if (GameClient.Get<ITutorialManager>().IsTutorial && GameClient.Get<ITutorialManager>().CurrentStep != 10 &&
            GameClient.Get<ITutorialManager>().CurrentStep != 16 &&
            GameClient.Get<ITutorialManager>().CurrentStep != 21)
            return;

        if (_active && _hovering)
        {
            GameClient.Get<IGameplayManager>().GetController<BattlegroundController>().StopTurn();
            SetEnabled(false);
        }

        _buttonText.transform.localPosition = _textDefaultPosition;
    }
}
