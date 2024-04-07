using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEngine;

/*
	json�f�[�^��class�p�����[�^����ScriptableObject�𐶐����A
	�f�[�^�̓��e��ݒ�B

	�f�[�^�̕ҏW��json�ōs�����Q�[������̃A�N�Z�X��ScriptableObject�ōs�������ꍇ�ɕ֗�
*/
public class JsonUtil 
{
	// json�f�[�^����ScriptableObject����
	public static ScriptableObject CreateScriptableObject(string jsonRootText)
	{
		jsonRootText = RemoveComment(jsonRootText);

		var jsonRoot = Json.Deserialize(jsonRootText) as Dictionary<string, object>;
		var rootType = GetObjectType(jsonRoot);

		var obj = ScriptableObject.CreateInstance(rootType);
		JsonUtil.CreateObject(jsonRoot, obj);
		return obj;
	}


	// Json����R�����g�폜(jsonc �� json)
	// �{���͕�������������疳�������Ȃ��Ƃ����Ȃ��B
	// //�`\n
	// /*�`*/
	// ��L���폜
	private static string RemoveComment(string jsonText)
	{
		var sb = new StringBuilder();
		var i = 0;
		while ((i >= 0) && (i < jsonText.Length))
		{
			if (IsCommentStart(jsonText, i, "//"))
			{
				i = SeekCommentEnd(jsonText, i, "\n");
				continue;
			}

			if (IsCommentStart(jsonText, i, "/*"))
			{
				i = SeekCommentEnd(jsonText, i, "*/");
				continue;
			}

			sb.Append(jsonText[i]);
			i++;
		}

		return sb.ToString();
	}

	// �C�ӂ̃L�[��
	private static bool IsCommentStart(string jsonText, int startIndex, string startCode)
	{
		// �����񂪑���Ȃ��ꍇ�̓}�b�`���Ȃ�
		if (jsonText.Length < (startCode.Length + startIndex))
			return false;

		for (int i = 0; i < startCode.Length; i++)
		{
			if (jsonText[startIndex + i] != startCode[i])
				return false;
		}
		return true;
	}

	// �I���R�[�h�܂Ői�߂�
	private static int SeekCommentEnd(string jsonText, int startIndex, string endCode)
	{
		var endIndex = jsonText.IndexOf(endCode, startIndex);
		if (endIndex < 0)
			return endIndex;

		return endIndex + endCode.Length;
	}


	// �I�u�W�F�N�g�̃N���X�̌^���擾
	private static Type GetObjectType(IDictionary<string, object> jsonObj)
	{
		try
		{
			// �I�u�W�F�N�g����class�p�����[�^�̃C���X�^���X�𐶐�
			if (!jsonObj.TryGetValue("class", out var className))
			{
				throw new Exception("class not found.");
			}
			var result = Type.GetType(className.ToString());
			if (result == null)
			{
				throw new Exception($"[{className}] is not type");
			}
			return result;
		}
		catch (Exception e)
		{
			throw new Exception( $"GetObjectType error. {e.Message}");
		}
	}

	// �I�u�W�F�N�g�N���X�쐬
	private static object CreateInstance(IDictionary<string, object> jsonObj)
	{
		var classType = GetObjectType(jsonObj);
		return Activator.CreateInstance(classType);
	}


	// �I�u�W�F�N�g�z��𐶐�
	private static IList<object> CreateArray(IList<object> jsonList)
	{
		var tmp = new List<object>();
		foreach (var item in jsonList)
		{
			var elem = item as IDictionary<string, object>;
			if (elem == null)
				continue;

			tmp.Add(CreateObject(elem));
		}

		return tmp;
	}


	// �I�u�W�F�N�g�𐶐�
	private static object CreateObject(IDictionary<string, object> jsonObj, object userObject = null)
	{
		var tmp = new Dictionary<string, object>();
		object obj = null;


		try
		{
			obj = (userObject == null) ? CreateInstance(jsonObj) : userObject;

			foreach (var item in jsonObj)
			{
				// object�z��Ȃ�
				var arrayNode = item.Value as IList<object>;
				if ((arrayNode != null) && (arrayNode[0] is IDictionary<string, object>))
				{
					tmp.Add(item.Key, CreateArray(arrayNode));
					continue;
				}

				var objNode = item.Value as IDictionary<string, object>;
				if (objNode != null)
				{
					tmp.Add(item.Key, CreateObject(objNode));
					continue;
				}
			}

			// �I�u�W�F�N�g�쐬
			var type = obj.GetType();

			foreach (var item in tmp)
			{
				try
				{
					// ���O�ɓo�^���Ă�����̂��폜
					jsonObj.Remove(item.Key);

					// ���O�œo�^�������̂�ݒ�
					var field = type.GetField(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (field == null)
					{
						throw new Exception($"[{item.Key}]���N���X[{type.Name}]����擾�ł��܂���B");
					}

					// �f�[�^�̌��ƕϊ��悪IList�Ȃ�v�f���R�s�[
					if (typeof(IList).IsAssignableFrom(field.FieldType))
					{
						var arrayField = Activator.CreateInstance(field.FieldType) as IList;
						var srcList = item.Value as IList;
						if (srcList != null)
						{
							foreach (var srcValue in srcList)
							{
								arrayField.Add(srcValue);
							}
						}
						field.SetValue(obj, arrayField);
					}
					else
					{
						field.SetValue(obj, item.Value);
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Field[{item.Key}] setting error\n{e.Message}");
				}
			}

			// �I�u�W�F�N�g�̃p�����[�^��ݒ�
			var partsOne = Json.Serialize(jsonObj);
			JsonUtility.FromJsonOverwrite(partsOne, obj);
		}
		catch (Exception e)
		{
			var objName = (obj == null) ? "None" : obj.GetType().Name;
			throw new Exception($"object [{objName}] error. \n{e.Message}");
		}

		return obj;
	}
}
