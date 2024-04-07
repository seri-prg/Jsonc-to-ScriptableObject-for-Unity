using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/*
	json → scriptable object 変換サポート

*/
public class Json2ScriptableUtil 
{
	// ソースパスから出力先パスを生成
	private static string GetDstPath(string srcPath, string dstDir)
	{
		var fileName = Path.GetFileNameWithoutExtension(srcPath) + ".asset";
		return Path.Combine(dstDir, fileName);
	}

	// 元のjsonファイルリスト取得
	private static string[] GetSrcJsonList(string basePath)
	{
		return Directory.GetFiles(basePath, "*.jsonc");
	}



	// 全てのjsonファイルに対して処理を実行
	// funcの戻り値がfalseの場合は中断
	private static void DoAllJson(string basePath, Func<string, string, bool> func)
	{
		if (func == null)
			return;

		var fileList = GetSrcJsonList(basePath);
		foreach (var jsonPath in fileList)
		{
			var text = File.ReadAllText(jsonPath);

			if (!func.Invoke(jsonPath, text))
				break;
		}
	}

	/// <summary>
	/// アセットを上書きで作成する(metaデータはそのまま)
	/// </summary>
	public static void CreateAssetWithOverwrite(UnityEngine.Object asset, string exportPath)
	{
		//アセットが存在しない場合はそのまま作成(metaファイルも新規作成)
		if (!File.Exists(exportPath))
		{
			// ディレクトリがないなら
			var dir = Path.GetDirectoryName(exportPath);
			if (!Directory.Exists(dir))
			{
				var dinfo = new DirectoryInfo(dir);
				var parentPath = dir.Substring(0, dir.Length - dinfo.Name.Length);
				if (parentPath.EndsWith("\\"))
				{
					parentPath = parentPath.Substring(0, parentPath.Length - 1);
				}

				AssetDatabase.CreateFolder(parentPath, dinfo.Name);
			}

			AssetDatabase.CreateAsset(asset, exportPath);
			return;
		}

		//仮ファイルを作るためのディレクトリを作成
		var fileName = Path.GetFileName(exportPath);
		var tmpDirectoryPath = Path.Combine(exportPath.Replace(fileName, ""), "tmpDirectory");
		Directory.CreateDirectory(tmpDirectoryPath);

		//仮ファイルを保存
		var tmpFilePath = Path.Combine(tmpDirectoryPath, fileName);
		AssetDatabase.CreateAsset(asset, tmpFilePath);

		//仮ファイルを既存のファイルに上書き(metaデータはそのまま)
		FileUtil.ReplaceFile(tmpFilePath, exportPath);

		//仮ディレクトリとファイルを削除
		AssetDatabase.DeleteAsset(tmpDirectoryPath);

		//データ変更をUnityに伝えるためインポートしなおし
		AssetDatabase.ImportAsset(exportPath);
	}

	// srcDirにあるjsonファイルを全てscriptableobjectにコンバート
	public static void Convert(string srcDir, string dstDir, bool forceConvert)
	{
		DoAllJson(srcDir, (string path, string text) =>
		{
			try
			{
				var dstAssetPath = GetDstPath(path, dstDir);

				// ハッシュキーを取得
				var newHash = Md5Util.GetHash(text);
				var importer = AssetImporter.GetAtPath(dstAssetPath);

				if (!forceConvert)
				{
					// 既に存在するならハッシュを比較。同じ場合は更新なし
					if ((importer != null) && (importer.userData == newHash))
						return true;
				}

				var obj = JsonUtil.CreateScriptableObject(text);
				CreateAssetWithOverwrite(obj, dstAssetPath);
				// AssetDatabase.CreateAsset(obj, dstAssetPath);

				if (importer == null)
				{
					importer = AssetImporter.GetAtPath(dstAssetPath);
				}

				importer.userData = newHash;
				importer.SaveAndReimport();
			}
			catch (Exception e)
			{
				Debug.LogError($"[{path}] error.\n{e.Message}");
			}

			return true;
		});

		Debug.Log("end import json data");
	}

}
