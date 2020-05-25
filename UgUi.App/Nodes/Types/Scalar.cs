using System.Globalization;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Types
{
	[NodeInfo]
	public class Scalar : NodeOperationBase
	{
		private double x = 0.0;
		[Input(Order = 0, InputAnchor = true, DisplayName = "", OutputAnchor = true, Serializable = true)]
		public double X
		{
			get { return x; }
			set { SetField(ref x, value, nameof(X)); }
		}

		public override string[] GetOutputs()
		{
			return new string[] { X.ToString("F4", CultureInfo.InvariantCulture) };
		}
	}
}
