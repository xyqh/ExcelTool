using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Tools;

public class ConfigTool
{
	public static void ParseAllSerialize()
	{
		Parse.RegisterEvents();
		ParseAllConfigFiles();
		LoadExcelNames();
		SaveCsvFiles();
	}

	static List<string> files = new List<string>();
	/// <summary>
	/// 遍历目录及其子目录
	/// </summary>
	static void Recursive(string path)
	{
		string[] names = Directory.GetFiles(path);
		string[] dirs = Directory.GetDirectories(path);
		foreach (string filename in names)
		{
			string ext = Path.GetExtension(filename);
			if (ext.Equals(".meta")) continue;
			files.Add(filename.Replace('\\', '/'));
		}
		foreach (string dir in dirs)
		{
			Recursive(dir.Replace('\\', '/'));
		}
	}

	private static void ParseAllConfigFiles()
	{
		string resPath = $"{Main.PREFAB_PATH}/Man Ape/Assets/HotUpdate/Config/ConfigFile";
        string exportPath = $"{Main.PREFAB_PATH}/Man Ape/Assets/HotUpdate/Config/ConfigPartial";
		Recursive(resPath);
		for (int i = 0; i < files.Count; i++)
		{
			var strs = files[i].Replace(resPath, "~").Split('~');
			var exportFile = exportPath + strs[1].Replace(".cs", "Partial.cs");
			if (File.Exists(exportFile)) File.Delete(exportFile);
			ParseFile(files[i], exportFile);
		}
	}

	public static void ParseFile(string readPath, string writePath)
	{
		var fileName = readPath.Replace($"{Main.PREFAB_PATH}/Man Ape/Assets/HotUpdate/Config/ConfigFile", "");
		var dirNames = writePath.Split('/');
		var directory = writePath.Replace(dirNames[dirNames.Length - 1], "");
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		string[] rows = File.ReadAllLines(readPath);
		string outPutStr = "using UnityEngine;\r\nusing System;\r\n";
		bool isFirstAttr = true;
		for (int index = 0; index < rows.Length; index++)
		{
			var row = rows[index];
			if (row.Contains("using") && !row.Contains("UnityEngine;") && !row.Contains("System;"))
			{
				outPutStr += row + "\r\n";
				continue;
			}
			else if (row.Contains("partial"))
			{
				var name = row.Trim().Replace("\t", "").Split(' ')[3];
				outPutStr += string.Format(System.Globalization.CultureInfo.InvariantCulture, "\r\npublic partial class {0}\r\n{{\r\n", name);
				outPutStr += "\tpublic override void AddAttribute(string _key, string _value)\r\n\t{\r\n";
				outPutStr += "\t\tbase.AddAttribute(_key, _value);\r\n";
				outPutStr += "\t\tif (string.IsNullOrEmpty(_value))\r\n\t\t\treturn;\r\n";
				continue;
			}
			var newRow = row.Trim().Replace("\t", "").Split(';')[0];
			if (newRow.ToLower(System.Globalization.CultureInfo.InvariantCulture).Contains("[end]"))
			{
				break;
			}
			if (!string.IsNullOrEmpty(newRow))
			{
				var strs = newRow.Split(' ');
				if (strs.Length >= 3)
				{
					if (isFirstAttr)
					{
						isFirstAttr = false;
						outPutStr += string.Format(System.Globalization.CultureInfo.InvariantCulture, "\t\tif(_key.Equals(\"{0}\"))\r\n\t\t{{\r\n\t\t\t", strs[2]);
					}
					else
					{
						outPutStr += string.Format(System.Globalization.CultureInfo.InvariantCulture, "\r\n\t\telse if(_key.Equals(\"{0}\"))\r\n\t\t{{\r\n\t\t\t", strs[2]);
					}
					outPutStr += "try\r\n\t\t\t{\r\n\t\t\t\t";
					outPutStr += Parse.Foo(newRow) + "\r\n";
                    outPutStr += "\t\t\t}\r\n\t\t\tcatch (Exception exception)\r\n\t\t\t{\r\n\t\t\t\tDebug.LogError(\"" + fileName + " File Serialize Error with key: " + strs[2] + "   value: \" + _value +\"" + "\\n\" + exception.ToString());\r\n\t\t\t}\r\n";
					outPutStr += "\t\t}";
				}
			}
		}
		outPutStr += GetEndParseString();
		File.WriteAllText(writePath, outPutStr);
	}

	private static string GetEndParseString()
	{
		return "\r\n\t}\r\n}";
	}
		
	private static List<string> allCsvFiles = new List<string>();
	private static void LoadExcelNames()
	{
		allCsvFiles.Clear();
		DirectoryInfo csvDirectoryInfo = new DirectoryInfo($"{Main.PREFAB_PATH}/Excel");
		if (csvDirectoryInfo.Exists && csvDirectoryInfo.GetFiles().Length > 0)
		{
			foreach (FileInfo fileInfo in csvDirectoryInfo.GetFiles())
			{
				if (!fileInfo.Name.Contains(".meta") && !fileInfo.Name.Contains("~") && fileInfo.Name.Contains(".xlsx"))
				{
					allCsvFiles.Add(fileInfo.Name);
				}
			}
		}
	}
		
	private static void SaveCsvFiles()
	{
		if (allCsvFiles.Count > 0)
		{
			string register =
				"public class ConfigRegister\n{\n";
			register += "\tpublic static void LoadConfigFiles()\n\t{\n";
			Regex regex = new Regex(@"#.*\.", RegexOptions.IgnoreCase);
			for (int index = 0; index < allCsvFiles.Count; index++)
			{
				register += $"\t\tConfigManager.GetInstance().LoadingConfigFile<{regex.Replace(allCsvFiles[index], ".").Replace(".xlsx", "")}>();\n";
			}
			//register += "\t\tConfigManager.GetInstance().LoadingConfigFile<LocalConfig>();\n";
			register += "\t}\n}";

			var writePath = $"{Main.PREFAB_PATH}/Man Ape/Assets/HotUpdate/Config/ConfigRegister.cs";
            File.WriteAllText(writePath, register);
		}
	}
}