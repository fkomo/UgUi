using System.Web;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Types;

namespace Ujeby.UgUi.Nodes.Network
{
	[NodeInfo]
	public class UrlEncode : _String
	{
		public override void Execute()
		{
			var newValue = HttpUtility.UrlEncode(input ?? string.Empty);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
