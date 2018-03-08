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

        List<Func<TypeDef, MethodDef, string>> dll_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, MethodDef, string>> dll_h_arg_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, int, string, MethodDef, string>> dll_cpp_arg_rules = new List<Func<TypeDef, int, string, MethodDef, string>>();

        List<Func<TypeDef, MethodDef, string>> lib_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, MethodDef, string>> lib_h_arg_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, int, string, MethodDef, string>> lib_cpp_arg_rules = new List<Func<TypeDef, int, string, MethodDef, string>>();

        string dllClassName = "WrapperDLL";

        string GetDLLRet(TypeDef t, MethodDef m)
        {
            foreach(var f in dll_ret_rules)
            {
                var r = f(t, m);
                if (r != null) return r;
            }
            return "";
        }

        string GetDLLHArg(TypeDef t, MethodDef m)
        {
            foreach (var f in dll_h_arg_rules)
            {
                var r = f(t, m);
                if (r != null) return r;
            }
            return "";
        }

        string GetDLLCppArg(TypeDef t, int i, string s, MethodDef m)
        {
            foreach (var f in dll_cpp_arg_rules)
            {
                var r = f(t, i, s, m);
                if (r != null) return r;
            }
            return "";
        }

        string GetLIBRet(TypeDef t, MethodDef m)
        {
            foreach (var f in lib_ret_rules)
            {
                var r = f(t, m);
                if (r != null) return r;
            }
            return "";
        }

        string GetLIBHArg(TypeDef t, MethodDef m)
        {
            foreach (var f in lib_h_arg_rules)
            {
                var r = f(t, m);
                if (r != null) return r;
            }
            return "";
        }

        string GetLIBCppArg(TypeDef t, int i, string s, MethodDef m)
        {
            foreach (var f in lib_cpp_arg_rules)
            {
                var r = f(t, i, s, m);
                if (r != null) return r;
            }
            return "";
        }

        public Exporter(Settings settings, ParseResult doxygen)
		{
			this.settings = settings;
			this.doxygen = doxygen;

            foreach(var pt in BuildIn.PrimitiveType)
            {
                dll_ret_rules.Add(
                    (t,m) => 
                    {
                        if (t.Name != pt) return null;
                        return pt;
                    }
                );

                dll_h_arg_rules.Add(
                    (t, m) =>
                    {
                        if (t.Name != pt) return null;
                        return pt;
                    }
                );

                dll_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        if (t.Name != pt) return null;
                        return string.Format("auto arg{0} = {1};", i, s);
                    }
                );

                lib_ret_rules.Add(
                    (t, m) =>
                    {
                        if (t.Name != pt) return null;
                        return pt;
                    }
                );

                lib_h_arg_rules.Add(
                    (t, m) =>
                    {
                        if (t.Name != pt) return null;
                        return pt;
                    }
                );

                lib_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        if (t.Name != pt) return null;
                        return string.Format("auto arg{0} = {1};", i, s);
                    }
                );
            }

            // Other rules
            {
                dll_ret_rules.Add(
                    (t, m) =>
                    {
                        return "void*";
                    }
                );

                dll_h_arg_rules.Add(
                    (t, m) =>
                    {
                        return "void*";
                    }
                );

                dll_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        return string.Format("auto arg{0} = ({1}){2};", i, t.Name, s);
                    }
                );

                lib_ret_rules.Add(
                    (t, m) =>
                    {
                        return string.Format("std::shared_ptr<{0}>", t.Name.Replace("*", "").Replace(" ",""));
                    }
                );

                lib_h_arg_rules.Add(
                    (t, m) =>
                    {
                        return string.Format("std::shared_ptr<{0}>", t.Name.Replace("*", "").Replace(" ", ""));
                    }
                );

                lib_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        return string.Format("auto arg{0} = {1}.get()->self;", i, s);
                    }
                );
            }
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
                if (!settings.ClassWhiteList.Any(_ => _ == c.Name)) continue;

                var dllFuncPrefix = c.Name + "_";

                AddLIBH("class " + c.Name + " {");
                PushLIBHIndent();
                AddLIBH("void* self = nullptr;");
                AddLIBH("bool isCtrlSelf = false;");
                AddLIBH("public:");

                foreach (var m in c.Methods)
                {
                    if (m.IsStatic) continue;
                    if (!m.IsPublic) continue;

                    // A method which has an argument whose type is shared_ptr<T> is ignored.
                    if (m.Parameters.Any(_ => _.Type.IsSharedPtr)) continue;

                    // A method whose return type is shared_ptr<T> is ignored.
                    if (m.ReturnType.IsSharedPtr) continue;

                    var isConstract = string.IsNullOrEmpty(m.ReturnType.Name) && !m.Name.Contains("~");
                    var isDestruct = string.IsNullOrEmpty(m.ReturnType.Name) && m.Name.Contains("~");

                    var methodName = m.Name;
                    var returnType = m.ReturnType;
                    var libParameters = m.Parameters.ToArray().ToList();
                    var dllParameters = m.Parameters.ToArray().ToList();

                    if (isConstract)
                    {
                        methodName = "Construct";
                        returnType.Name = "void *";
                    }
                    else if (isDestruct)
                    {
                        methodName = "Destruct";
                        returnType.Name = "void";

                        ParameterDef def = new ParameterDef();
                        def.Type.Name = "void *";
                        def.Name = "self";
                        dllParameters.Insert(0, def);
                    }
                    else
                    {
                        ParameterDef def = new ParameterDef();
                        def.Type.Name = "void *";
                        def.Name = "self";
                        dllParameters.Insert(0, def);
                    }

                    var dllFuncName = dllFuncPrefix + methodName;

                    // dll
                    var dllFuncArg = "(" + string.Join(",", dllParameters.Select((_,i) => GetDLLHArg(_.Type, m) + " " + _.Name).ToArray()) + ")";

                    // dll header
                    AddDLLH("virtual " + GetDLLRet(returnType, m) + " " + dllFuncName + dllFuncArg + ";");

                    // dll cpp
                    AddDLLCPP(GetDLLRet(returnType, m) + " " + dllClassName + "::" + dllFuncName + dllFuncArg + "{");

                    PushDLLCppIndent();

                    if(isConstract)
                    {
                        var argConverters = libParameters.Select((_, i) => GetDLLCppArg(_.Type, i, _.Name, m));
                        foreach(var a in argConverters)
                        {
                            AddDLLCPP(a);
                        }

                        AddDLLCPP("return " + c.Name + "(" + string.Join(",", libParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                    }
                    else if(isDestruct)
                    {
                        AddDLLCPP("auto self_ = (" + c.Name + "*)self;");
                        AddDLLCPP("delete self_;");
                    }
                    else
                    {
                        AddDLLCPP("auto self_ = (" + c.Name + "*)self;");

                        var argConverters = libParameters.Select((_, i) => GetDLLCppArg(_.Type, i, _.Name, m));
                        foreach (var a in argConverters)
                        {
                            AddDLLCPP(a);
                        }

                        if (returnType.Name == "void")
                        {
                            AddDLLCPP("self_->" + methodName + "(" + string.Join(",", libParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                        }
                        else
                        {
                            AddDLLCPP("auto ret = self_->" + methodName + "(" + string.Join(",", libParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                            AddDLLCPP("return ret;");
                        }
                    }

                    PopDLLCppIndent();

                    AddDLLCPP("};");
                    AddDLLCPP("");

                    // lib
                    var libFuncArg = "(" + string.Join(",", libParameters.Select(_ => GetLIBHArg(_.Type, m) + " " + _.Name).ToArray()) + ")";

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
                        AddLIBH(GetLIBRet(returnType, m) + " " + methodName + libFuncArg + ";");
                    }

                    // lib cpp
                    if (isConstract)
                    {
                        AddLIBCPP(c.Name + "::" + c.Name + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("auto arg0 = self;");
                            var argConverters = libParameters.Select((_, i) => GetLIBCppArg(_.Type, i + 1, _.Name, m));
                            foreach (var a in argConverters)
                            {
                                AddLIBCPP(a);
                            }

                            AddLIBCPP("self = dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                            AddLIBCPP("isCtrlSelf = true;");
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    else if (isDestruct)
                    {
                        AddLIBCPP(c.Name + "::~" + c.Name + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("if (isCtrlSelf) {");
                            PushLIBCppIndent();
                            AddLIBCPP("dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select(_ => _.Name).ToArray()) + ");");
                            PopLIBCppIndent();
                            AddLIBCPP("}");
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    else
                    {
                        AddLIBCPP(returnType + " " + c.Name + "::" + methodName + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("auto arg0 = self;");
                            var argConverters = libParameters.Select((_, i) => GetLIBCppArg(_.Type, i + 1, _.Name, m));
                            foreach (var a in argConverters)
                            {
                                AddLIBCPP(a);
                            }

                            if (returnType.Name == "void")
                            {
                                AddLIBCPP("dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                            }
                            else
                            {
                                AddLIBCPP("return dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
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
