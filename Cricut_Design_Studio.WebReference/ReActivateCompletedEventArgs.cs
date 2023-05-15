using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Cricut_Design_Studio.WebReference
{
	[DebuggerStepThrough]
	[GeneratedCode("System.Web.Services", "2.0.50727.4016")]
	[DesignerCategory("code")]
	public class ReActivateCompletedEventArgs : AsyncCompletedEventArgs
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

		internal ReActivateCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
			: base(exception, cancelled, userState)
		{
			this.results = results;
		}
	}
}
