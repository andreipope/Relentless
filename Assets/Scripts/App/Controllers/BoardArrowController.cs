// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using UnityEngine;

namespace LoomNetwork.CZB
{
    public class BoardArrowController : IController
    {
        public BoardArrow CurrentBoardArrow { get; set; }

        public bool IsBoardArrowNowInTheBattle { get; set; }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void BeginArrow(Vector3 startPosition)
        {
        }

        public void EndArrow()
        {
        }

        public void SetStatusOfBoardArrowOnBoard(bool status)
        {
            IsBoardArrowNowInTheBattle = status;
        }

        public void ResetCurrentBoardArrow()
        {
            if (CurrentBoardArrow != null)
            {
                CurrentBoardArrow.Dispose();
                CurrentBoardArrow = null;
            }
        }

        public void CreateArrowFromTo(Vector3 from, Vector3 to, object target = null)
        {
        }
    }
}
