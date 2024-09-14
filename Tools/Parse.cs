using System;
using System.Collections.Generic;

public class Parse
{

	//注册
	public static void RegisterEvents()
	{
		ParseFactory.RegisterEvent("int", ParseInt);
		ParseFactory.RegisterEvent("Int64", ParseInt64);
		ParseFactory.RegisterEvent("float", ParseFloat);
		ParseFactory.RegisterEvent("long", ParseLong);
		ParseFactory.RegisterEvent("double", ParseDouble);
		ParseFactory.RegisterEvent("string", ParseString);
		ParseFactory.RegisterEvent("bool", ParseBool);
		ParseFactory.RegisterEvent("array", ParseArray);
		ParseFactory.RegisterEvent("enum", ParseEnum);
		ParseFactory.RegisterEvent("class", ParseClass);
		ParseFactory.RegisterEvent("GValueInt", ParseGValueInt);
		ParseFactory.RegisterEvent("GValueFloat", ParseGValueFloat);
		ParseFactory.RegisterEvent("Color", ParseColor);
		ParseFactory.RegisterEvent("Color32", ParseColor32);
		ParseFactory.RegisterEvent("Vector3", ParseVector3);
		ParseFactory.RegisterEvent("Vector2", ParseVector2);
	}

	private static string GetTypeListSplitTag(string type)
	{
		string tag = "},";
		if (type.Equals("int") || type.Equals("Int64") || type.Equals("float") || type.Equals("long") ||
			type.Equals("double") || type.Equals("string")
			|| type.Equals("bool") || type.Contains("ENUM") || type.Equals("GValueInt") || type.Equals("GValueFloat"))
		{
			tag = ",";
		}
		return tag;
	}

	#region 具体事件
	private static string ParseClass(string keyName, string type, string valueTag, string startString)
	{
		return $@"{startString}{keyName} = new {type}();
{startString}				var strs = {valueTag}.Split(',');
{startString}				if (strs != null && strs.Length > 0)
{startString}				{{
{startString}					var length = strs.Length;
{startString}					for (int attrIndex = 0; attrIndex < length; attrIndex++)
{startString}					{{
{startString}						if (!string.IsNullOrEmpty(strs[attrIndex]))
{startString}						{{
{startString}							var kv = strs[attrIndex].Split('=');
{startString}							{keyName}.AddAttribute(kv[0], kv[1]);
{startString}						}}
{startString}					}}
{startString}				}}";
	}
		
	private static string ParseColor(string keyName, string type, string valueTag, string startString)
	{
		return $@"{startString}var strs = {valueTag}.Split(',');
{startString}				if (strs.Length == 3)
{startString}				{{
{startString}					{keyName} = new {type}(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
{startString}				}}
{startString}				else if (strs.Length == 4)
{startString}				{{
{startString}					{keyName} = new {type}(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]));
{startString}				}}";
	}
		
	private static string ParseColor32(string keyName, string type, string valueTag, string startString)
	{
		return $@"{startString}var strs = {valueTag}.Split(',');
{startString}				if (strs.Length == 4)
{startString}				{{
{startString}					{keyName} = new {type}(byte.Parse(strs[0]), byte.Parse(strs[1]), byte.Parse(strs[2]), byte.Parse(strs[3]));
{startString}				}}";
	}
		
	private static string ParseVector3(string keyName, string type, string valueTag, string startString)
	{
		return $@"{startString}var strs = {valueTag}.Split(',');
{startString}				if (strs.Length == 3)
{startString}				{{
{startString}					{keyName} = new {type}(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
{startString}				}}";
	}
		
	private static string ParseVector2(string keyName, string type, string valueTag, string startString)
	{
		return $@"{startString}var strs = {valueTag}.Split(',');
{startString}				if (strs.Length == 2)
{startString}				{{
{startString}					{keyName} = new {type}(float.Parse(strs[0]), float.Parse(strs[1]));
{startString}				}}";
	}

	private static string ParseInt(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = int.Parse({valueTag});";
	}

	private static string ParseInt64(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = Int64.Parse({valueTag});";
	}

	private static string ParseFloat(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = float.Parse({valueTag});";
	}

	private static string ParseLong(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = long.Parse({valueTag});";
	}

	private static string ParseDouble(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = double.Parse({valueTag});";
	}

	private static string ParseString(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = {valueTag};";
	}

	private static string ParseBool(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = {valueTag}.ToLower().Equals(\"true\");";
	}

	private static string ParseEnum(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = ({type})Enum.Parse(typeof({type}), {valueTag});";
	}

	private static string ParseGValueFloat(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = new GValueFloat(float.Parse({valueTag}));";
	}
		
	private static string ParseGValueInt(string keyName, string type, string valueTag, string startString)
	{
		return $"{startString}{keyName} = new GValueInt(int.Parse({valueTag}));";
	}

	private static string ParseArray(string keyName, string type, string valueTag, string startString)
	{
		var splitTag = GetTypeListSplitTag(type);
		string elementStr = "";
		if (splitTag.Equals(","))
		{
			elementStr = ParseType($"{keyName}[arryIndex]", type, "arrayStrs[arryIndex]", "\t\t");
		}
		else
		{
			elementStr = ParseType($"{keyName}[arryIndex]", type, "arrayStrs[arryIndex].Replace(\"{\", \"\").Replace(\"}\", \"\")", "\t\t");
		}
		return $@"{startString}var arrayStrs = {valueTag}.Split(""{splitTag}"");
{startString}				if (arrayStrs != null)
{startString}				{{
{startString}					int arrayLength = arrayStrs.Length;
{startString}					{keyName} = new {type}[arrayLength];
{startString}					for (int arryIndex = 0; arryIndex < arrayLength; arryIndex++)
{startString}					{{
{startString}				{elementStr}
{startString}					}}
{startString}				}}";
	}
	#endregion

	//接收需要解析的文件字符串
	public static string Foo(string content)
	{
		content = content.Trim();
		var strs = content.Split(' ');
		return ParseType(strs[2], strs[1], "_value", "");
	}

	public static string ParseType(string keyName, string type, string valueTag, string startString)
	{
		// 数组
		if (type.Contains("[]"))
		{
			return ParseArray(keyName, type.Replace("[]", ""), valueTag , startString);
		}
		// 枚举
		if (type.ToUpper().Contains("ENUM"))
		{
			return ParseEnum(keyName, type, valueTag , startString);
		}
		// 注册的基础类型
		var e = ParseFactory.GetParseEvent(type);
		if (e != null)
		{
			return e(keyName, type, valueTag, startString);
		}
		// 自定义类
		return ParseClass(keyName, type, valueTag , startString);
	}
}

/// <summary>
/// 解析的工厂
/// </summary>
public class ParseFactory
{
	public delegate string ParseEvent(string keyName, string type, string valueTag, string startString);
	private static Dictionary<string, ParseEvent> parseEvents = new Dictionary<string, ParseEvent>();
	/// <summary>
	/// 注册事件
	/// </summary>
	/// <param name="key">类型</param>
	/// <param name="e">解析类型</param>
	public static void RegisterEvent(string key, ParseEvent e)
	{
		if (!parseEvents.ContainsKey(key))
		{
			parseEvents.Add(key, e);
		}
	}
	/// <summary>
	/// 获得事件
	/// </summary>
	/// <param name="key">类型</param>
	/// <returns></returns>
	public static ParseEvent GetParseEvent(string key)
	{
		if (parseEvents.ContainsKey(key))
		{
			return parseEvents[key];
		}
		return null;
	}
}