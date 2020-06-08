using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Ujeby.Common.Tools;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo]
	public class SelfSignedCertificate : NodeBase
	{
		private string subject = null;
		[Input(Order = 0, InputAnchor = true, Serializable = true)]
		public string Subject
		{
			get { return subject; }
			set { SetField(ref subject, value, nameof(Subject)); }
		}

		private int expiration = 365;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int Expiration
		{
			get { return expiration; }
			set { SetField(ref expiration, value, nameof(Expiration)); }
		}

		private string password = null;
		[Input(Order = 2, InputAnchor = true, Serializable = true)]
		public string Password
		{
			get { return password; }
			set { SetField(ref password, value, nameof(Password)); }
		}

		private string sigAlg = "sha256";
		[Input(Order = 3, InputAnchor = true, Serializable = true)]
		public string SigAlg
		{
			get { return sigAlg; }
			set { SetField(ref sigAlg, value, nameof(SigAlg)); }
		}

		[Output(AnchorOnly = true)]
		public X509Certificate2 Certificate { get; private set; }

		[Output(AnchorOnly = true)]
		public byte[] PublicKey { get; private set; }

		[Output(AnchorOnly = true)]
		public byte[] PrivateKey { get; private set; }

		public override void Execute()
		{
			try
			{
				Certificate = null;
				PublicKey = PrivateKey = null;

				if (!string.IsNullOrEmpty(Subject) && Expiration > 0 && !string.IsNullOrEmpty(SigAlg))
				{
					Certificate = CreateSelfSignedCertificate(Expiration, Subject, SigAlg, Password, out byte[] privateKey, out byte[] publicKey);
					if (Certificate != null)
					{
						PublicKey = publicKey;
						PrivateKey = privateKey;
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				$"{ nameof(Subject) }:{ Subject }",
				$"{ nameof(Expiration) }:{ Expiration.ToString() }",
				$"{ nameof(SigAlg) }:{ SigAlg }",
				$"{ nameof(Password) }:{ Password }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				//Value,
				//$"{ nameof(Length) }:{ Length.ToString() }",
			};
		}

		private const string OpenSslPath = "openssl/openssl.exe";
		private const string OpenSslConfig = "openssl/config.cnf";
		private static string ExportDirectory { get { return System.IO.Path.Combine(UserDataFolder, "Certificates"); } }

		private static X509Certificate2 CreateSelfSignedCertificate(int expirationInDays, string subjectAndIssuer, string sigAlg, string pfxPassword, out byte[] privateKey, out byte[] publicKey)
		{
			privateKey = null;
			publicKey = null;

			if (!Directory.Exists(ExportDirectory))
				Directory.CreateDirectory(ExportDirectory);

			var path = System.IO.Path.Combine(ExportDirectory, subjectAndIssuer);

			RunOpenSsl($"req -x509 -nodes -days { expirationInDays } -newkey rsa:2048 -keyout { path }.key -out { path }.crt -{ sigAlg } -subj \"/CN={ subjectAndIssuer }\" -config { OpenSslConfig }");

			if (File.Exists(path + ".key") && File.Exists(path + ".crt"))
			{
				RunOpenSsl($"pkcs12 -export -out { path }.pfx -inkey { path }.key -in { path }.crt -password pass:{ pfxPassword }");

				if (File.Exists(path + ".pfx"))
				{
					privateKey = File.ReadAllBytes($"{ path }.key");
					publicKey = File.ReadAllBytes($"{ path }.crt");

					return new X509Certificate2(path + ".crt");
				}
			}

			return null;
		}

		private static void RunOpenSsl(string arguments)
		{
			var exeProcess = new Process() 
			{ 
				StartInfo = new ProcessStartInfo
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					FileName = OpenSslPath,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = arguments,
					RedirectStandardOutput = true,
				}
			};

			exeProcess.OutputDataReceived += (sender, args) => Log.WriteLine($"{ OpenSslPath } output: { args.Data }");
			exeProcess.Start();
			exeProcess.BeginOutputReadLine();
			exeProcess.WaitForExit();
		}
	}
}
