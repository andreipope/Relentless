using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ActionsReportController : IController
    {
        public event Action<PastActionsPopup.PastActionParam> GotNewActionReportEvent;

        public List<PastActionsPopup.PastActionParam> ActionsReports { get; private set; }


        private List<PastActionsPopup.PastActionParam> _bufferActionsReports;

        public void Init()
        {
            ActionsReports = new List<PastActionsPopup.PastActionParam>();
            _bufferActionsReports = new List<PastActionsPopup.PastActionParam>();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            ActionsReports.Clear();
            _bufferActionsReports.Clear();
        }

        public void ResetAll()
        {
        }

        public void PostGameActionReport(PastActionsPopup.PastActionParam report)
        {
            if (report != null)
            {
                if (report.CheckForCardOwner && !ActionsReports.Exists(x => x.Model == report.Model))
                {
                    _bufferActionsReports.Add(report);
                }
                else
                {
                    AddNewPostGameActionReport(report, !report.CheckForCardOwner);
                }
            }
        }

        private void AddNewPostGameActionReport(PastActionsPopup.PastActionParam report, bool checkBuffer = false)
        {
            ActionsReports.Add(report);
            GotNewActionReportEvent?.Invoke(report);
            if(checkBuffer)
            {
                CheckReportsInBuffer(report);
            }
        }

        private void CheckReportsInBuffer(PastActionsPopup.PastActionParam report)
        {
            foreach (PastActionsPopup.PastActionParam sortingReport in _bufferActionsReports)
            {
                ActionsReports.Add(sortingReport);
                GotNewActionReportEvent?.Invoke(sortingReport);
            }
            _bufferActionsReports.Clear();
        }
    }
}
