using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using Cricut_Design_Studio.Properties;

namespace Cricut_Design_Studio.com.cricut.webservice
{
	[DebuggerStepThrough]
	[WebServiceBinding(Name = "VersionSoap", Namespace = "http://webservice.cricut.com/")]
	[GeneratedCode("System.Web.Services", "2.0.50727.4016")]
	[DesignerCategory("code")]
	public class Version : SoapHttpClientProtocol
	{
		private SendOrPostCallback GetCurrentVersionOperationCompleted;

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

		public event GetCurrentVersionCompletedEventHandler GetCurrentVersionCompleted;

		public Version()
		{
			Url = Settings.Default.Cricut_DesignStudio_com_cricut_webservice_Version;
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

		[SoapDocumentMethod("http://webservice.cricut.com/GetCurrentVersion", RequestNamespace = "http://webservice.cricut.com/", ResponseNamespace = "http://webservice.cricut.com/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
		public string GetCurrentVersion(string sProductSKU)
		{
			object[] array = Invoke("GetCurrentVersion", new object[1] { sProductSKU });
			return (string)array[0];
		}

		public void GetCurrentVersionAsync(string sProductSKU)
		{
			GetCurrentVersionAsync(sProductSKU, null);
		}

		public void GetCurrentVersionAsync(string sProductSKU, object userState)
		{
			if (GetCurrentVersionOperationCompleted == null)
			{
				GetCurrentVersionOperationCompleted = OnGetCurrentVersionOperationCompleted;
			}
			InvokeAsync("GetCurrentVersion", new object[1] { sProductSKU }, GetCurrentVersionOperationCompleted, userState);
		}

		private void OnGetCurrentVersionOperationCompleted(object arg)
		{
			if (this.GetCurrentVersionCompleted != null)
			{
				InvokeCompletedEventArgs invokeCompletedEventArgs = (InvokeCompletedEventArgs)arg;
				this.GetCurrentVersionCompleted(this, new GetCurrentVersionCompletedEventArgs(invokeCompletedEventArgs.Results, invokeCompletedEventArgs.Error, invokeCompletedEventArgs.Cancelled, invokeCompletedEventArgs.UserState));
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
