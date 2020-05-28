using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(Vector2.XY))]
	[IgnoredProperty(Name = nameof(Vector3.XYZ))]
	[IgnoredProperty(Name = nameof(Vector2.Length))]
	public class Vector4 : Vector3
	{
		protected double w = 0.0;
		[Input(Order = 4, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double W
		{
			get { return w; }
			set { SetField(ref w, value, nameof(W)); }
		}

		[Input(Order = 5, InputAnchor = true, AnchorOnly = true, OutputAnchor = true)]
		public v4 XYZW
		{
			get { return new v4(x, y, z, w); }
			set
			{
				X = value?.X ?? 0.0;
				Y = value?.Y ?? 0.0;
				Z = value?.Z ?? 0.0;
				W = value?.W ?? 0.0;
			}
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				XYZW.ToString(),
			};
		}
	}
}
