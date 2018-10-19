using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CppWrapperGenerator
{
	[DataContract]
	class Settings
	{
		[DataMember]
		public string DoxygenXmlDirPath { get; set; }

        [DataMember]
		public string ExportDLLDirPath { get; set; }

        [DataMember]
        public string ExportLibDirPath { get; set; }

        [DataMember]
        public string[] ClassWhiteList { get; set; }

        [DataMember]
		public string[] ReleasableClassList { get; set; }

        [DataMember]
        public string[] UnwrappedClassList { get; set; }

		[DataMember]
		public string[] PrimitiveTypeList { get; set; }
    }
}
