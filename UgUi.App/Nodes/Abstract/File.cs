using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Abstract
{
	[NodeInfo(Abstract = true)]
	public class _File<T> : NodeOperationBase
	{
		protected string path = string.Empty;
		[Input(Order = 0, InputAnchor = true, Serializable = true)]
		public string Path
		{
			get { return path; }
			set { SetField(ref path, value, nameof(Path)); }
		}

		protected T toWrite;
		[Input(Order = 1, InputAnchor = true, AnchorOnly = true)]
		public T Write
		{
			get { return toWrite; }
			set { SetField(ref toWrite, value, nameof(Write)); }
		}

		[Output(AnchorOnly = true)]
		public T Read { get; protected set; }

		[Output(AnchorOnly = true)]
		public int Length { get; protected set; } = 0;

		public override string[] GetInputs()
		{
			return new string[]
			{ 
				Path,
				$"{ nameof(Write) }:{ toWrite?.ToString() }" ,
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{
				$"{ nameof(Read) }:{ Read?.ToString() }" ,
				$"{ nameof(Length) }:{ Length }" 
			};
		}
	}
}
