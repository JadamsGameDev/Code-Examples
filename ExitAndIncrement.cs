/*************************************************************************
 * 
 * MIXT CONFIDENTIAL
 * ________________
 * 
 *  [2016] - [2018] Mixt Ltd
 *  All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is, and remains
 * the property of Mixt Ltd and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to Mixt Ltd
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Mixt Ltd.
 */
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Backsup the project by creating a new copy on the disk in this format: "project name"."day"."month"."year"v"interger"
/// Example: NewUnityProject.31.1.17v1
/// 
/// note even works while the project is open
/// </summary>

public class ExitAndIncrement  {
	[MenuItem("File/Exit and Backup")]
	private static void ExitAndCopy()
	{
		Copy ();
		EditorApplication.Exit (0);
	}

	[MenuItem("File/Backup")]
	private static void Copy()
	{
		string[] s = Application.dataPath.Split('/');

		string projectName = s[s.Length - 2];
		
		string startDir = Application.dataPath;
		startDir = startDir.Replace ("/Assets", "");
		string date = DateTime.Now.Day + "." + DateTime.Now.Month + "." + (DateTime.Now.Year.ToString().Replace("20", ""));
		UnityEngine.Debug.Log (date);
		string newDir = startDir + date;
		int i = 0;
		while (Directory.Exists (newDir)) {
			i++;
			newDir = startDir + date + "v" + i.ToString ();
		}
		
		string batDir = startDir.Replace ("/" + projectName, "");
		List<string> folders = batDir.Split('/').ToList();
		string excludeFileDir = "";
		for(int f = 0; f < folders.Count(); f++) {
			if (folders[f].Contains (" ")) {
				string dosName;
				folders [f] = folders [f].Replace (" ", "");
				if (folders [f].Length < 6) {
					if (folders [f].Length < 3) {
						Console.WriteLine("Back up doesn't support folders with only 2 characters and a space");
						return;
					}
					dosName = folders [f];
				} else {
					dosName = folders [f].Substring (0, 6);
				}
				dosName = dosName.ToUpper () + "~1";
				folders [f] = dosName;
			}
			excludeFileDir += folders [f] + "\\";
		}
		excludeFileDir += "ExcludeFiles" + projectName + ".txt";
		string cMD = "xcopy \"@startDir\" \"@newDir\" @exclude/s/h/e/k/f/c/i";
		cMD = cMD.Replace ("@startDir", startDir);
		cMD = cMD.Replace ("@newDir", newDir);
		cMD = cMD.Replace ("@exclude", "/EXCLUDE:" + excludeFileDir);

		if (File.Exists (batDir + "/ExcludeFiles" + projectName + ".txt") == false) {
			StreamWriter esw = new StreamWriter (batDir + "/ExcludeFiles" + projectName + ".txt");
			esw.Close ();
		}
		File.Delete (batDir + "/save" + projectName + ".bat");
		StreamWriter sw = new StreamWriter (batDir + "/save"+ projectName +".bat");
		sw.WriteLine ("set gameName=" + projectName);
		sw.WriteLine (cMD);
		sw.WriteLine ("Pause");
		sw.Close ();
		Process.Start (batDir + "/save"+ projectName +".bat");

	}

	

}
