using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEngine;

/*
	jsonデータのclassパラメータからScriptableObjectを生成し、
	データの内容を設定。

	データの編集はjsonで行うがゲームからのアクセスはScriptableObjectで行いたい場合に便利
*/
public class JsonUtil 
{
	// jsonデータからScriptableObject生成
	public static ScriptableObject CreateScriptableObject(string jsonRootText)
	{
		jsonRootText = RemoveComment(jsonRootText);

		var jsonRoot = Json.Deserialize(jsonRootText) as Dictionary<string, object>;
		var rootType = GetObjectType(jsonRoot);

		var obj = ScriptableObject.CreateInstance(rootType);
		JsonUtil.CreateObject(jsonRoot, obj);
		return obj;
	}


	// Jsonからコメント削除(jsonc → json)
	// 本当は文字列内だったら無視をしないといけない。
	// //～\n
	// /*～*/
	// 上記を削除
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

	// 任意のキーか
	private static bool IsCommentStart(string jsonText, int startIndex, string startCode)
	{
		// 文字列が足りない場合はマッチしない
		if (jsonText.Length < (startCode.Length + startIndex))
			return false;

		for (int i = 0; i < startCode.Length; i++)
		{
			if (jsonText[startIndex + i] != startCode[i])
				return false;
		}
		return true;
	}

	// 終了コードまで進める
	private static int SeekCommentEnd(string jsonText, int startIndex, string endCode)
	{
		var endIndex = jsonText.IndexOf(endCode, startIndex);
		if (endIndex < 0)
			return endIndex;

		return endIndex + endCode.Length;
	}


	// オブジェクトのクラスの型を取得
	private static Type GetObjectType(IDictionary<string, object> jsonObj)
	{
		try
		{
			// オブジェクト内のclassパラメータのインスタンスを生成
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

	// オブジェクトクラス作成
	private static object CreateInstance(IDictionary<string, object> jsonObj)
	{
		var classType = GetObjectType(jsonObj);
		return Activator.CreateInstance(classType);
	}


	// オブジェクト配列を生成
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


	// オブジェクトを生成
	private static object CreateObject(IDictionary<string, object> jsonObj, object userObject = null)
	{
		var tmp = new Dictionary<string, object>();
		object obj = null;


		try
		{
			obj = (userObject == null) ? CreateInstance(jsonObj) : userObject;

			foreach (var item in jsonObj)
			{
				// object配列なら
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

			// オブジェクト作成
			var type = obj.GetType();

			foreach (var item in tmp)
			{
				try
				{
					// 事前に登録してあるものを削除
					jsonObj.Remove(item.Key);

					// 自前で登録したものを設定
					var field = type.GetField(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (field == null)
					{
						throw new Exception($"[{item.Key}]がクラス[{type.Name}]から取得できません。");
					}

					// データの元と変換先がIListなら要素をコピー
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

			// オブジェクトのパラメータを設定
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
