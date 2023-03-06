using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BuildCounter : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;


    //Used to get the specific build number for each type
    public void OnPreprocessBuild(BuildReport report)
    {
        BuildScriptableObject BSO = ScriptableObject.CreateInstance<BuildScriptableObject>();

        switch (report.summary.platform)
        {
            case BuildTarget.iOS:

                break;
            case BuildTarget.Android:
                PlayerSettings.Android.bundleVersionCode++;
               BSO.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
                break;
            default:
                Debug.LogWarning($"Not set up for {report.summary.platform} yet!");
                break;
        }

        //The saving, deleting, and creation of new and old build number data
        AssetDatabase.DeleteAsset("Assets/Resources/Build.asset"); //Delete old build assets
        AssetDatabase.CreateAsset(BSO, "Assets/Resources/Build.asset"); //Create a new build asset
        AssetDatabase.SaveAssets();

        //Write the build number to a .txt file
        string parent = Directory.GetParent(Application.dataPath).FullName;
        string path = parent + @"\Builds\BuildNumber.txt";
        
        if (!File.Exists(path))
        {   
            using (StreamWriter sw = File.CreateText(path))
            {

            }
        }

        //reset
        File.WriteAllText(path, "");

        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine($"{Application.version}.{BSO.buildNumber}");
            sw.Close();
        }
    }


    //Used for builds outside of android
    private string IncrementBuildNumber(string buildNumber)
    {
        int.TryParse(buildNumber, out int outputBuildNumber);

        return (outputBuildNumber + 1).ToString();
    }

    public class PostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPostprocessBuild(BuildReport report)
        {
            string parent = Directory.GetParent(Application.dataPath).FullName;
            string path = parent + @"\Builds\GoogleUploader.exe";

            if (File.Exists(path)) Process.Start(path, "-Silent");
            else Debug.LogWarning("Unable to find the exe for apk file uploading make sure you have the path right");
        }
    }
}
