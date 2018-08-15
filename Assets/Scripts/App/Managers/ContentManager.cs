// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ContentManager : IService, IContentManager
    {
        public List<SpreadsheetModelTutorialInfo> TutorialInfo
        {
            get { return _tutorialInfo; }
        }

        private static string TUTORIAL_LOCALIZATION_PATH = "https://docs.google.com/spreadsheets/d/1c6dQRpXM-mwT9NUsKKCp6XwE2OmIz9z1papZguXF--4/edit?usp=sharing";

        private ILocalizationManager _localizationManager;

        private List<SpreadsheetModelTutorialInfo> _tutorialInfo;

        public void Init()
        {
            _localizationManager = GameClient.Get<ILocalizationManager>();

            UpdateContentData();
        }

        public void Update()
        {
        }

        private void UpdateContentData()
        {
            FillTutorialInfo();
        }

        private void FillTutorialInfo()
        {
            _tutorialInfo = new List<SpreadsheetModelTutorialInfo>();

            var list = GetDataFromDB<SpreadsheetModelTutorialInfo>(Enumerators.SpreadsheetType.TUTORIAL);

            foreach (SpreadsheetModelTutorialInfo item in list)
            {
                item.Description = InternalTools.ReplaceLineBreaks(item.Description);
                _tutorialInfo.Add(item);
            }
        }

        public ArrayList GetDataFromDB<T>(Enumerators.SpreadsheetType type, bool removeLatestLine = true)
        {

            string path = Constants.CONTENT_FOLDER_NAME + _localizationManager.CurrentLanguage + "_" + type.ToString() + Constants.SPREADSHEET_FILE_FORMAT;

            CSVMap map = new CSVMap(typeof(T));
            ArrayList list;

            list = map.loadCsvFromFile(path.Replace(Constants.SPREADSHEET_FILE_FORMAT, string.Empty));

            if (removeLatestLine)
                list.RemoveAt(list.Count - 1);

            return list;
        }

        public void Dispose()
        {

        }
    }


    public class SpreadsheetModelTutorialInfo
    {
        public string Description;
    }
}
