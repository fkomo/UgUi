﻿using System.IO;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Abstract;

namespace Ujeby.UgUi.Operations.IO
{
	[NodeInfo]
	public class FileExists : Check<string>
	{
		public override void Execute()
		{
			Output = !string.IsNullOrEmpty(Input) && File.Exists(Input);
		}
	}
}
