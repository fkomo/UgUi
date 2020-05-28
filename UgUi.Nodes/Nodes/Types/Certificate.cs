using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	//[FunctionInfo]
	//public class Certificate : Operation
	//{
	//	protected string path = string.Empty;
	//	[Input(Order = 0, InputAnchor = true)]
	//	public string Path
	//	{
	//		get { return path; }
	//		set { SetField(ref path, value, nameof(Path)); }
	//	}

	//	[Output(AnchorOnly = true)]
	//	public byte[] PublicKey { get; private set; }

	//	[Output(AnchorOnly = true)]
	//	public X509Certificate2 Output { get; private set; }

	//	public override void Execute()
	//	{
	//		try
	//		{
	//			PublicKey = new byte[] { };

	//			Output = new X509Certificate2(Path);
	//			PublicKey = Output.GetPublicKey();
	//		}
	//		catch (Exception ex)
	//		{
	//			Log.WriteLine(ex.ToString());
	//		}
	//	}

	//	public override string[] GetInputs()
	//	{
	//		return new string[]
	//		{
	//			Path,
	//		};
	//	}

	//	public override string[] GetOutputs()
	//	{
	//		return new string[]
	//		{
	//			//Value,
	//			//$"{ nameof(Length) }:{ Length.ToString() }",
	//		};
	//	}
	//}
}
