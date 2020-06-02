using System;
using System.Runtime.Serialization;
using System.Windows;

namespace Ujeby.UgUi.Core
{
	[DataContract]
	internal class UgUiFile
	{
		public const string Extension = ".xml";

		[DataContract]
		internal class Connection
		{
			[DataMember]
			public Guid LeftId { get; set; }
			[DataMember]
			public Guid RightId { get; set; }
			[DataMember]
			public string LeftAnchorName { get; set; }
			[DataMember]
			public string RightAnchorName { get; set; }
		}

		[DataContract]
		internal class Node
		{
			[DataMember]
			public Guid Id { get; set; }
			[DataMember]
			public string TypeName { get; set; }
			[DataMember]
			public Point Position { get; set; }
			[DataMember]
			public string Data { get; set; }
			[DataMember]
			public string Name { get; set; }
		}

		[DataMember]
		public Connection[] Connections { get; set; }

		[DataMember]
		public Node[] Nodes { get; set; }
	}
}
