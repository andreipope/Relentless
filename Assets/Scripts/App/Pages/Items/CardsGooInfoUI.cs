
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class CardGooInfoUI
    {
        private Dictionary<int, GooGraph> _gooGraphs;

        public void Load(GameObject obj)
        {
            _gooGraphs = new Dictionary<int, GooGraph>();

            GameObject graphList = obj.transform.Find("Graph_List").gameObject;
            for (int i = 0; i < 11; i++)
            {
                GameObject gooGraphUi = graphList.transform.Find("Graph_bg_" + i).gameObject;

                GooGraph gooGraph = new GooGraph(gooGraphUi);
                gooGraph.SetGooCost(i);
                _gooGraphs.Add(i, gooGraph);
            }
        }

        public void SetGooMeter(List<DeckCardData> cards)
        {
            Dictionary<int, int> cardsGoo = new Dictionary<int, int>();
            for (int i = 0; i < 11; i++)
            {
                cardsGoo.Add(i, 0);
            }

            for (int i = 0; i < cards.Count; i++)
            {
                int iTmp = i;
                int index = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards
                    .FindIndex(card => card.CardKey.Equals(cards[iTmp].CardKey));
                int gooCost = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards[index].Cost;

                if (gooCost > 10)
                {
                    cardsGoo[10] += cards[i].Amount;
                }
                else
                {
                    cardsGoo[gooCost] += cards[i].Amount;
                }

            }

            for (int i = 0; i < _gooGraphs.Count; i++)
            {
                float value = (float)cardsGoo[i] / cards.Count;
                _gooGraphs[i].SetMeterValue(value);
            }
        }
    }

    public class GooGraph
    {
        private Image _graphMeterImage;
        private TextMeshProUGUI _gooCost;

        public GooGraph(GameObject obj)
        {
            _graphMeterImage = obj.transform.Find("Graph").GetComponent<Image>();
            _gooCost = obj.transform.Find("Goo_Bottle/GooCost").GetComponent<TextMeshProUGUI>();
        }

        public void SetGooCost(int cost)
        {
            string finalCost = cost.ToString();
            if (cost >= 10)
                finalCost = cost + "+";
            _gooCost.text = finalCost;
        }

        public void SetMeterValue(float value)
        {
            _graphMeterImage.fillAmount = value;
        }
    }
}
