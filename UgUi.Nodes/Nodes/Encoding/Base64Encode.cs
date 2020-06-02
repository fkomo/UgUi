using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(EncodingNode.InputText))]
	[IgnoredProperty(Name = nameof(EncodingNode.OutputBin))]
	public class Base64Encode : EncodingNode
	{
		public override void Execute()
		{
			OutputText = System.Convert.ToBase64String(InputBin);
		}
	}
}
