using System;
using System.Collections.Generic;
using UnityEngine;


public class SampleObject : ScriptableObject
{
	[SerializeField, Header("�T���v����")]
	private string sampleName;

	[SerializeField, Header("�T���v���l")]
	private int value;

	[SerializeReference, Header("�f�[�^�z��")]
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

