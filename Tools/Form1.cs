using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Functions;
using System.Xml.Linq;

namespace Tools
{
    public partial class Main : Form
    {
        private StringBuilder log = new StringBuilder();

        public static string PREFAB_PATH = $"{Environment.CurrentDirectory}/..";

        public Main()
        {
#if DEBUG
            PREFAB_PATH = "E:/project/manape_unity";
#else
#endif
            PREFAB_PATH = PREFAB_PATH.Replace("\\", "/");
            log.Clear();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ConfigBtn_Click(object sender, EventArgs e)
        {
            var directory = $"{PREFAB_PATH}/Excel";
            directory = directory.Replace("\\", "/");
            log.AppendLine(directory);
            if (!Directory.Exists(directory))
            {
                log.AppendLine($"{directory} not exist");
                return;
            }

            var csvDirectory = $"{directory}/Csv";
            if (!Directory.Exists(csvDirectory))
            {
                Directory.CreateDirectory(csvDirectory);
            }

            StringBuilder stringBuilder = new StringBuilder();

            var files = Directory.GetFiles(directory);
            foreach (var excelFile in files)
            {
                var workbook = new XSSFWorkbook(excelFile);
                var sheet = workbook.GetSheetAt(0);
                if (sheet != null)
                {
                    var firstRow = sheet.GetRow(0);
                    var names = firstRow.Cells[0].StringCellValue.Split('=');
                    var name = names[names.Length - 1];

                    // 处理csv
                    var str = GetExcelContent(sheet);
                    stringBuilder.Append(str);
                    stringBuilder.Append('\n');
                    File.WriteAllText($"{csvDirectory}/{name}.csv", str);

                    // 处理csharp类文件
                    var classSource = GetCSharpContent(sheet, name);
                    File.WriteAllText($"{directory}/../Man Ape/Assets/HotUpdate/Config/ConfigFile/{name}.cs", classSource);
                }
            }

            var encryptStr = AESHelper.AESEncrypt(stringBuilder.ToString());
            var outputDirectory = $"{directory}/../Resource/Windows/Data";
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            File.WriteAllText($"{outputDirectory}/data", encryptStr);
            log.AppendLine($"{outputDirectory}/data");
            log.AppendLine("生成配置文件成功");
            ShowLog();
        }

        private string GetExcelContent(ISheet sheet)
        {
            var str = new StringBuilder();
            str.Append(sheet.LastRowNum - 2);
            // 行用=
            for (var i = 2; i <= sheet.LastRowNum; ++i)
            {
                if (i == 3)
                {
                    // 不需要类型信息
                    continue;
                }
                
                str.Append('\n');

                var row = sheet.GetRow(i);
                // 列不能用=
                for (var j = 0; j < row.LastCellNum; ++j)
                {
                    var value = "";
                    if (row.Cells[j].CellType == CellType.String)
                    {
                        value = row.Cells[j].StringCellValue;
                    }
                    else if (row.Cells[j].CellType == CellType.Numeric)
                    {
                        value = row.Cells[j].NumericCellValue.ToString();
                    }

                    if (value.Contains(","))
                    {
                        value = $"\"{value}\"";
                    }
                    if (j != 0)
                    {
                        str.Append('^');
                    }
                    str.Append(value);
                }
            }

            return str.ToString();
        }

        private string GetCSharpContent(ISheet sheet, string name)
        {
            // 处理csharp
            var classSource = new StringBuilder();
            classSource.AppendLine("using UnityEngine;\nusing System.Collections;\n");
            classSource.AppendLine($"public partial class {name} : ConfigBase");
            classSource.AppendLine("{");
            var row2 = sheet.GetRow(2);
            var row3 = sheet.GetRow(3);
            for (var i = 0; i < row2.LastCellNum; ++i)
            {
                var iName = row2.Cells[i].StringCellValue;
                var iType = row3.Cells[i].StringCellValue;
                if (iName.Equals("id"))
                {
                    classSource.AppendLine($"\tpublic {iType} {iName};");
                }
                else
                {
                    classSource.AppendLine($"\tpublic {iType} {iName};");
                }
            }
            classSource.AppendLine("}");

            return classSource.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ConfigParseBtn_Click(object sender, EventArgs e)
        {
            ConfigTool.ParseAllSerialize();
        }

        public void ShowLog()
        {
            label1.Text = log.ToString();
        }
    }
}
