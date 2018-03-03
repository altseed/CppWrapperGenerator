using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace CSharpWrapperGenerator
{
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

            string dllClassName = "WrapperDLL";

            List<string> dll_headerText = new List<string>();
            List<string> dll_cppText = new List<string>();
            List<string> lib_headerText = new List<string>();
            List<string> lib_cppText = new List<string>();

            dll_headerText.Add("#pragma once");
            dll_headerText.Add("");
            dll_headerText.Add("class " + dllClassName + " {");
            dll_headerText.Add("public:");

            dll_cppText.Add("#include \"asd.WrapperDLL.h\"");
            dll_cppText.Add("");

            lib_headerText.Add("#pragma once");
            lib_headerText.Add("");
            lib_headerText.Add("asd {");

            lib_cppText.Add("#include \"asd.WrapperLib.h\"");
            lib_cppText.Add("");
            lib_cppText.Add("asd {");

            foreach (var c in doxygenParser.ClassDefs)
            {
                var dllFuncPrefix = c.Name + "_";

                lib_headerText.Add("class " + c.Name + "{");
                lib_headerText.Add("void* self = nullptr;");
                lib_headerText.Add("public:");

                foreach (var m in c.Methods)
                {
                    if (m.IsStatic) continue;

                    var isConstract = string.IsNullOrEmpty(m.ReturnType) && !m.Name.Contains("~");
                    var isDestruct = string.IsNullOrEmpty(m.ReturnType) && m.Name.Contains("~");

                    if (isConstract) continue;
                    if (isDestruct) continue;

                    var dllFuncName = dllFuncPrefix + m.Name;

                    // dll
                    var dllFuncArg = "(void* self" + (m.Parameters.Count > 0 ? "," : "") + string.Join(",", m.Parameters.Select(_ => _.Type + " " + _.Name).ToArray()) + ")";

                    // dll header
                    dll_headerText.Add("virtual " + m.ReturnType + " " + dllFuncName + dllFuncArg + ";");

                    // dll cpp
                    dll_cppText.Add(m.ReturnType + dllClassName + "::" + dllFuncName + dllFuncArg + "{");

                    dll_cppText.Add("auto self_ = (" + c.Name + ")self;");
                    dll_cppText.Add("self_->" + m.Name + "(" + string.Join(",", m.Parameters.Select(_ => _.Type + " " + _.Name).ToArray()) + ");");
                    dll_cppText.Add("};");
                    dll_cppText.Add("");

                    // lib
                    var libFuncArg = "(" + string.Join(",", m.Parameters.Select(_ => _.Type + " " + _.Name).ToArray()) + ")";

                    // lib header
                    lib_headerText.Add(m.ReturnType + " " + m.Name + dllFuncArg + ";");

                    // lib cpp
                    lib_cppText.Add(m.ReturnType + c.Name + "::" + m.Name + libFuncArg + "{");

                    lib_cppText.Add("dll->" + dllFuncName + "(self);");
                    lib_cppText.Add("};");
                    lib_cppText.Add("");
                }

                lib_headerText.Add("};");
                lib_headerText.Add("");
            }

            dll_headerText.Add("}");

            lib_headerText.Add("}");
            lib_cppText.Add("}");


            System.IO.File.WriteAllLines("dll.h", dll_headerText.ToArray());
            System.IO.File.WriteAllLines("dll.cpp", dll_cppText.ToArray());
            System.IO.File.WriteAllLines("lib.h", lib_headerText.ToArray());
            System.IO.File.WriteAllLines("lib.cpp", lib_cppText.ToArray());
        }
    }



}
