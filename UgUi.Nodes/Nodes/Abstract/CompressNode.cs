
using System.IO;

namespace Ujeby.UgUi.Nodes.Abstract
{
	[NodeInfo(Abstract = true)]
	public class CompressNode : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, DisplayName = "Src", AnchorOnly = true)]
		public byte[] Input { get; set; }

		[Output(AnchorOnly = true, DisplayName = "Dest")]
		public byte[] Output { get; set; }

		[Output(AnchorOnly = true)]
		public int Length { get { return Output?.Length ?? 0; } }

		public override string[] GetInputs()
		{
			return new string[]
			{
				//Input,
				$"{ nameof(Input) }.{ nameof(Length) }:{ (Input?.Length ?? 0).ToString() }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				//Output,
				$"{ nameof(Output) }.{ nameof(Length) }:{ Length.ToString() }",
			};
		}

		protected static void CopyTo(Stream source, Stream destination)
		{
			var copyBuffer = new byte[4096];

			int count;
			while ((count = source.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
				destination.Write(copyBuffer, 0, count);
		}
	}
}
