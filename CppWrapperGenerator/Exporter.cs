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
		List<string> lib_internalHeaderText = new List<string>();

		int dll_h_indent = 0;
        int dll_cpp_indent = 0;
        int lib_h_indent = 0;
        int lib_cpp_indent = 0;
		int lib_internal_h_indent = 0;

        List<Func<TypeDef, MethodDef, string>> dll_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, MethodDef, string>> dll_h_arg_rules = new List<Func<TypeDef, MethodDef, string>>();
		List<Func<TypeDef, MethodDef, string>> dll_cpp_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
		List<Func<TypeDef, int, string, MethodDef, string>> dll_cpp_arg_rules = new List<Func<TypeDef, int, string, MethodDef, string>>();

        List<Func<TypeDef, MethodDef, string>> lib_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
        List<Func<TypeDef, MethodDef, string>> lib_h_arg_rules = new List<Func<TypeDef, MethodDef, string>>();
		List<Func<TypeDef, MethodDef, string>> lib_cpp_ret_rules = new List<Func<TypeDef, MethodDef, string>>();
		List<Func<TypeDef, int, string, MethodDef, string>> lib_cpp_arg_rules = new List<Func<TypeDef, int, string, MethodDef, string>>();

		List<Func<TypeDef, bool, string>> dll_destructor_rules = new List<Func<TypeDef, bool, string>>();

		string dllClassName = "WrapperDLL";

		public string[] ReleasableClasses = new string[0];

        public string[] UnwrappedClasses = new string[0];

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

		string GetDLLCppRet(TypeDef t, MethodDef m)
		{
			foreach (var f in dll_cpp_ret_rules)
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

		string GetLIBCppRet(TypeDef t, MethodDef m)
		{
			foreach (var f in lib_cpp_ret_rules)
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

		string GetDLLDestructor(TypeDef t, bool isDestructorPrivate)
		{
			foreach (var f in dll_destructor_rules)
			{
				var r = f(t, isDestructorPrivate);
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
                        return t.ToString();
                    }
                );

                dll_h_arg_rules.Add(
                    (t, m) =>
                    {
                        if (t.Name != pt) return null;
                        return t.ToString();
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
                        return t.ToString();
					}
                );

                lib_h_arg_rules.Add(
                    (t, m) =>
                    {
                        if (t.Name != pt) return null;
                        return t.ToString();
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

			// enum
			{
				dll_ret_rules.Add(
					(t, m) =>
					{
						if (!doxygen.EnumDefs.Any(_=>_.Name == t.Name)) return null;
						return t.ToString();
					}
				);

				dll_h_arg_rules.Add(
					(t, m) =>
					{
						if (!doxygen.EnumDefs.Any(_ => _.Name == t.Name)) return null;
						return t.ToString();
					}
				);

				dll_cpp_arg_rules.Add(
					(t, i, s, m) =>
					{
						if (!doxygen.EnumDefs.Any(_ => _.Name == t.Name)) return null;
						return string.Format("auto arg{0} = {1};", i, s);
					}
				);

				lib_ret_rules.Add(
					(t, m) =>
					{
						if (!doxygen.EnumDefs.Any(_ => _.Name == t.Name)) return null;
						return t.ToString();
					}
				);

				lib_h_arg_rules.Add(
					(t, m) =>
					{
						if (!doxygen.EnumDefs.Any(_ => _.Name == t.Name)) return null;
						return t.ToString();
					}
				);

				lib_cpp_arg_rules.Add(
					(t, i, s, m) =>
					{
						if (!doxygen.EnumDefs.Any(_ => _.Name == t.Name)) return null;
						return string.Format("auto arg{0} = {1};", i, s);
					}
				);
			}

			// vector
			{
				dll_ret_rules.Add(
					(t, m) =>
					{
						if (!t.IsVector) return null;
						return t.ToString();
					}
				);

				dll_h_arg_rules.Add(
					(t, m) =>
					{
						if (!t.IsVector) return null;
						return t.ToString();
					}
				);

				dll_cpp_arg_rules.Add(
					(t, i, s, m) =>
					{
						if (!t.IsVector) return null;
						if(t.IsRef)
						{
							return string.Format("auto& arg{0} = {1};", i, s);
						}
						else
						{
							return string.Format("auto arg{0} = {1};", i, s);
						}
					}
				);

				lib_ret_rules.Add(
					(t, m) =>
					{
						if (!t.IsVector) return null;
						return t.ToString();
					}
				);

				lib_h_arg_rules.Add(
					(t, m) =>
					{
						if (!t.IsVector) return null;
						return t.ToString();
					}
				);

				lib_cpp_arg_rules.Add(
					(t, i, s, m) =>
					{
						if (!t.IsVector) return null;
						if(t.IsRef)
						{
							return string.Format("auto& arg{0} = {1};", i, s);
						}
						else
						{
							return string.Format("auto arg{0} = {1};", i, s);
						}
					}
				);
			}

			// Special create rules
			{
				lib_ret_rules.Add(
					(t, m) =>
					{
						if (!m.Name.StartsWith("Create")) return null;
						return string.Format("std::shared_ptr<{0}>", m.ReturnType.Name);
					}
				);

				lib_cpp_ret_rules.Add(
					(t, m) =>
					{
						if (!m.Name.StartsWith("Create")) return null;
						return string.Format("return std::shared_ptr<{0}>( new {0}(ret, true) );", t.Name);
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

				dll_cpp_ret_rules.Add(
					(t, m) =>
					{
						return string.Format("return ret;");
					}
				);

				dll_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        return string.Format("auto arg{0} = ({1}*){2};", i, t.Name, s);
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

				lib_cpp_ret_rules.Add(
					(t, m) =>
					{
						return string.Format("return ret;");
					}
				);

				lib_cpp_arg_rules.Add(
                    (t, i, s, m) =>
                    {
                        if(UnwrappedClasses.Contains(t.Name))
                        {
                            return string.Format("auto arg{0} = {1}.get();", i, s);
                        }
                        return string.Format("auto arg{0} = {1}.get()->self;", i, s);
                    }
                );

				dll_destructor_rules.Add(
					(t, b) =>
					{
						// TODO : Generarize
						if(ReleasableClasses.Contains(t.Name))
						{
							return "SafeRelease(self_);";
						}

						if(b)
						{
							return null;
						}
						else
						{
							return string.Format("delete self_;", t.Name);
						}
					}
				);

			}
		}

		public void Export(string libDirPath, string dllDirPath)
		{
			string dllH_Header = @"
#pragma once

#include <stdio.h>
#include <stdint.h>
#include <memory>
#include <vector>

#include <asd.common.Base.h>
#include ""asd.Core.Base.h""

extern ""C""
{
	ASD_DLLEXPORT void* ASD_STDCALL CreateWrapperDLL();
	ASD_DLLEXPORT void ASD_STDCALL DeleteWrapperDLL(void* o);
}

namespace asd
{
";

			string dllH_Footer = @"
};
";

			string dllCpp_Header = @"
#include ""asd.WrapperDLL.h""

#include ""Sound/asd.Sound.h""
#include ""Sound/asd.SoundSource.h""

#include ""IO/asd.File.h""
#include ""IO/asd.StaticFile.h""
#include ""IO/asd.StreamFile.h""
#include ""Tool/asd.Tool.h""

namespace asd
{

";

			string dllCpp_Footer = @"
};

extern ""C""
{

ASD_DLLEXPORT void* ASD_STDCALL CreateWrapperDLL()
{
	return new asd::WrapperDLL();
}

ASD_DLLEXPORT void ASD_STDCALL DeleteWrapperDLL(void* o)
{
	auto o_ = (asd::WrapperDLL*)o;
	delete o_;
}

}

";
			string libH_Header = @"
#pragma once

#include <stdio.h>
#include <stdint.h>
#include <memory>
#include <vector>

#include ""asd.CoreToEngine.h""

namespace asd {

";
			string libH_Footer = @"
};
";

			string libCpp_Header = @"
#include ""asd.WrapperLib.h""
#include ""asd.WrapperLib.Internal.h""
#include ""asd.CoreToEngine.h""

namespace asd
{

static WrapperDLL* dll = nullptr;

void InitializeWrapper(CreateWrapperDLLFunc func) {
	dll = (WrapperDLL*)func();
};

void TerminateWrapper(DeleteWrapperDLLFunc func) {
	func(dll);
};

";
			string libCpp_Footer = @"
};
";

			string StreamFile_Additional = @"
/**
	@brief	指定したサイズ分、ファイルを読み込む。
	@param	buffer	出力先
	@param	size	読み込まれるサイズ
*/
void Read(std::vector<uint8_t>& buffer, int32_t size)
{
	auto result = Read(size);
	buffer.resize(result);
	memcpy(buffer.data(), GetTempBuffer(), result);
}
";

			AddDLLH(dllH_Header);

            AddDLLH("class " + dllClassName + " {");
            AddDLLH("public:");

            AddDLLCPP(dllCpp_Header);
            
            AddLIBH(libH_Header);
			AddLIBCPP(libCpp_Header);

            PushDLLHIndent();

			// predefined
			foreach (var c in doxygen.ClassDefs)
			{
				if (!settings.ClassWhiteList.Any(_ => _ == c.Name)) continue;
				AddLIBH("class " + c.Name + ";");
			}
			AddLIBH("class AutoGeneratedWrapperAccessor;");

			AddLIBH("");

			foreach (var c in doxygen.ClassDefs)
            {
                if (!settings.ClassWhiteList.Any(_ => _ == c.Name)) continue;

                var dllFuncPrefix = c.Name + "_";


				AddLIBH("/**");
				AddLIBH("\t@brief " + c.Brief);
				AddLIBH("*/");
				AddLIBH("class " + c.Name + " {");
                PushLIBHIndent();
                AddLIBH("void* self = nullptr;");
                AddLIBH("bool isCtrlSelf = false;");

				// friend class (TODO smart)
				foreach (var c2 in doxygen.ClassDefs)
				{
					if (!settings.ClassWhiteList.Any(_ => _ == c2.Name)) continue;
					
					AddLIBH("friend class " + c2.Name + ";");
				}

				AddLIBH("friend class AutoGeneratedWrapperAccessor;");

				// Default constructor
				if (!c.Methods.Any(m=> string.IsNullOrEmpty(m.ReturnType.Name) && !m.Name.Contains("~") && m.IsPublic))
				{
					AddLIBH(c.Name + "(void* self, bool isCtrlSelf);");
					AddLIBCPP(c.Name + "::" + c.Name + "(void* self, bool isCtrlSelf) {");
					AddLIBCPP("\tthis->self = self;");
					AddLIBCPP("\tthis->isCtrlSelf = isCtrlSelf;");
					AddLIBCPP("}");
				}

				bool isDestructorPrivate = c.Methods.Any(_ => _.Name.Contains("~") && !_.IsPublic);

				AddLIBH("public:");

                foreach (var m in c.Methods)
                {
                    if (m.IsStatic) continue;

					var isConstract = string.IsNullOrEmpty(m.ReturnType.Name) && !m.Name.Contains("~");
					var isDestruct = string.IsNullOrEmpty(m.ReturnType.Name) && m.Name.Contains("~");

					// A method note is none
					if (string.IsNullOrEmpty(m.Brief) && !isConstract && !isDestruct) continue;

					// A method whose note has a #Ignore is ignored.
					if (m.Note?.Contains("#Ignore") ?? false) continue;
					   
                    // A method which has an argument whose type is shared_ptr<T> is ignored.
                    if (m.Parameters.Any(_ => _.Type.IsSharedPtr)) continue;

                    // A method whose return type is shared_ptr<T> is ignored.
                    if (m.ReturnType.IsSharedPtr) continue;



					// non public functions are not valid without destructor
					if (!isDestruct && !m.IsPublic) continue;
					
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

						var f = GetDLLDestructor(new TypeDef() { Name = c.Name }, !m.IsPublic);

						if(f != "")
						{
							AddDLLCPP(f);
						}
					}
                    else
                    {
                        AddDLLCPP("auto self_ = (" + c.Name + "*)self;");

                        var argConverters = libParameters.Select((_, i) => GetDLLCppArg(_.Type, i, _.Name, m));
                        foreach (var a in argConverters)
                        {
                            AddDLLCPP(a);
                        }

                        if (returnType.Name == "void" && !returnType.IsPointer)
                        {
                            AddDLLCPP("self_->" + methodName + "(" + string.Join(",", libParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                        }
                        else
                        {
							var retType = "auto";
							if (returnType.IsRef) retType = "auto&";

                            AddDLLCPP(retType + " ret = self_->" + methodName + "(" + string.Join(",", libParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
							AddDLLCPP(GetDLLCppRet(returnType, m));
						}
					}

                    PopDLLCppIndent();

                    AddDLLCPP("};");
                    AddDLLCPP("");

                    // lib
                    var libFuncArg = "(" + string.Join(",", libParameters.Select(_ => GetLIBHArg(_.Type, m) + " " + _.Name).ToArray()) + ")";

					if(m.IsConst)
					{
						libFuncArg += " const";
					}

					// lib header

					AddLIBH("/**");
					AddLIBH("\t@brief " + m.Brief);

					foreach(var p in m.Parameters)
					{
						AddLIBH("\t@param " + p.Name + " " + p.Brief);
					}

					if(!string.IsNullOrEmpty(m.BriefOfReturn))
					{
						AddLIBH("\t@return " + m.BriefOfReturn);
					}

					AddLIBH("*/");

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
					AddLIBH("");

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
                        AddLIBCPP(GetLIBRet(returnType, m) + " " + c.Name + "::" + methodName + libFuncArg + "{");
                        PushLIBCppIndent();
                        {
                            AddLIBCPP("auto arg0 = self;");
                            var argConverters = libParameters.Select((_, i) => GetLIBCppArg(_.Type, i + 1, _.Name, m));
                            foreach (var a in argConverters)
                            {
                                AddLIBCPP(a);
                            }

                            if (returnType.Name == "void" && !returnType.IsPointer)
                            {
                                AddLIBCPP("dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
                            }
                            else
                            {
								var retType = "auto";
								if (returnType.IsRef) retType = "auto&";

								AddLIBCPP(retType + " ret = dll->" + dllFuncName + "(" + string.Join(",", dllParameters.Select((_, i) => "arg" + i).ToArray()) + ");");
								AddLIBCPP(GetLIBCppRet(returnType, m));
							}
                        }
                        PopLIBCppIndent();
                        AddLIBCPP("};");
                    }
                    
                    AddLIBCPP("");
                }

				if(c.Name == "StreamFile")
				{
					AddLIBH(StreamFile_Additional);
				}

                PopLIBHIndent();
                AddLIBH("};");
                AddLIBH("");
            }


            PopDLLHIndent();

			AddDLLH("};");

			AddDLLH(dllH_Footer);
			AddDLLCPP(dllCpp_Footer);
			AddLIBH(libH_Footer);
			AddLIBCPP(libCpp_Footer);
			
            System.IO.File.WriteAllLines(dllDirPath + "asd.WrapperDLL.h", dll_headerText.ToArray(), System.Text.Encoding.UTF8);
            System.IO.File.WriteAllLines(dllDirPath + "asd.WrapperDLL.cpp", dll_cppText.ToArray(), System.Text.Encoding.UTF8);
            System.IO.File.WriteAllLines(libDirPath + "asd.WrapperLib.h", lib_headerText.ToArray(), System.Text.Encoding.UTF8);
			System.IO.File.WriteAllLines(libDirPath + "asd.WrapperLib.cpp", lib_cppText.ToArray(), System.Text.Encoding.UTF8);
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

		void AddInternalLIBH(string str)
		{
			string s = string.Empty;
			for (int i = 0; i < lib_internal_h_indent; i++)
			{
				s += "\t";
			}

			s += str;

			lib_internalHeaderText.Add(s);
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

		void PushInternalLIBHIndent() { lib_internal_h_indent++; }
		void PopInternalLIBHIndent() { lib_internal_h_indent--; }

	}
}
