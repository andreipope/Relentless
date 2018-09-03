using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class ContentManager : IService, IContentManager
    {
        private ILocalizationManager _localizationManager;

        public List<SpreadsheetModelInfo> TutorialInfo { get; private set; }
        public List<SpreadsheetModelInfo> FlavorTextInfo { get; private set; }

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
            FillFlavorTextInfo();
        }

        private void FillTutorialInfo()
        {
            TutorialInfo = new List<SpreadsheetModelInfo>();

            ArrayList list = GetDataFromDb<SpreadsheetModelInfo>(Enumerators.SpreadsheetType.TUTORIAL);

            foreach (SpreadsheetModelInfo item in list)
            {
                item.Description = InternalTools.ReplaceLineBreaks(item.Description);
                TutorialInfo.Add(item);
            }
        }

        private void FillFlavorTextInfo()
        {
            FlavorTextInfo = new List<SpreadsheetModelInfo>();

            ArrayList list = GetDataFromDb<SpreadsheetModelInfo>(Enumerators.SpreadsheetType.FLAVOR_TEXT);

            foreach (SpreadsheetModelInfo item in list)
            {
                item.Description = InternalTools.ReplaceLineBreaks(item.Description);
                FlavorTextInfo.Add(item);
            }
        }


    }

    public class SpreadsheetModelInfo
    {
        public string Description;
    }
}
