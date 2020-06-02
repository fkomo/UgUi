
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo(DisplayName = "Unicode")]
	public class _Unicode : EncodingNode
	{
		public override void Execute()
		{
			if (InputBin?.Length > 0)
				OutputText = System.Text.Encoding.UTF8.GetString(InputBin);
			else
				OutputText = string.Empty;

			if (!string.IsNullOrEmpty(InputText))
				OutputBin = System.Text.Encoding.UTF8.GetBytes(InputText);
			else
				OutputBin = new byte[] { };
		}
	}
}
