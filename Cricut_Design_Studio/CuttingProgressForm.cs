using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class CuttingProgressForm : Form
	{
		private IContainer components;

		private Label cuttingMessageLabel;

		private Label label1;

		public ProgressBar cuttingProgressBar;

		public Button cuttingCancelButton;

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
			this.cuttingProgressBar = new System.Windows.Forms.ProgressBar();
			this.cuttingCancelButton = new System.Windows.Forms.Button();
			this.cuttingMessageLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.cuttingProgressBar.Location = new System.Drawing.Point(12, 72);
			this.cuttingProgressBar.Name = "cuttingProgressBar";
			this.cuttingProgressBar.Size = new System.Drawing.Size(200, 23);
			this.cuttingProgressBar.TabIndex = 0;
			this.cuttingCancelButton.Location = new System.Drawing.Point(226, 72);
			this.cuttingCancelButton.Name = "cuttingCancelButton";
			this.cuttingCancelButton.Size = new System.Drawing.Size(75, 23);
			this.cuttingCancelButton.TabIndex = 1;
			this.cuttingCancelButton.Text = "Cancel";
			this.cuttingCancelButton.UseVisualStyleBackColor = true;
			this.cuttingCancelButton.Click += new System.EventHandler(cuttingCancelButton_Click);
			this.cuttingMessageLabel.AutoSize = true;
			this.cuttingMessageLabel.Location = new System.Drawing.Point(12, 34);
			this.cuttingMessageLabel.Name = "cuttingMessageLabel";
			this.cuttingMessageLabel.Size = new System.Drawing.Size(235, 13);
			this.cuttingMessageLabel.TabIndex = 2;
			this.cuttingMessageLabel.Text = "(This box will disappear when cutting is finished.)";
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(210, 17);
			this.label1.TabIndex = 3;
			this.label1.Text = "Click Cancel to stop cutting now.";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(313, 107);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.cuttingMessageLabel);
			base.Controls.Add(this.cuttingCancelButton);
			base.Controls.Add(this.cuttingProgressBar);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CuttingProgressForm";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Cutting - 0% complete";
			base.TopMost = true;
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CuttingProgressForm_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public CuttingProgressForm()
		{
			InitializeComponent();
			base.TopMost = true;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
		}

		private void cuttingCancelButton_Click(object sender, EventArgs e)
		{
			Form1.myRootForm.cuttingBackgroundWorker.CancelAsync();
		}

		private void CuttingProgressForm_FormClosing(object sender, FormClosingEventArgs e)
		{
		}
	}
}
