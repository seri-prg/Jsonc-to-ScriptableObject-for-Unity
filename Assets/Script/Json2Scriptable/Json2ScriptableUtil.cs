using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/*
	json �� scriptable object �ϊ��T�|�[�g

*/
public class Json2ScriptableUtil 
{
	// �\�[�X�p�X����o�͐�p�X�𐶐�
	private static string GetDstPath(string srcPath, string dstDir)
	{
		var fileName = Path.GetFileNameWithoutExtension(srcPath) + ".asset";
		return Path.Combine(dstDir, fileName);
	}

	// ����json�t�@�C�����X�g�擾
	private static string[] GetSrcJsonList(string basePath)
	{
		return Directory.GetFiles(basePath, "*.jsonc");
	}



	// �S�Ă�json�t�@�C���ɑ΂��ď��������s
	// func�̖߂�l��false�̏ꍇ�͒��f
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
	/// �A�Z�b�g���㏑���ō쐬����(meta�f�[�^�͂��̂܂�)
	/// </summary>
	public static void CreateAssetWithOverwrite(UnityEngine.Object asset, string exportPath)
	{
		//�A�Z�b�g�����݂��Ȃ��ꍇ�͂��̂܂܍쐬(meta�t�@�C�����V�K�쐬)
		if (!File.Exists(exportPath))
		{
			// �f�B���N�g�����Ȃ��Ȃ�
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

		//���t�@�C������邽�߂̃f�B���N�g�����쐬
		var fileName = Path.GetFileName(exportPath);
		var tmpDirectoryPath = Path.Combine(exportPath.Replace(fileName, ""), "tmpDirectory");
		Directory.CreateDirectory(tmpDirectoryPath);

		//���t�@�C����ۑ�
		var tmpFilePath = Path.Combine(tmpDirectoryPath, fileName);
		AssetDatabase.CreateAsset(asset, tmpFilePath);

		//���t�@�C���������̃t�@�C���ɏ㏑��(meta�f�[�^�͂��̂܂�)
		FileUtil.ReplaceFile(tmpFilePath, exportPath);

		//���f�B���N�g���ƃt�@�C�����폜
		AssetDatabase.DeleteAsset(tmpDirectoryPath);

		//�f�[�^�ύX��Unity�ɓ`���邽�߃C���|�[�g���Ȃ���
		AssetDatabase.ImportAsset(exportPath);
	}

	// srcDir�ɂ���json�t�@�C����S��scriptableobject�ɃR���o�[�g
	public static void Convert(string srcDir, string dstDir, bool forceConvert)
	{
		DoAllJson(srcDir, (string path, string text) =>
		{
			try
			{
				var dstAssetPath = GetDstPath(path, dstDir);

				// �n�b�V���L�[���擾
				var newHash = Md5Util.GetHash(text);
				var importer = AssetImporter.GetAtPath(dstAssetPath);

				if (!forceConvert)
				{
					// ���ɑ��݂���Ȃ�n�b�V�����r�B�����ꍇ�͍X�V�Ȃ�
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
