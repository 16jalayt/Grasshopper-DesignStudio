using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class UpdatingProgressForm : Form
	{
		private IContainer components;

		public ProgressBar updatingProgressBar;

		private Label label1;

		private Label cuttingMessageLabel;

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
			this.updatingProgressBar = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.cuttingMessageLabel = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.updatingProgressBar.Location = new System.Drawing.Point(12, 70);
			this.updatingProgressBar.Name = "updatingProgressBar";
			this.updatingProgressBar.Size = new System.Drawing.Size(289, 23);
			this.updatingProgressBar.TabIndex = 1;
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(211, 17);
			this.label1.TabIndex = 4;
			this.label1.Text = "Please do not close this window.";
			this.cuttingMessageLabel.AutoSize = true;
			this.cuttingMessageLabel.Location = new System.Drawing.Point(12, 34);
			this.cuttingMessageLabel.Name = "cuttingMessageLabel";
			this.cuttingMessageLabel.Size = new System.Drawing.Size(271, 26);
			this.cuttingMessageLabel.TabIndex = 5;
			this.cuttingMessageLabel.Text = "(This box will disappear when the Cricut firmware update\r\nis finished.)";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(313, 107);
			base.Controls.Add(this.cuttingMessageLabel);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.updatingProgressBar);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "UpdatingProgressForm";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "UpdatingProgressForm";
			base.TopMost = true;
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public UpdatingProgressForm()
		{
			InitializeComponent();
		}
	}
}
