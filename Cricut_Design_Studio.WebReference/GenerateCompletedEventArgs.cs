using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Cricut_Design_Studio.WebReference
{
	[GeneratedCode("System.Web.Services", "2.0.50727.4016")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class GenerateCompletedEventArgs : AsyncCompletedEventArgs
	{
		private object[] results;

		public int Result
		{
			get
			{
				RaiseExceptionIfNecessary();
				return (int)results[0];
			}
		}

		internal GenerateCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
			: base(exception, cancelled, userState)
		{
			this.results = results;
		}
	}
}
