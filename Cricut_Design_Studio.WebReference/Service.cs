using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using Cricut_Design_Studio.Properties;

namespace Cricut_Design_Studio.WebReference
{
	[DebuggerStepThrough]
	[WebServiceBinding(Name = "ServiceSoap", Namespace = "http://tempuri.org/")]
	[GeneratedCode("System.Web.Services", "2.0.50727.4016")]
	[DesignerCategory("code")]
	public class Service : SoapHttpClientProtocol
	{
		private SendOrPostCallback GenerateOperationCompleted;

		private SendOrPostCallback RegisterOperationCompleted;

		private SendOrPostCallback ReActivateOperationCompleted;

		private SendOrPostCallback ValidateOperationCompleted;

		private bool useDefaultCredentialsSetExplicitly;

		public new string Url
		{
			get
			{
				return base.Url;
			}
			set
			{
				if (IsLocalFileSystemWebService(base.Url) && !useDefaultCredentialsSetExplicitly && !IsLocalFileSystemWebService(value))
				{
					base.UseDefaultCredentials = false;
				}
				base.Url = value;
			}
		}

		public new bool UseDefaultCredentials
		{
			get
			{
				return base.UseDefaultCredentials;
			}
			set
			{
				base.UseDefaultCredentials = value;
				useDefaultCredentialsSetExplicitly = true;
			}
		}

		public event GenerateCompletedEventHandler GenerateCompleted;

		public event RegisterCompletedEventHandler RegisterCompleted;

		public event ReActivateCompletedEventHandler ReActivateCompleted;

		public event ValidateCompletedEventHandler ValidateCompleted;

		public Service()
		{
			Url = Settings.Default.Cricut_DesignStudio_WebReference_Service;
			if (IsLocalFileSystemWebService(Url))
			{
				UseDefaultCredentials = true;
				useDefaultCredentialsSetExplicitly = false;
			}
			else
			{
				useDefaultCredentialsSetExplicitly = true;
			}
		}

