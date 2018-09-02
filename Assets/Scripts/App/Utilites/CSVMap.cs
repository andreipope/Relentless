using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LoomNetwork.CZB.Helpers
{
    public class CSVMap
    {
        /*
         * 
         * Quick Usage Guide:
         * 
         * CSVMap allows you to map a CSV file to a class in Unity.  This is perfect for monster lists, skill lists, or item lists
        Usage is simple.  Make a new CSVMap object like this:
        CSVMap myMap = new CSVMap();

        then you can make a class to define how your data is saved, here is an example of an item class:

        public class item {
            public int itemId;
            public string itemName;
            public string itemIcon;
            public string[] useEffects;
        }

        Now you can make a CSV file with the same header names as the field names of the class.   
        They must be the same!
        Like this:

        itemId,itemName,itemIcon,useEffects
        1,Bens Magical Potion,POTION_ICON,POTION_HEAL|POTION_GROW|POTION_MAGIC_IMMUNE
        2,Bens Poison Potion, POTION_ICON_POISON, POTION_POISON

        To use CSVMap to transform your CSV data into an ArrayList of your items simply do this:

        CSVMap myMap = new CSVMap();
        myMap.defineColumns(typeof(ItemClass));
        ArrayList itemList = myMap.loadCsvFromFile("items"); //remember not to use an extension for embedded Unity resources.

        and thats it!  Now itemList will be filled with all of the data from your CSV file.
        Remember to put the CSV file in your Resources folder in your project root.  
        If you don't have a resources folder you need to make one.

        A few tips:

        Vector3's are serialized in the CSV with colons.  
        Example:  1:2:3 in a csv field is the same as new Vector3(1.0f,2.0f,3.0f);

        String lists are serialized with a pipe: |
        Example: apples|bananas|carrots makes a string[] with 3 elements.

        Currently the only array type that is supported is a string array.
        */
        private Hashtable columnMap = new Hashtable();

        private Type ClassTemplate;

        public CSVMap()
        {
        }

        /// <summary>
        ///     Shortcut to defineColumns from constructor
        /// </summary>
        /// <param name="classDefinition">Class definition.</param>
        public CSVMap(Type classDefinition)
        {
            defineColumns(classDefinition);
        }

        /// <summary>
        ///     Define Columns - call this first, with the typeof of the class you want to map to.  Example:
        ///     CSVMap myMap = new CSVMap();
        ///     myMap.defineColumns(typeof(MYCLASS));
        ///     This will analyze your class and save the information for later.
        /// </summary>
        /// <param name="classDefinition">Class definition.</param>
        public void defineColumns(Type classDefinition)
        {
            ClassTemplate = classDefinition;
            columnMap = new Hashtable();
            MemberInfo[] members = classDefinition.GetFields();
            foreach (FieldInfo m in members)
            {
                columnMap[m.Name] = m;
            }
        }

        /// <summary>
        ///     Use this after calling defineColumns to load your CSV data (if it is not coming from a file).
        ///     The data can easily come from a web service call over the internet, a website, a file, or elsewhere.
        /// </summary>
        /// <returns>The csv from string.</returns>
        /// <param name="data">Data.</param>
        public ArrayList loadCsvFromString(string data)
        {
            string[] lines = data.Split('\n');
            int ctr = 0;
            ArrayList rows = new ArrayList();
            ArrayList columns = new ArrayList();
            foreach (string line in lines)
            {
                // if (fix_break)
                // 		continue;
                Regex csvread = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                string[] c = csvread.Split(line);

                for (int i = 0; i < c.Length; i++)
                {
                    c[i] = c[i].TrimStart(' ', '"');
                    c[i] = c[i].TrimEnd('"');
                }

                if (ctr == 0)
                {
                    foreach (string colName in c)
                    {
                        columns.Add(colName.Trim("\n\r ".ToCharArray())); // TODO remove trim
                    }
                } else
                {
                    object templated = Activator.CreateInstance(ClassTemplate);
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (i > columnMap.Count - 1)
                        {
                            continue;
                        }

                        FieldInfo templateInfo = (FieldInfo)columnMap[columns[i]];

                        // Debug.Log("------- " + templateInfo);
                        // 	Debug.Log (templateInfo + " " + columns [i] + " " + columnMap.Count);
                        if (templateInfo == null)
                        {
                            Debug.LogError("CSV Field Not Found In ClassTemplate: " + columns[i] + "  length: " + columns[i].ToString().Length + "  in " + ClassTemplate);
                        }

                        Type colType = templateInfo.FieldType;

                        if ((colType != null) && (c[i] != null) && (c[i].Length > 0))
                        {
                            if (colType == typeof(Vector3))
                            {
                                Vector3 useVector = new Vector3(float.Parse(c[i].Split(':')[0]), float.Parse(c[i].Split(':')[1]), float.Parse(c[i].Split(':')[2]));
                                templateInfo.SetValue(templated, useVector);
                            } else if (colType == typeof(string[]))
                            {
                                string[] useList = c[i].Split('|');
                                templateInfo.SetValue(templated, useList);
                            } else
                            {
                                // Debug.Log("------- " + c[i]);
                                templateInfo.SetValue(templated, Convert.ChangeType(c[i], colType));
                            }
                        }
                    }

                    rows.Add(templated);
                }

                ctr++;
            }

            return rows;
        }

        /// <summary>
        ///     loadCsvFromFile will load data directly from your Resources folder of your Unity project.  Perfect for embedded
        ///     game data.
        ///     Remember not to put an extension on your filename.  Example:  "items.csv" just becomes "items".
        ///     loadCsvFromFile("items");
        /// </summary>
        /// <returns>The csv from file.</returns>
        /// <param name="fileName">File name.</param>
        public ArrayList loadCsvFromFile(string fileName)
        {
            TextAsset textAsset = (TextAsset)Resources.Load(fileName, typeof(TextAsset));

            if (textAsset == null)
            {
                return null;
            }

            return loadCsvFromString(textAsset.text);
        }

        public ArrayList loadCsvFromPersistentPathFile(string fileName)
        {
            string text = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return loadCsvFromString(text);
        }
    }
}
