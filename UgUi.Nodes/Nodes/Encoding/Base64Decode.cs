using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(EncodingNode.InputBin))]
	[IgnoredProperty(Name = nameof(EncodingNode.OutputText))]
	public class Base64Decode : EncodingNode
	{
		public override void Execute()
		{
			OutputBin = System.Convert.FromBase64String(InputText);
		}
	}
}
 