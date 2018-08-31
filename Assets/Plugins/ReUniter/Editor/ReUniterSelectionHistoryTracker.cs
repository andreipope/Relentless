using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reuniter
{
    [InitializeOnLoad]
    class ReUniterSelectionHistoryTracker
    {
        private static int frameCounter;
        public static List<Object[]> PreviousSelections = new List<Object[]>();
        private const int MAX_HISTORY_COUNT = 100;

        static ReUniterSelectionHistoryTracker()
        {
            if (File.Exists(DataFilePath()))
                LoadPreviousSelections();
            EditorApplication.update += TrackSelectionChange;
        }

        private static void LoadPreviousSelections()
        {
            using (var stream = new BinaryReader(new FileStream(DataFilePath(), FileMode.Open)))
            {
                var selCount = stream.ReadInt32();
                for (var i = 0; i < selCount; i++)
                {
                    var objCount = stream.ReadInt32();
                    var objs = new Object[objCount];
                    for (var j = 0; j < objCount; j++)
                    {
                        var instanceId = stream.ReadInt32();
                        objs[j] = EditorUtility.InstanceIDToObject(instanceId);
                    }
                    PreviousSelections.Add(objs);
                }
            }
        }

        private static void SavePreviousSelections()
        {
            using (var stream = new BinaryWriter(new FileStream(DataFilePath(), File.Exists(DataFilePath())? FileMode.Truncate : FileMode.CreateNew)))
            {
                stream.Write(PreviousSelections.Count);
                PreviousSelections._Each(x =>
                {
                    stream.Write(x.Count(y => y != null));
                    x.Where(y => y != null)._Each(y => stream.Write(y.GetInstanceID()));
                });
            }
        }

        private static string DataFilePath()
        {
            return Application.dataPath + "/.reuniter_data";
        }

        static void TrackSelectionChange()
        {
            frameCounter++;
            if (frameCounter%10 != 0) return;
            if (Selection.objects.Any() &&
                (PreviousSelections.Count == 0 || !AreSelectionsEqual(PreviousSelections[0], Selection.objects)))
            {
                var existingIndex = PreviousSelections.FindIndex(x => AreSelectionsEqual(x, Selection.objects));
                if (existingIndex != -1)
                {
                    PreviousSelections.RemoveAt(existingIndex);
                }
                RecordSelection();
                if (PreviousSelections.Count > MAX_HISTORY_COUNT)
                {
                    PreviousSelections.RemoveRange(MAX_HISTORY_COUNT, PreviousSelections.Count-MAX_HISTORY_COUNT);
                }
                SavePreviousSelections();
            }
        }

        private static void RecordSelection()
        {
            var selectionCopy = new Object[Selection.objects.Length];
            Array.Copy(Selection.objects, selectionCopy, selectionCopy.Length);
            PreviousSelections.Insert(0, selectionCopy);
        }

        public static bool AreSelectionsEqual(Object[] sel1, Object[] sel2)
        {
            if (sel1.Length != sel2.Length)
                return false;
            for (var i = 0; i < sel1.Length; i++)
            {
                if (sel1[i]== null || sel2[i]==null || sel1[i].GetInstanceID() != sel2[i].GetInstanceID())
                    return false;
            }
            return true;
        }
    }
}