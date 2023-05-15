using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlphaFormTransformer;

namespace Cricut_Design_Studio
{
	public class SplashScreen : Form
	{
		private const int TIMER_INTERVAL = 50;

		private const string REG_KEY_INITIALIZATION = "Initialization";

		private const string REGVALUE_PB_MILISECOND_INCREMENT = "Increment";

		private const string REGVALUE_PB_PERCENTS = "Percents";

		private static SplashScreen ms_frmSplash;

		private static Thread ms_oThread;

		private double m_dblOpacityIncrement = 0.5;

		private double m_dblOpacityDecrement = 0.5;

		private static string ms_sStatus;

		private double m_dblCompletionFraction;

		private Rectangle m_rProgress;

		private double m_dblLastCompletionFraction;

		private double m_dblPBIncrementPerTimerInterval = 0.015;

		private bool m_bFirstLaunch;

		private DateTime m_dtStart;

		private bool m_bDTSet;

		private int m_iIndex = 1;

		private int m_iActualTicks;

		private ArrayList m_alPreviousCompletionFraction;

		private ArrayList m_alActualTimes = new ArrayList();

		private System.Windows.Forms.Timer splashScreenTimer;

		private ProgressBar pnlStatus;

		private global::AlphaFormTransformer.AlphaFormTransformer alphaFormTransformer1;

		private Label label1;

		private Label lblStatus;

		private Label lblTimeRemaining;

		private Panel panel1;

		private AlphaFormMarker alphaFormMarker2;

		private AlphaFormMarker alphaFormMarker1;

		private Panel panel2;

		private IContainer components;

		public static SplashScreen SplashForm => ms_frmSplash;

