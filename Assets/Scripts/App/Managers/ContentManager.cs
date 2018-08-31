// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ContentManager : IService, IContentManager
    {
        public List<SpreadsheetModelInfo> TutorialInfo
        {
            get { return _tutorialInfo; }
        }

        public List<SpreadsheetModelInfo> FlavorTextInfo
        {
            get { return _flavorTextInfo; }
        }

        private static string TUTORIAL_LOCALIZATION_PATH = "https://docs.google.com/spreadsheets/d/1c6dQRpXM-mwT9NUsKKCp6XwE2OmIz9z1papZguXF--4/edit?usp=sharing";

        private ILocalizationManager _localizationManager;

        private List<SpreadsheetModelInfo> _tutorialInfo;
        private List<SpreadsheetModelInfo> _flavorTextInfo;

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
            FillFlavorTextInfo();
        }

        private void FillFlavorTextInfo()
        {
            _flavorTextInfo = new List<SpreadsheetModelInfo>();
            var list = GetDataFromDB<SpreadsheetModelInfo>(Enumerators.SpreadsheetType.FLAVOR_TEXT, false);
            foreach (SpreadsheetModelInfo item in list)
            {
                item.Description = InternalTools.ReplaceLineBreaks(item.Description);
                _flavorTextInfo.Add(item);
            }
        }

        private void FillTutorialInfo()
        {
            _tutorialInfo = new List<SpreadsheetModelInfo>();

            var list = GetDataFromDB<SpreadsheetModelInfo>(Enumerators.SpreadsheetType.TUTORIAL);

            foreach (SpreadsheetModelInfo item in list)
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
               
            Debug.Log(path);
            list = map.loadCsvFromFile(path.Replace(Constants.SPREADSHEET_FILE_FORMAT, string.Empty));

            if (removeLatestLine)
                list.RemoveAt(list.Count - 1);

            return list;
        }

        public void Dispose()
        {

        }
    }


    public class SpreadsheetModelInfo
    {
        public string Description;
    }
}
