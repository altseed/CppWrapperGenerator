using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWrapperGenerator
{
	class ParseResult
	{
		public IList<ClassDef> ClassDefs { get; set; }
		public IList<EnumDef> EnumDefs { get; set; }
	}
}
