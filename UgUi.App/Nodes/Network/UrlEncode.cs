using System.Web;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Types;

namespace Ujeby.UgUi.Operations.Network
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
