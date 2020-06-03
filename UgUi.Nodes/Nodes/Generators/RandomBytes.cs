using System.Security.Cryptography;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo]
	public class RandomBytes : NodeBase
	{
		protected int length = 0;
		[Input(Order = 0, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public int Length
		{
			get { return length; }
			set { SetField(ref length, value, nameof(Length)); }
		}

		[Output(AnchorOnly = true, DisplayName = "Bin")]
		public byte[] Value { get; set; }

		public override void Execute()
		{
			Value = new byte[Length];
			using (var rngcsp = new RNGCryptoServiceProvider())
				rngcsp.GetBytes(Value);
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Length.ToString(), 
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				//Value
			};
		}
	}
}
