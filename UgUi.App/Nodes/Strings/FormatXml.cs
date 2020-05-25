using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Types;

namespace Ujeby.UgUi.Operations.Strings
{
	[NodeInfo]
	public class FormatXml : _String
	{
		public override void Execute()
		{
			var newValue = Ujeby.Common.Tools.Utils.GetFormattedXml(input);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
