using System.Web;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Types;

namespace Ujeby.UgUi.Nodes.Network
{
	[NodeInfo]
	public class UrlDecode : _String
	{
		public override void Execute()
		{
			var newValue = HttpUtility.UrlDecode(input ?? string.Empty);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
 