using System;
using System.Globalization;
using System.Security.Cryptography;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Generators
{
	[NodeInfo]
	public class Rng : NodeOperationBase
	{
		protected string seed = string.Empty;
		[Input(Order = 0, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public string Seed
		{
			get { return seed; }
			set 
			{
				SetField(ref seed, value, nameof(Seed));
				random = new Random(!string.IsNullOrEmpty(Seed) ? Seed.GetHashCode() : new RNGCryptoServiceProvider().GetHashCode());
			}
		}

		protected double value = 0.0;
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public double Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		protected Random random = new Random(new RNGCryptoServiceProvider().GetHashCode());

		public override void Execute()
		{
			Value = random.NextDouble();
		}

		public override string[] GetInputs()
		{
			return new string[] { Seed };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value.ToString("F4", CultureInfo.InvariantCulture) };
		}
	}
}
