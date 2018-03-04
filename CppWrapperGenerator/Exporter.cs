using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWrapperGenerator
{
	class Exporter
	{
		private Settings settings;
		private ParseResult doxygen;

        List<string> dll_headerText = new List<string>();
        List<string> dll_cppText = new List<string>();
        List<string> lib_headerText = new List<string>();
        List<string> lib_cppText = new List<string>();

        int dll_h_indent = 0;
        int dll_cpp_indent = 0;
        int lib_h_indent = 0;
        int lib_cpp_indent = 0;

        string dllClassName = "WrapperDLL";

        public Exporter(Settings settings, ParseResult doxygen)
		{
			this.settings = settings;
			this.doxygen = doxygen;
		}

		public void Export()
		{
            AddDLLH("#pragma once");
            AddDLLH("");

            AddDLLH("class " + dllClassName + " {");
            AddDLLH("public:");

            AddDLLCPP("#include \"asd.WrapperDLL.h\"");
            AddDLLCPP("");

            AddLIBH("#pragma once");
            AddLIBH("");
            AddLIBH("asd {");
            AddLIBH("");

            AddLIBH("void InitializeWrapper();");
            AddLIBH("void TerminateWrapper();");
            AddLIBH("");

            AddLIBCPP("#include \"asd.WrapperLib.h\"");
            AddLIBCPP("");
            AddLIBCPP("asd {");
            AddLIBCPP("");
            AddLIBCPP("static " + dllClassName + "* dll = nullptr;");
            AddLIBCPP("");
            AddLIBCPP("void InitializeWrapper() {");
            AddLIBCPP("\tdll = (" + dllClassName + "*)Create" + dllClassName + "();");
            AddLIBCPP("};");
            AddLIBCPP("");
            AddLIBCPP("void TerminateWrapper() {");
            AddLIBCPP("\tDelete" + dllClassName + "(dll);");
            AddLIBCPP("};");


            PushDLLHIndent();

            foreach (var c in doxygen.ClassDefs)
            {
                var dllFuncPrefix = c.Name + "_";

                AddLIBH("class " + c.Name + " {");
                PushLIBHIndent();
                AddLIBH("void* self = nullptr;");
                AddLIBH("public:");

                foreach (var m in c.Methods)
                {
                    if (m.IsStatic) continue;
                    if (!m.IsPublic) continue;

                    var isConstract = string.IsNullOrEmpty(m.ReturnType) && !m.Name.Contains("~");
                    var isDestruct = string.IsNullOrEmpty(m.ReturnType) && m.Name.Contains("~");

                    var methodName = m.Name;
                    var returnType = m.ReturnType;
                    var libParameters = m.Parameters.ToArray().ToList();
                    var dllParameters = m.Parameters.ToArray().ToList();

                    if (isConstract)
                    {
                        methodName = "Construct";
                        returnType = "void *";
                    }
                    else if (isDestruct)
                    {
                        methodName = "Destruct";
                        returnType = "void";

                        ParameterDef def = new ParameterDef();
                        def.Type = "void *";
                        def.Name = "self";
                        dllParameters.Insert(0, def);
                    }
                    else
                    {
                        ParameterDef def = new ParameterDef();
                        def.Type = "void *";
                        def.Name = "self";
                        dllParameters.Insert(0, def);
                    }

                    var dllFuncName = dllFuncPrefix + methodName;

                    // dll
                    var dllFuncArg = "(" + string.Join(",", dllParameters.Select(_ => _.Type + " " + _.Name).ToArray()) + ")";

                    // dll header
                    AddDLLH("virtual " + returnType + " " + dllFuncName + dllFuncArg + ";");

                    // dll cpp
                    AddDLLCPP(returnType + " " + dllClassName + "::" + dllFuncName + dllFuncArg + "{");

                    PushDLLCppIndent();

                    if(isConstract)
                    {
                        AddDLLCPP("return " + c.Name + "(" + string.Join(",", libParameters.Select(_ => _.Name).ToArray()) + ");");
                    }
                    else if(isDestruct)
                    {
                        AddDLLCPP("auto self_ = (" + c.Name + "*)self;");
                        AddDLLCPP("delete self_;");
                    }
                    else
                    {
                        AddDLLCPP("auto self_ = (" + c.Name + "*)self;");

                        if (returnType == "void")
                        {
                            AddDLLCPP("self_->" + methodName + "(" + string.Join(",", libParameters.Select(_ => _.Name).ToArray()) + ");");
                        }
                        else
                        {
                            AddDLLCPP("return self_->" + methodName + "(" + string.Join(",", libParameters.Select(_ => _.Name).ToArray()) + ");");
                        }
                    }

                    PopDLLCppIndent();

                    AddDLLCPP("};");
                    AddDLLCPP("");

                    // lib
                    var libFuncArg = "(" + string.Join(",", libParameters.Select(_ => _.Type + " " + _.Name).ToArray()) + ")";

                    // lib header
                    if (isConstract)
                    {
                        AddLIBH(c.Name + libFuncArg + ";");
                    }
                    else if (isDestruct)
                    {
                        AddLIBH("virtual ~" + c.Name + libFuncArg + ";");
                    }
                    else
                    {
                        AddLIBH(returnType + " " + methodName + libFuncArg + ";");
                    }

                    // lib cpp
                    if (isConstract)
                    {
                        AddLIBCPP(c.Name + "::" + c.Name + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("self = dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select(_ => _.Name).ToArray()) + ");");
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    else if (isDestruct)
                    {
                        AddLIBCPP(c.Name + "::~" + c.Name + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select(_ => _.Name).ToArray()) + ");");
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    else
                    {
                        AddLIBCPP(returnType + " " + c.Name + "::" + methodName + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            if (returnType == "void")
                            {
                                AddLIBCPP("dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select(_ => _.Name).ToArray()) + ");");
                            }
                            else
                            {
                                AddLIBCPP("return dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select(_ => _.Name).ToArray()) + ");");
                            }
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    
                    AddLIBCPP("");
                }

                PopLIBHIndent();
                AddLIBH("};");
                AddLIBH("");
            }

            PopDLLHIndent();

            AddDLLH("}");

            AddDLLH("extern \"C\" {");
            AddDLLH("void* Create" + dllClassName + "();");
            AddDLLH("void Delete" + dllClassName + "(void* o);");
            AddDLLH("}");

            AddDLLCPP("void* Create" + dllClassName + "() {");
            PushDLLCppIndent();
            AddDLLCPP("return new " + dllClassName + "();");
            PopDLLCppIndent();
            AddDLLCPP("}");
            AddDLLCPP("");

            AddDLLCPP("void Delete" + dllClassName + "(void* o) {");
            PushDLLCppIndent();
            AddDLLCPP("auto o_ = (" + dllClassName + "*)o;");
            AddDLLCPP("delete o_;");
            PopDLLCppIndent();
            AddDLLCPP("}");


            AddLIBH("}");
            AddLIBCPP("}");


            System.IO.File.WriteAllLines("dll.h", dll_headerText.ToArray());
            System.IO.File.WriteAllLines("dll.cpp", dll_cppText.ToArray());
            System.IO.File.WriteAllLines("lib.h", lib_headerText.ToArray());
            System.IO.File.WriteAllLines("lib.cpp", lib_cppText.ToArray());
        }

        void AddDLLH(string str)
        {
            string s = string.Empty;
            for(int i = 0; i < dll_h_indent; i++)
            {
                s += "\t";
            }

            s += str;

            dll_headerText.Add(s);
        }

        void AddDLLCPP(string str)
        {
            string s = string.Empty;
            for (int i = 0; i < dll_cpp_indent; i++)
            {
                s += "\t";
            }

            s += str;

            dll_cppText.Add(s);
        }

        void AddLIBH(string str)
        {
            string s = string.Empty;
            for (int i = 0; i < lib_h_indent; i++)
            {
                s += "\t";
            }

            s += str;

            lib_headerText.Add(s);
        }

        void AddLIBCPP(string str)
        {
            string s = string.Empty;
            for (int i = 0; i < lib_cpp_indent; i++)
            {
                s += "\t";
            }

            s += str;

            lib_cppText.Add(s);
        }

        void PushDLLHIndent() { dll_h_indent++; }
        void PopDLLHIndent() { dll_h_indent--; }

        void PushDLLCppIndent() { dll_cpp_indent++; }
        void PopDLLCppIndent() { dll_cpp_indent--; }

        void PushLIBHIndent() { lib_h_indent++; }
        void PopLIBHIndent() { lib_h_indent--; }

        void PushLIBCppIndent() { lib_cpp_indent++; }
        void PopLIBCppIndent() { lib_cpp_indent--; }

    }
}
