using System.Globalization;

namespace Ujeby.UgUi.Nodes.Constants
{
	[NodeInfo(DisplayName = "PI")]
	public class _PI : NodeBase
	{
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public double Value
		{
			get { return System.Math.PI; }
			set { OnPropertyChanged(nameof(Value)); }
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value.ToString("F4", CultureInfo.InvariantCulture) };
		}
	}
}
