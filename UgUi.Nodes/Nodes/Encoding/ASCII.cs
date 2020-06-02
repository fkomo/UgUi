
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo(DisplayName = "ASCII")]
	public class _ASCII : EncodingNode
	{
		public override void Execute()
		{
			if (InputBin?.Length > 0)
				OutputText = System.Text.Encoding.ASCII.GetString(InputBin);
			else
				OutputText = string.Empty;

			if (!string.IsNullOrEmpty(InputText))
				OutputBin = System.Text.Encoding.ASCII.GetBytes(InputText);
			else
				OutputBin = new byte[] { };
		}
	}
}
