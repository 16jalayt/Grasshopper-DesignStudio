using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cricut_Design_Studio.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "8.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;

		[SpecialSetting(SpecialSetting.WebServiceUrl)]
		[DebuggerNonUserCode]
		[ApplicationScopedSetting]
		[DefaultSettingValue("http://webservice.cricut.com/service.asmx")]
		public string Cricut_DesignStudio_WebReference_Service => (string)this["Cricut_DesignStudio_WebReference_Service"];

		[DebuggerNonUserCode]
		[SpecialSetting(SpecialSetting.WebServiceUrl)]
		[ApplicationScopedSetting]
		[DefaultSettingValue("http://webservice.cricut.com/Version.asmx")]
		public string Cricut_DesignStudio_com_cricut_webservice_Version => (string)this["Cricut_DesignStudio_com_cricut_webservice_Version"];
	}
}
