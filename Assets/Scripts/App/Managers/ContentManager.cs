// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Helpers;

namespace LoomNetwork.CZB
{
    public class ContentManager : IService, IContentManager
    {
        private static string TUTORIAL_LOCALIZATION_PATH = "https://docs.google.com/spreadsheets/d/1c6dQRpXM-mwT9NUsKKCp6XwE2OmIz9z1papZguXF--4/edit?usp=sharing";

        private ILocalizationManager _localizationManager;

        public List<SpreadsheetModelTutorialInfo> TutorialInfo { get; private set; }

        public void Init()
        {
            _localizationManager = GameClient.Get<ILocalizationManager>();

            UpdateContentData();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public ArrayList GetDataFromDB<T>(Enumerators.SpreadsheetType type, bool removeLatestLine = true)
        {
            string path = Constants.CONTENT_FOLDER_NAME + _localizationManager.CurrentLanguage + "_" + type + Constants.SPREADSHEET_FILE_FORMAT;

            CSVMap map = new CSVMap(typeof(T));
            ArrayList list;

            list = map.loadCsvFromFile(path.Replace(Constants.SPREADSHEET_FILE_FORMAT, string.Empty));

            if (removeLatestLine)
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private void UpdateContentData()
        {
            FillTutorialInfo();
        }

        private void FillTutorialInfo()
        {
            TutorialInfo = new List<SpreadsheetModelTutorialInfo>();

            ArrayList list = GetDataFromDB<SpreadsheetModelTutorialInfo>(Enumerators.SpreadsheetType.TUTORIAL);

            foreach (SpreadsheetModelTutorialInfo item in list)
            {
                item.Description = InternalTools.ReplaceLineBreaks(item.Description);
                TutorialInfo.Add(item);
            }
        }
    }

    public class SpreadsheetModelTutorialInfo
    {
        public string Description;
    }
}
