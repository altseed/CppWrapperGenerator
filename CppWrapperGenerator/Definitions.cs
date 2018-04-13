using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWrapperGenerator
{
    class BuildIn
    {
        public static string[] PrimitiveType =
        {
            "int",
            "int32_t",
            "void",
            "float",
            "bool",
			"char16_t",
        };
    }

    class TypeDef
    {
        public string Name = string.Empty;
        public bool IsSharedPtr = false;
		public bool IsVector = false;
        public bool IsPrimitiveType = false;
		public bool IsConst = false;
		public bool IsRef = false;
		public bool IsPointer = false;

		public void Parse(string original)
		{
			if (original.Contains("std::shared_ptr"))
			{
				IsSharedPtr = true;
				original = original.Replace("std::shared_ptr", "").Replace("<", "").Replace(">", "").Replace(" ", "");
			}

			if (original.Contains("const"))
			{
				IsConst = true;
				original = original.Replace("const", "");
			}

			if (original.Contains("*"))
			{
				IsPointer = true;
				original = original.Replace("*", "");
			}

			if (original.Contains("*"))
			{
				IsPointer = true;
				original = original.Replace("*", "");
			}

			if (original.Contains("&"))
			{
				IsRef = true;
				original = original.Replace("&", "");
			}

			if (original.Contains("std::vector"))
			{
				IsVector = true;
				original = original.Replace("std::vector", "").Replace("<", "").Replace(">", "").Replace(" ", "");
			}

			original = original.Trim(' ');

			Name = original;

			foreach (var type in BuildIn.PrimitiveType)
			{
				if (Name.Contains(type))
				{
					IsPrimitiveType = true;
				}
			}
		}

		public override string ToString()
		{
			var ret = Name;

			if(IsVector)
			{
				ret = string.Format("std::vector<{0}>", ret);
			}

			if (IsConst)
			{
				ret = string.Format("const {0}", ret);
			}

			if (IsPointer)
			{
				ret = string.Format("{0}*", ret);
			}

			if (IsRef)
			{
				ret = string.Format("{0}&", ret);
			}

			if (IsSharedPtr)
			{
				ret = string.Format("std::shared_ptr<{0}>", ret);
			}

			return ret;
		}
    }

	class EnumDef
	{
		public string Name = string.Empty;
		public string Brief = string.Empty;
		public List<EnumMemberDef> Members = new List<EnumMemberDef>();
	}

	class EnumMemberDef
	{
		public string Name = string.Empty;
		public string Brief = string.Empty;
	}

	class ClassDef
	{
		public string Name = string.Empty;
		public string Brief = string.Empty;
		public string Note = null;
		public List<MethodDef> Methods = new List<MethodDef>();
		public List<PropertyDef> Properties = new List<PropertyDef>();
		public bool CoreIsPrivate { get; set; }

		public override string ToString()
		{
			return string.Format("ClassDef {0}, Method x {1}", Name, Methods.Count);
		}
	}

	class MethodDef
	{
		public bool IsStatic = false;
        public bool IsPublic = false;
		public bool IsConst = false;
		public string Name = string.Empty;
		public string Brief = string.Empty;
        public TypeDef ReturnType = new TypeDef();
		public string BriefOfReturn = string.Empty;
		public bool ReturnIsEnum = false;
		public string Note = null;
		public List<ParameterDef> Parameters = new List<ParameterDef>();

		public override string ToString()
		{
			return string.Format("MethodDef {0}, Parameters x {1}", Name, Parameters.Count);
		}
	}

	class ParameterDef
	{
        public TypeDef Type = new TypeDef();
		public string CoreType = string.Empty;
		public string Name = string.Empty;
		public string Brief = string.Empty;
		public bool IsRef = false;
		public bool IsEnum = false;
		public bool IsWrappingObject = false;

		public override string ToString()
		{
			return string.Format("ParameterDef {0} {1}", Type, Name);
		}
	}

	class PropertyDef
	{
		public string Type = string.Empty;
		public string Name = string.Empty;
		public bool HaveGetter = false;
		public bool HaveSetter = false;
		public string Brief = string.Empty;
		public bool IsRefForSet = false;
		public bool IsEnum = false;
	}
}
