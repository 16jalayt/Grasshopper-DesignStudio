using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Cricut_Design_Studio.WebReference
{
	[GeneratedCode("System.Web.Services", "2.0.50727.4016")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	public class RegisterCompletedEventArgs : AsyncCompletedEventArgs
	{
		private object[] results;

		public string Result
		{
			get
			{
				RaiseExceptionIfNecessary();
				return (string)results[0];
			}
		}

		internal RegisterCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
			: base(exception, cancelled, userState)
		{
			this.results = results;
		}
	}
}
