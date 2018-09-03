using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Helpers;

namespace LoomNetwork.CZB
{
    public class ContentManager : IService, IContentManager
    {
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

        public ArrayList GetDataFromDb<T>(Enumerators.SpreadsheetType type, bool removeLatestLine = true)
        {
            string path = Constants.ContentFolderName + _localizationManager.CurrentLanguage + "_" + type +
                Constants.SpreadsheetFileFormat;

            CsvMap map = new CsvMap(typeof(T));
            ArrayList list;

            list = map.LoadCsvFromFile(path.Replace(Constants.SpreadsheetFileFormat, string.Empty));

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

            ArrayList list = GetDataFromDb<SpreadsheetModelTutorialInfo>(Enumerators.SpreadsheetType.TUTORIAL);

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
