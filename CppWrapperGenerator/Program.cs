﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace CppWrapperGenerator
{
    /// <summary>
    /// 
    /// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			if(args.Length < 1)
			{
				Console.WriteLine("第１引数に設定ファイルを指定してください");
				return;
			}

			Settings settings;
			var settingFilePath = args[0];
			var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Settings));
			using (var file = File.Open(settingFilePath, FileMode.Open))
			{
				settings = serializer.ReadObject(file) as Settings;
			}

			var settingsDirectory = Path.GetDirectoryName(args[0]);

			var doxygenParser = new DoxygenParser();
			try
			{
				doxygenParser.AddNamespaceFile(Path.Combine(settingsDirectory, settings.DoxygenXmlDirPath, "namespaceasd.xml"));
				var docs = Directory.EnumerateFiles(Path.Combine(settingsDirectory, settings.DoxygenXmlDirPath), "*.xml", SearchOption.TopDirectoryOnly).ToArray();
				docs = docs.Where(_ => _.Contains("classasd_")).ToArray();

				doxygenParser.AddClassFiles(docs);
			}
			catch (DirectoryNotFoundException ex)
			{
				Console.WriteLine("Doxygenのディレクトリが見つかりませんでした。");
				return;
			}
			catch(FileNotFoundException ex)
			{
				Console.WriteLine("Doxygenのxmlドキュメントが見つかりませんでした。");
				return;
			}

            Exporter exporter = new Exporter(settings, doxygenParser.Result);

			exporter.ReleasableClasses = settings.ReleasableClassList;
            exporter.UnwrappedClasses = settings.UnwrappedClassList;
            exporter.Export(settings.ExportLibDirPath, settings.ExportDLLDirPath);
        }
    }



}
