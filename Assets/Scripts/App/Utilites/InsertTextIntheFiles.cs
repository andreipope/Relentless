// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InsertTextIntheFiles : MonoBehaviour {

   // [MenuItem("Utilites/AddCopyright")]
    public static void AddText()
    {
       string text =  File.ReadAllText(Application.dataPath + "/Resources/LoomCopyright.txt");

        DoInTheFolders(text, Application.dataPath + "/Scripts");
    }


    private static void DoInTheFolders(string text, string directory)
    {
        var rootDir = new DirectoryInfo(directory);
        foreach(var dir in rootDir.GetDirectories())
        {
            DoInTheFolders(text, dir.FullName);
        }

        foreach(var file in rootDir.GetFiles())
        {
            if(file.Name.EndsWith(".cs"))
            {
                InsertTextInFile(file, text);
            }
        }
    }

    private static void InsertTextInFile(FileInfo file, string text)
    {
       var fileContent = File.ReadAllText(file.FullName);

        fileContent = text + Environment.NewLine + Environment.NewLine + fileContent;

        File.WriteAllText(file.FullName, fileContent);
    }
}
