using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/*
	jsonc����scripatable object�ϊ��pwindow�Ăяo��
*/
public class J2SWindow : EditorWindow
{
	const string DstDir = @"Assets/Sample/Dst";  // �o��Unity�p�X
	const string SrcDir = @"Sample\Src\";    // ���f�[�^�p�X

	[MenuItem("Window/Json 2 ScriptableObject Editor")]
	static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		var window = (J2SWindow)EditorWindow.GetWindow(typeof(J2SWindow));
		window.Show();
	}

	static private void SelectFolder(string path)
	{
		if (path[path.Length - 1] == '/')
			path = path.Substring(0, path.Length - 1);

		// Load object
		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

		// Select the object in the project folder
		Selection.activeObject = obj;

		// Also flash the folder yellow to highlight it
		EditorGUIUtility.PingObject(obj);
	}



	void OnGUI()
	{
		var dirs = Directory.GetDirectories(Path.Combine(Application.dataPath, SrcDir), "*", SearchOption.TopDirectoryOnly);

		GUILayout.Label("*.jsonc��scriptableobject�ɕϊ����܂�");

		foreach (var item in dirs)
		{
			var dirName = Path.GetFileName(item);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(dirName))
			{
				Json2ScriptableUtil.Convert(item, Path.Combine(DstDir, dirName), false);
			}
			if (GUILayout.Button($"{dirName}(�������s)"))
			{
				Json2ScriptableUtil.Convert(item, Path.Combine(DstDir, dirName), true);
			}
			if (GUILayout.Button($"�I��"))
			{
				SelectFolder(Path.Combine(DstDir, dirName));
			}
			GUILayout.EndHorizontal();
		}
	}

}
