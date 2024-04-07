using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Md5Util
{
	public static string GetHash(string _strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		return GetHash(ue.GetBytes(_strToEncrypt));
	}

	private static string GetHash(byte[] bytesToEncrypt)
	{
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytesToEncrypt);

		string hashString = "";
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		return hashString.PadLeft(32, '0');
	}
}