		public SplashScreen()
		{
			base.TopMost = true;
			base.ShowInTaskbar = false;
			InitializeComponent();
			base.Opacity = 0.0;
			splashScreenTimer.Interval = 50;
			splashScreenTimer.Start();
			panel1.BackColor = Color.FromArgb(218, 239, 196);
			panel2.BackColor = Color.FromArgb(200, 226, 173);
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
			label1.Text = versionInfo.FileDescription + "\nVersion " + versionInfo.FileVersion + "\n©" + versionInfo.CompanyName + " 2007-2010\nAll rights reserved.\n";
			label1.Text += "The trademarks Cricut, Cricut DesignStudio, Cricut Essentials, Cricut Expression and related";
			label1.Text += " logos and graphics are trademarks and copyrighted works of Provo Craft and Novelty, Inc.";
			label1.Text += " and may not be used or reproduced without permission. ©2005-2010 All rights reserved.";
			label1.Text += " Refer to the user documentation for patent and trademark information.";
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.splashScreenTimer = new System.Windows.Forms.Timer(this.components);
			this.alphaFormTransformer1 = new global::AlphaFormTransformer.AlphaFormTransformer();
			this.alphaFormMarker2 = new AlphaFormTransformer.AlphaFormMarker();
			this.alphaFormMarker1 = new AlphaFormTransformer.AlphaFormMarker();
			this.panel2 = new System.Windows.Forms.Panel();
			this.pnlStatus = new System.Windows.Forms.ProgressBar();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblTimeRemaining = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.alphaFormTransformer1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.splashScreenTimer.Tick += new System.EventHandler(timer1_Tick);
			this.alphaFormTransformer1.BackgroundImage = Cricut_Design_Studio.MarcusResources.splash_bg_2;
			this.alphaFormTransformer1.Controls.Add(this.alphaFormMarker2);
			this.alphaFormTransformer1.Controls.Add(this.alphaFormMarker1);
			this.alphaFormTransformer1.Controls.Add(this.panel2);
			this.alphaFormTransformer1.Controls.Add(this.panel1);
			this.alphaFormTransformer1.DragSleep = 30u;
			this.alphaFormTransformer1.Location = new System.Drawing.Point(0, 0);
			this.alphaFormTransformer1.Name = "alphaFormTransformer1";
			this.alphaFormTransformer1.Size = new System.Drawing.Size(491, 372);
			this.alphaFormTransformer1.TabIndex = 6;
			this.alphaFormMarker2.FillBorder = 4u;
			this.alphaFormMarker2.Location = new System.Drawing.Point(293, 30);
			this.alphaFormMarker2.Name = "alphaFormMarker2";
			this.alphaFormMarker2.Size = new System.Drawing.Size(17, 17);
			this.alphaFormMarker2.TabIndex = 8;
			this.alphaFormMarker1.FillBorder = 4u;
			this.alphaFormMarker1.Location = new System.Drawing.Point(293, 84);
			this.alphaFormMarker1.Name = "alphaFormMarker1";
			this.alphaFormMarker1.Size = new System.Drawing.Size(17, 17);
			this.alphaFormMarker1.TabIndex = 7;
			this.panel2.Controls.Add(this.pnlStatus);
			this.panel2.Location = new System.Drawing.Point(285, 25);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(164, 27);
			this.panel2.TabIndex = 6;
			this.pnlStatus.Location = new System.Drawing.Point(2, 2);
			this.pnlStatus.Name = "pnlStatus";
			this.pnlStatus.Size = new System.Drawing.Size(160, 23);
			this.pnlStatus.TabIndex = 3;
			this.panel1.Controls.Add(this.lblTimeRemaining);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.lblStatus);
			this.panel1.Location = new System.Drawing.Point(287, 76);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(161, 250);
			this.panel1.TabIndex = 5;
			this.lblTimeRemaining.BackColor = System.Drawing.Color.Transparent;
			this.lblTimeRemaining.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.lblTimeRemaining.ForeColor = System.Drawing.Color.Black;
			this.lblTimeRemaining.Location = new System.Drawing.Point(6, 6);
			this.lblTimeRemaining.Name = "lblTimeRemaining";
			this.lblTimeRemaining.Size = new System.Drawing.Size(150, 18);
			this.lblTimeRemaining.TabIndex = 2;
			this.lblTimeRemaining.Text = "Time remaining";
			this.lblTimeRemaining.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblTimeRemaining.DoubleClick += new System.EventHandler(SplashScreen_DoubleClick);
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(6, 60);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 184);
			this.label1.TabIndex = 4;
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblStatus.BackColor = System.Drawing.Color.Transparent;
			this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.lblStatus.ForeColor = System.Drawing.Color.Black;
			this.lblStatus.Location = new System.Drawing.Point(6, 24);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(150, 36);
			this.lblStatus.TabIndex = 0;
			this.lblStatus.Text = "Loading";
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblStatus.DoubleClick += new System.EventHandler(SplashScreen_DoubleClick);
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			base.ClientSize = new System.Drawing.Size(491, 372);
			base.Controls.Add(this.alphaFormTransformer1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Name = "SplashScreen";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Cricut DesignStudio";
			base.TransparencyKey = System.Drawing.SystemColors.Control;
			base.DoubleClick += new System.EventHandler(SplashScreen_DoubleClick);
			base.Load += new System.EventHandler(SplashScreen_Load);
			this.alphaFormTransformer1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		public static void ShowSplashScreen()
		{
			if (ms_frmSplash == null)
			{
				ms_oThread = new Thread(ShowForm);
				ms_oThread.IsBackground = true;
				ms_oThread.SetApartmentState(ApartmentState.STA);
				ms_oThread.Start();
			}
		}

		private static void ShowForm()
		{
			ms_frmSplash = new SplashScreen();
			Application.Run(ms_frmSplash);
		}

		public static void CloseForm()
		{
			if (ms_frmSplash != null && !ms_frmSplash.IsDisposed)
			{
				ms_frmSplash.m_dblOpacityIncrement = 0.0 - ms_frmSplash.m_dblOpacityDecrement;
			}
			ms_oThread = null;
			ms_frmSplash = null;
		}

		public static void SetStatus(string newStatus)
		{
			SetStatus(newStatus, setReference: true);
		}

		public static void SetStatus(string newStatus, bool setReference)
		{
			ms_sStatus = newStatus;
			if (ms_frmSplash != null && setReference)
			{
				ms_frmSplash.SetReferenceInternal();
			}
		}

		public static void SetReferencePoint()
		{
			if (ms_frmSplash != null)
			{
				ms_frmSplash.SetReferenceInternal();
			}
		}

		private void SetReferenceInternal()
		{
			if (!m_bDTSet)
			{
				m_bDTSet = true;
				m_dtStart = DateTime.Now;
				ReadIncrements();
			}
			double num = ElapsedMilliSeconds();
			m_alActualTimes.Add(num);
			m_dblLastCompletionFraction = m_dblCompletionFraction;
			if (m_alPreviousCompletionFraction != null && m_iIndex < m_alPreviousCompletionFraction.Count)
			{
				m_dblCompletionFraction = (double)m_alPreviousCompletionFraction[m_iIndex++];
			}
			else
			{
				m_dblCompletionFraction = ((m_iIndex > 0) ? 1 : 0);
			}
		}

		private double ElapsedMilliSeconds()
		{
			return (DateTime.Now - m_dtStart).TotalMilliseconds;
		}

		private void ReadIncrements()
		{
			string stringRegistryValue = RegistryAccess.GetStringRegistryValue("Increment", "0.0015");
			if (double.TryParse(stringRegistryValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result))
			{
				m_dblPBIncrementPerTimerInterval = result;
			}
			else
			{
				m_dblPBIncrementPerTimerInterval = 0.0015;
			}
			string stringRegistryValue2 = RegistryAccess.GetStringRegistryValue("Percents", "");
			if (stringRegistryValue2 != "")
			{
				string[] array = stringRegistryValue2.Split(null);
				m_alPreviousCompletionFraction = new ArrayList();
				for (int i = 0; i < array.Length; i++)
				{
					if (double.TryParse(array[i], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result2))
					{
						m_alPreviousCompletionFraction.Add(result2);
					}
					else
					{
						m_alPreviousCompletionFraction.Add(1.0);
					}
				}
				return;
			}
			m_bFirstLaunch = true;
			try
			{
				lblTimeRemaining.Text = "";
			}
			catch
			{
			}
		}

		private void StoreIncrements()
		{
			string text = "";
			double num = ElapsedMilliSeconds();
			for (int i = 0; i < m_alActualTimes.Count; i++)
			{
				text = text + ((double)m_alActualTimes[i] / num).ToString("0.####", NumberFormatInfo.InvariantInfo) + " ";
			}
			RegistryAccess.SetStringRegistryValue("Percents", text);
			m_dblPBIncrementPerTimerInterval = 1.0 / (double)m_iActualTicks;
			RegistryAccess.SetStringRegistryValue("Increment", m_dblPBIncrementPerTimerInterval.ToString("#.000000", NumberFormatInfo.InvariantInfo));
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			lblStatus.Text = ms_sStatus;
			if (m_dblOpacityIncrement > 0.0)
			{
				m_iActualTicks++;
				if (base.Opacity < 1.0)
				{
					base.Opacity += m_dblOpacityIncrement;
				}
			}
			else if (base.Opacity > 0.0)
			{
				try
				{
					base.Opacity += m_dblOpacityIncrement;
				}
				catch
				{
				}
			}
			else
			{
				StoreIncrements();
				Close();
			}
			if (m_bFirstLaunch || !(m_dblLastCompletionFraction < m_dblCompletionFraction))
			{
				return;
			}
			m_dblLastCompletionFraction += m_dblPBIncrementPerTimerInterval;
			int num = (int)Math.Round(m_dblLastCompletionFraction * 100.0);
			pnlStatus.Value = ((num < pnlStatus.Minimum) ? pnlStatus.Minimum : ((num > pnlStatus.Maximum) ? pnlStatus.Maximum : num));
			if (m_dblLastCompletionFraction > 0.0)
			{
				int num2 = 1 + (int)(50.0 * ((1.0 - m_dblLastCompletionFraction) / m_dblPBIncrementPerTimerInterval)) / 1000;
				if (num2 == 1)
				{
					lblTimeRemaining.Text = $"1 second remaining";
				}
				else
				{
					lblTimeRemaining.Text = $"{num2} seconds remaining";
				}
			}
		}

		private void pnlStatus_Paint(object sender, PaintEventArgs e)
		{
			if (!m_bFirstLaunch && e.ClipRectangle.Width > 0 && m_iActualTicks > 1)
			{
				m_rProgress.Width = ((m_rProgress.Width <= 1) ? 1 : m_rProgress.Width);
				m_rProgress.Height = ((m_rProgress.Height <= 1) ? 1 : m_rProgress.Height);
				LinearGradientBrush brush = new LinearGradientBrush(m_rProgress, Color.FromArgb(213, 228, 207), Color.FromArgb(242, 245, 241), LinearGradientMode.Horizontal);
				e.Graphics.FillRectangle(brush, m_rProgress);
			}
		}

		private void SplashScreen_DoubleClick(object sender, EventArgs e)
		{
			CloseForm();
		}

		private void SplashScreen_Load(object sender, EventArgs e)
		{
			alphaFormTransformer1.TransformForm();
		}
	}
}