		[SoapDocumentMethod("http://tempuri.org/Generate", RequestNamespace = "http://tempuri.org/", ResponseNamespace = "http://tempuri.org/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
		public int Generate(int NumberOfKeys)
		{
			object[] array = Invoke("Generate", new object[1] { NumberOfKeys });
			return (int)array[0];
		}

		public void GenerateAsync(int NumberOfKeys)
		{
			GenerateAsync(NumberOfKeys, null);
		}

		public void GenerateAsync(int NumberOfKeys, object userState)
		{
			if (GenerateOperationCompleted == null)
			{
				GenerateOperationCompleted = OnGenerateOperationCompleted;
			}
			InvokeAsync("Generate", new object[1] { NumberOfKeys }, GenerateOperationCompleted, userState);
		}

		private void OnGenerateOperationCompleted(object arg)
		{
			if (this.GenerateCompleted != null)
			{
				InvokeCompletedEventArgs invokeCompletedEventArgs = (InvokeCompletedEventArgs)arg;
				this.GenerateCompleted(this, new GenerateCompletedEventArgs(invokeCompletedEventArgs.Results, invokeCompletedEventArgs.Error, invokeCompletedEventArgs.Cancelled, invokeCompletedEventArgs.UserState));
			}
		}

		[SoapDocumentMethod("http://tempuri.org/Register", RequestNamespace = "http://tempuri.org/", ResponseNamespace = "http://tempuri.org/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
		public string Register(string FirstName, string LastName, string Email, string SixPartkey, string MACAddress)
		{
			object[] array = Invoke("Register", new object[5] { FirstName, LastName, Email, SixPartkey, MACAddress });
			return (string)array[0];
		}

		public void RegisterAsync(string FirstName, string LastName, string Email, string SixPartkey, string MACAddress)
		{
			RegisterAsync(FirstName, LastName, Email, SixPartkey, MACAddress, null);
		}

		public void RegisterAsync(string FirstName, string LastName, string Email, string SixPartkey, string MACAddress, object userState)
		{
			if (RegisterOperationCompleted == null)
			{
				RegisterOperationCompleted = OnRegisterOperationCompleted;
			}
			InvokeAsync("Register", new object[5] { FirstName, LastName, Email, SixPartkey, MACAddress }, RegisterOperationCompleted, userState);
		}

		private void OnRegisterOperationCompleted(object arg)
		{
			if (this.RegisterCompleted != null)
			{
				InvokeCompletedEventArgs invokeCompletedEventArgs = (InvokeCompletedEventArgs)arg;
				this.RegisterCompleted(this, new RegisterCompletedEventArgs(invokeCompletedEventArgs.Results, invokeCompletedEventArgs.Error, invokeCompletedEventArgs.Cancelled, invokeCompletedEventArgs.UserState));
			}
		}

		[SoapDocumentMethod("http://tempuri.org/ReActivate", RequestNamespace = "http://tempuri.org/", ResponseNamespace = "http://tempuri.org/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
		public string ReActivate(string SixPartkey, string MACAddress)
		{
			object[] array = Invoke("ReActivate", new object[2] { SixPartkey, MACAddress });
			return (string)array[0];
		}

		public void ReActivateAsync(string SixPartkey, string MACAddress)
		{
			ReActivateAsync(SixPartkey, MACAddress, null);
		}

		public void ReActivateAsync(string SixPartkey, string MACAddress, object userState)
		{
			if (ReActivateOperationCompleted == null)
			{
				ReActivateOperationCompleted = OnReActivateOperationCompleted;
			}
			InvokeAsync("ReActivate", new object[2] { SixPartkey, MACAddress }, ReActivateOperationCompleted, userState);
		}

		private void OnReActivateOperationCompleted(object arg)
		{
			if (this.ReActivateCompleted != null)
			{
				InvokeCompletedEventArgs invokeCompletedEventArgs = (InvokeCompletedEventArgs)arg;
				this.ReActivateCompleted(this, new ReActivateCompletedEventArgs(invokeCompletedEventArgs.Results, invokeCompletedEventArgs.Error, invokeCompletedEventArgs.Cancelled, invokeCompletedEventArgs.UserState));
			}
		}

		[SoapDocumentMethod("http://tempuri.org/Validate", RequestNamespace = "http://tempuri.org/", ResponseNamespace = "http://tempuri.org/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
		public int Validate(string Activationkey, string MAC, ref int value)
		{
			object[] array = Invoke("Validate", new object[3] { Activationkey, MAC, value });
			value = (int)array[1];
			return (int)array[0];
		}

		public void ValidateAsync(string Activationkey, string MAC, int value)
		{
			ValidateAsync(Activationkey, MAC, value, null);
		}

		public void ValidateAsync(string Activationkey, string MAC, int value, object userState)
		{
			if (ValidateOperationCompleted == null)
			{
				ValidateOperationCompleted = OnValidateOperationCompleted;
			}
			InvokeAsync("Validate", new object[3] { Activationkey, MAC, value }, ValidateOperationCompleted, userState);
		}

		private void OnValidateOperationCompleted(object arg)
		{
			if (this.ValidateCompleted != null)
			{
				InvokeCompletedEventArgs invokeCompletedEventArgs = (InvokeCompletedEventArgs)arg;
				this.ValidateCompleted(this, new ValidateCompletedEventArgs(invokeCompletedEventArgs.Results, invokeCompletedEventArgs.Error, invokeCompletedEventArgs.Cancelled, invokeCompletedEventArgs.UserState));
			}
		}

		public new void CancelAsync(object userState)
		{
			base.CancelAsync(userState);
		}

		private bool IsLocalFileSystemWebService(string url)
		{
			if (url == null || url == string.Empty)
			{
				return false;
			}
			Uri uri = new Uri(url);
			if (uri.Port >= 1024 && string.Compare(uri.Host, "localHost", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			return false;
		}
	}
}
