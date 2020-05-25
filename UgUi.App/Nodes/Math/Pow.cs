
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Operations.Abstract;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Math
{
	[NodeInfo]
	public class Pow : UnaryOperator<v4>
	{
		protected double power = 2.0;
		[Input(Order = 1, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double Power
		{
			get { return power; }
			set { SetField(ref power, value, nameof(Power)); }
		}

		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Pow(input.X, Power), 
					System.Math.Pow(input.Y, Power),
					System.Math.Pow(input.Z, Power), 
					System.Math.Pow(input.W, Power));
		}
	}
}
