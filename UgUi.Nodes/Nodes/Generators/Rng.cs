using System;
using System.Security.Cryptography;
using Ujeby.Common.Tools.Types;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo]
	public class Rng : NodeBase
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

		protected int count = 1;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int Count
		{
			get { return count; }
			set { SetField(ref count, value, nameof(Count)); }
		}

		protected v4[] value;
		[Output(AnchorOnly = true, ReadOnly = true, DisplayName = "")]
		public v4[] Value { get; private set; }

		protected Random random = new Random(new RNGCryptoServiceProvider().GetHashCode());

		public override void Execute()
		{
			Value = new v4[Count];
			for (var i = 0; i < count; ++i)
				Value[i] = new v4(
					random.NextDouble(), 
					random.NextDouble(), 
					random.NextDouble(), 
					random.NextDouble());
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Seed,
				$"{ nameof(Count) }:{ Count.ToString() }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				//Value.ToString() 
			};
		}
	}
}
