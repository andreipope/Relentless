using UnityEngine;
using UnityEngine.UI;

public class FadeoutBars
{
    private Scrollbar _deckCardsScrollBar;
    private GameObject _leftFadeGameObject;
    private GameObject _rightFadeGameObject;

    public void Init(Scrollbar deckCardsScrollBar, GameObject leftFadeGameObject, GameObject rightFadeGameObject)
    {
        _deckCardsScrollBar = deckCardsScrollBar;
        _leftFadeGameObject = leftFadeGameObject;
        _rightFadeGameObject = rightFadeGameObject;
    }

    public void Update()
    {
        if (_leftFadeGameObject == null || _rightFadeGameObject == null)
            return;

        UpdateFade();
    }

    private void UpdateFade()
    {
        bool isScrollBarActive = _deckCardsScrollBar.gameObject.activeSelf;
        _leftFadeGameObject.SetActive(isScrollBarActive);
        _rightFadeGameObject.SetActive(isScrollBarActive);

        if (_deckCardsScrollBar.value <= 0)
        {
            _leftFadeGameObject.SetActive(false);
            _rightFadeGameObject.SetActive(true);
        }
        else if(_deckCardsScrollBar.value >= 0.9999f)
        {
            _leftFadeGameObject.SetActive(true);
            _rightFadeGameObject.SetActive(false);
        }
    }
}
