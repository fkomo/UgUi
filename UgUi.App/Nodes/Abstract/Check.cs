using System.IO;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Abstract
{
	[NodeInfo(Abstract = true)]
	public class Check<T> : NodeOperationBase
	{
		protected T input;
		[Input(Order = 0, InputAnchor = true, Serializable = true)]
		public T Input
		{
			get { return input; }
			set { SetField(ref input, value, nameof(Input)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public bool Output { get; protected set; } = false;

		public override string[] GetInputs()
		{
			return new string[] { Input.ToString() };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output.ToString() };
		}
	}
}
