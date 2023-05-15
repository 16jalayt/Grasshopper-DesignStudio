using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	internal class AboutBox : Form
	{
		private IContainer components;

		private TableLayoutPanel tableLayoutPanel;

		private Label cogDevLabel;

		private Label provoCraftLabel;

		private PictureBox logoPictureBox;

		private Label regNumLabel;

		private Label actNumLabel;

		private Button okButton;

		private Label regCodeLabel;

		private Label descLabel;

		public string AssemblyTitle
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
				if (customAttributes.Length > 0)
				{
					AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
					if (assemblyTitleAttribute.Title != "")
					{
						return assemblyTitleAttribute.Title;
					}
				}
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

		public string AssemblyDescription
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)customAttributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
			}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cricut_Design_Studio.AboutBox));
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.provoCraftLabel = new System.Windows.Forms.Label();
			this.cogDevLabel = new System.Windows.Forms.Label();
			this.regNumLabel = new System.Windows.Forms.Label();
			this.actNumLabel = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.regCodeLabel = new System.Windows.Forms.Label();
			this.descLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.logoPictureBox).BeginInit();
			base.SuspendLayout();
			this.tableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.provoCraftLabel, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.cogDevLabel, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.regNumLabel, 1, 3);
			this.tableLayoutPanel.Controls.Add(this.actNumLabel, 1, 4);
			this.tableLayoutPanel.Controls.Add(this.okButton, 1, 7);
			this.tableLayoutPanel.Controls.Add(this.regCodeLabel, 1, 5);
			this.tableLayoutPanel.Controls.Add(this.descLabel, 1, 6);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 8;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 95f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.Size = new System.Drawing.Size(476, 320);
			this.tableLayoutPanel.TabIndex = 0;
			this.logoPictureBox.Image = (System.Drawing.Image)resources.GetObject("logoPictureBox.Image");
			this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
			this.logoPictureBox.Name = "logoPictureBox";
			this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 8);
			this.logoPictureBox.Size = new System.Drawing.Size(164, 314);
			this.logoPictureBox.TabIndex = 12;
			this.logoPictureBox.TabStop = false;
			this.provoCraftLabel.AutoSize = true;
			this.provoCraftLabel.Location = new System.Drawing.Point(173, 70);
			this.provoCraftLabel.Name = "provoCraftLabel";
			this.provoCraftLabel.Size = new System.Drawing.Size(82, 13);
			this.provoCraftLabel.TabIndex = 15;
			this.provoCraftLabel.Text = "provoCraftLabel";
			this.cogDevLabel.AutoSize = true;
			this.cogDevLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.cogDevLabel.Location = new System.Drawing.Point(173, 20);
			this.cogDevLabel.Name = "cogDevLabel";
			this.cogDevLabel.Size = new System.Drawing.Size(79, 15);
			this.cogDevLabel.TabIndex = 14;
			this.cogDevLabel.Text = "cogDevLabel";
			this.regNumLabel.AutoSize = true;
			this.regNumLabel.Location = new System.Drawing.Point(173, 165);
			this.regNumLabel.Name = "regNumLabel";
			this.regNumLabel.Size = new System.Drawing.Size(70, 13);
			this.regNumLabel.TabIndex = 16;
			this.regNumLabel.Text = "regNumLabel";
			this.actNumLabel.AutoSize = true;
			this.actNumLabel.Location = new System.Drawing.Point(173, 183);
			this.actNumLabel.Name = "actNumLabel";
			this.actNumLabel.Size = new System.Drawing.Size(70, 13);
			this.actNumLabel.TabIndex = 17;
			this.actNumLabel.Text = "actNumLabel";
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(398, 294);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 18;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.regCodeLabel.AutoSize = true;
			this.regCodeLabel.Location = new System.Drawing.Point(173, 201);
			this.regCodeLabel.Name = "regCodeLabel";
			this.regCodeLabel.Size = new System.Drawing.Size(73, 13);
			this.regCodeLabel.TabIndex = 19;
			this.regCodeLabel.Text = "regCodeLabel";
			this.descLabel.AutoSize = true;
			this.descLabel.Location = new System.Drawing.Point(173, 226);
			this.descLabel.Name = "descLabel";
			this.descLabel.Size = new System.Drawing.Size(30, 13);
			this.descLabel.TabIndex = 20;
			this.descLabel.Text = "desc";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			base.CancelButton = this.okButton;
			base.ClientSize = new System.Drawing.Size(494, 338);
			base.Controls.Add(this.tableLayoutPanel);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AboutBox";
			base.Padding = new System.Windows.Forms.Padding(9);
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AboutBox";
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.logoPictureBox).EndInit();
			base.ResumeLayout(false);
		}

		public AboutBox()
		{
			InitializeComponent();
			Text = $"About {AssemblyTitle}";
			cogDevLabel.Text = AssemblyProduct + "\nVersion " + AssemblyVersion + "\n©" + AssemblyCompany + " 2007-2010. All rights reserved.\n\n";
			provoCraftLabel.Text = "The trademarks Cricut, Cricut DesignStudio, Cricut Essentials, Cricut Expression and related";
			provoCraftLabel.Text += " logos and graphics are trademarks and copyrighted works of Provo Craft and Novelty, Inc.";
			provoCraftLabel.Text += " and may not be used or reproduced without permission. ©2005-2010 All rights reserved.";
			provoCraftLabel.Text += " Refer to the user documentation for patent and trademark information.";
			BackColor = Color.FromArgb(202, 227, 175);
			regNumLabel.Text = "SN " + Form1.myRootForm.checkProgThenUserKey("regNum");
			actNumLabel.Text = "KEY " + Form1.myRootForm.checkProgThenUserKey("actNum");
			regCodeLabel.Text = "REG " + Form1.myRootForm.getMACAddress().Replace(":", "");
			descLabel.Text = "Phil Beffrey, Aaron Johnson, Jason Brinkerhoff, Rodney Stock, Dan Lyke, Marcus Badgley, ";
			descLabel.Text += "Lisa Polson, Jackie Brinkerhoff, Jenn Boyer, Nathan Forsgren, Kimberly Harris, ";
			descLabel.Text += "Jackie Shafer, Ed Alvarez, Beth Jepson, Dan Larsen, Noralee Peterson, Jill Webster, ";
			descLabel.Text += "and Robert Workman.";
		}
	}
}
