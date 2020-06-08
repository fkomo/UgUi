using System;
using System.Security.Cryptography.X509Certificates;
using Ujeby.Common.Tools;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	public class Certificate : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, OutputAnchor = true, DisplayName = "")]
		public X509Certificate2 Input { get; set; }

		private string password = null;
		[Input(Order = 1, InputAnchor = true)]
		public string Password 
		{ 
			get { return password; }
			set { SetField(ref password, value, nameof(Password)); }
		}

		[Output(AnchorOnly = true)]
		public string Subject { get; set; }

		[Output(AnchorOnly = true)]
		public string Issuer { get; set; }

		[Output(AnchorOnly = true)]
		public string SerialNumber { get; set; }

		[Output(AnchorOnly = true)]
		public DateTime? NotBefore { get; set; }

		[Output(AnchorOnly = true)]
		public DateTime? NotAfter { get; set; }

		[Output(AnchorOnly = true)]
		public string ThumbPrint { get; set; }

		//[Output(AnchorOnly = true)]
		//public byte[] PublicKey { get; private set; }

		//[Output(AnchorOnly = true)]
		//public byte[] PrivateKey { get; private set; }

		public override void Execute()
		{
			try
			{
				//string certificateText = File.ReadAllText("certificate_pub.crt");
				//string privateKeyText = File.ReadAllText("private.key");
				//ICertificateProvider provider = new CertificateFromFileProvider(certificateText, privateKeyText);
				//var certificate = provider.Certificate;

				//string privateKeyText = File.ReadAllText("private.key");
				//IOpenSSLPrivateKeyDecoder decoder = new OpenSSLPrivateKeyDecoder();
				//RSACryptoServiceProvider cryptoServiceProvider = decoder.Decode(privateKeyText);
				//// Example: sign the data
				//byte[] hello = new UTF8Encoding().GetBytes("Hello World");
				//byte[] hashValue = cryptoServiceProvider.SignData(hello, CryptoConfig.MapNameToOID("SHA256"));


				if (Input != null)
				{
					Issuer = Input.Issuer;
					Subject = Input.Subject;
					SerialNumber = Input.SerialNumber;
					NotBefore = Input.NotBefore;
					NotAfter = Input.NotAfter;
					ThumbPrint = Input.Thumbprint;
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
				//Path,
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
	}
}
