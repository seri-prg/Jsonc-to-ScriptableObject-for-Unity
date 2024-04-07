using System;
using System.Collections.Generic;
using UnityEngine;


public class SampleObject : ScriptableObject
{
	[SerializeField, Header("サンプル名")]
	private string sampleName;

	[SerializeField, Header("サンプル値")]
	private int value;

	[SerializeReference, Header("データ配列")]
	private List<ItemBase> itemList;
}


[Serializable]
public abstract class ItemBase
{
	[SerializeField]
	private string name;
}

[Serializable]
public class ItemString : ItemBase
{
	[SerializeField]
	private string strValue;
}

[Serializable]
public class ItemInt : ItemBase
{
	[SerializeField]
	private int intValue;
}

