using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppWrapperGenerator
{

	/// <summary>
	/// Doxygenのxml解析
	/// </summary>
	class DoxygenParser
	{
		public List<EnumDef> EnumDefs = new List<EnumDef>();
		public List<ClassDef> ClassDefs = new List<ClassDef>();

		public ParseResult Result
		{
			get
			{
				return new ParseResult
				{
					EnumDefs = EnumDefs,
					ClassDefs = ClassDefs,
				};
			}
		}

		public void AddNamespaceFile(string path)
		{
			var doc = System.Xml.Linq.XDocument.Load(path);

			var doxygen = doc.Document.Elements("doxygen").FirstOrDefault();
			var compound = doxygen.Elements("compounddef").FirstOrDefault();
			var section_enum = compound.Elements("sectiondef").Where(_ => _.Attribute("kind") != null && _.Attribute("kind").Value == "enum").FirstOrDefault();

			// enum
			if (section_enum != null)
			{
				var enumdefs = section_enum.Elements().Where(_ => _.Attribute("kind") != null && _.Attribute("kind").Value == "enum");

				foreach (var enumdef in enumdefs)
				{
					var edef = new EnumDef();

					{
						// 名前
						edef.Name = enumdef.Element("name").Value;

						// 要約
						var briefdescription = enumdef.Element("briefdescription");
						if (briefdescription != null && briefdescription.Element("para") != null)
						{
							var para = briefdescription.Element("para");
							edef.Brief = para.Value;
						}
					}


					// メンバー
					var enumvalues = enumdef.Elements("enumvalue");
					foreach (var enumvalue in enumvalues)
					{
						var emd = new EnumMemberDef();

						// 名前
						emd.Name = enumvalue.Element("name").Value;

						// 要約
						var briefdescription = enumvalue.Element("briefdescription");
						if (briefdescription != null && briefdescription.Element("para") != null)
						{
							var para = briefdescription.Element("para");
							emd.Brief = para.Value;
						}

						edef.Members.Add(emd);
					}

					EnumDefs.Add(edef);
				}
			}
		}

		public void AddClassFiles(string[] pathes)
		{
			foreach(var path in pathes)
			{
				var classDef = new ClassDef();
				var doc = XDocument.Load(path);
				var compounddef = doc.Element("doxygen").Element("compounddef");
				classDef.Name = compounddef.Element("compoundname").Value.Replace("asd::", "");

				var briefNode = compounddef.Element("briefdescription").Element("para");
				if(briefNode != null)
				{
					classDef.Brief = briefNode.Value;
				}

				var detailNode = compounddef.Element("detaileddescription").Element("para");
				if(detailNode != null)
				{
					var simplesect = detailNode.Element("simplesect");
					if(simplesect != null && simplesect.Attribute("kind").Value == "note")
					{
						classDef.Note = simplesect.Element("para").Value.Replace("<linebreak/>", "<br/>");
					}
				}

				var functions = compounddef.Elements("sectiondef")
					.Where(x => x.Attribute("kind") != null
						&& (x.Attribute("kind").Value == "protected-func" || x.Attribute("kind").Value == "public-func"))
					.SelectMany(x => x.Elements("memberdef"));

				foreach(var funcNode in functions)
				{
					classDef.Methods.Add(BuildMethod(funcNode));
				}

				ClassDefs.Add(classDef);
			}
		}

		private MethodDef BuildMethod(XElement funcNode)
		{
			var methodDef = new MethodDef();
			methodDef.Name = funcNode.Element("name").Value;

            methodDef.IsPublic = funcNode.Attribute("prot").Value == "public";

			methodDef.IsConst = funcNode.Attribute("const").Value == "yes";

			var typeNode = funcNode.Element("type");
            if (typeNode != null)
            {
                methodDef.ReturnType.Parse(typeNode.Value);
            }

            var briefNode = funcNode.Element("briefdescription").Element("para");
			if(briefNode != null)
			{
				methodDef.Brief = briefNode.Value;
			}

            List<Tuple<string, string>> argParams = new List<Tuple<string, string>>();

            var param_ = funcNode.Elements("param");
            if (param_ != null)
            {
                var params_array = param_.ToArray();
                
                foreach(var param in params_array)
                {
                    var type = param.Element("type").Value;
                    var declname = param.Element("declname")?.Value ?? string.Empty;
                    argParams.Add(Tuple.Create(type, declname));
                }
            }

            var para = funcNode.Element("detaileddescription").Element("para");
			if(para != null)
			{
				var simplesect = para.Elements("simplesect");
				foreach (var item in simplesect)
				{
					if (item.Attribute("kind").Value == "return")
					{
						methodDef.BriefOfReturn = item.Element("para").Value;
					}
					if (item.Attribute("kind").Value == "note")
					{
						methodDef.Note = item.Element("para").Value;
					}
				}

				var parameterlist = para.Element("parameterlist");
				if(parameterlist != null && parameterlist.Attribute("kind").Value == "param")
				{
					foreach(var parameterNode in parameterlist.Elements("parameteritem"))
					{
						methodDef.Parameters.Add(BuildParameter(parameterNode, argParams));
					}
				}
			}

			return methodDef;
		}

		private ParameterDef BuildParameter(XElement parameterNode, List<Tuple<string, string>> argParams)
		{
			var def = new ParameterDef();
			def.Name = parameterNode.Element("parameternamelist").Element("parametername").Value;
			def.Brief = parameterNode.Element("parameterdescription").Element("para").Value;

            var arg = argParams.FirstOrDefault(_ => _.Item2 == def.Name);

            if(arg != null)
            {
                def.Type.Parse(arg.Item1);
            }

            return def;
		}
	}
}
