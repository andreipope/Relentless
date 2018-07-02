using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.CZB
{
    public class CardInGraveyard
    {
        public GameObject selfObject;
        public Image image;

        public CardInGraveyard(GameObject gameObject, Sprite sprite = null)
        {
            selfObject = gameObject;
            image = selfObject.transform.Find("Image").GetComponent<Image>();

            if (sprite != null)
                image.sprite = sprite;
        }

        public void Dispose()
        {
            if (selfObject != null)
                GameObject.Destroy(selfObject);
        }
    }
}