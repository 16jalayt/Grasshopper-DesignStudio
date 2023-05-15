using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

internal class LayeredWindowForm : Form
{
	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 524288;
			return createParams;
		}
	}

	public LayeredWindowForm()
	{
		base.FormBorderStyle = FormBorderStyle.None;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		e.Cancel = true;
		base.OnClosing(e);
		base.Owner.Close();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		InitializeStyles();
		base.OnHandleCreated(e);
	}

	public void SetBits(Bitmap bitmap)
	{
		if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
		{
			throw new ApplicationException("The bitmap must be 32 bits per pixel with an alpha channel.");
		}
		IntPtr hObj = IntPtr.Zero;
		IntPtr dC = Win32.GetDC(IntPtr.Zero);
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = Win32.CreateCompatibleDC(dC);
		try
		{
			Win32.Point pptDst = new Win32.Point(base.Left, base.Top);
			Win32.Size psize = new Win32.Size(bitmap.Width, bitmap.Height);
			Win32.BLENDFUNCTION pblend = default(Win32.BLENDFUNCTION);
			Win32.Point pptSrc = new Win32.Point(0, 0);
			intPtr = bitmap.GetHbitmap(Color.FromArgb(0));
			hObj = Win32.SelectObject(intPtr2, intPtr);
			pblend.BlendOp = 0;
			pblend.SourceConstantAlpha = byte.MaxValue;
			pblend.AlphaFormat = 1;
			pblend.BlendFlags = 0;
			Win32.UpdateLayeredWindow(base.Handle, dC, ref pptDst, ref psize, intPtr2, ref pptSrc, 0, ref pblend, 2);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Win32.SelectObject(intPtr2, hObj);
				Win32.DeleteObject(intPtr);
			}
			Win32.ReleaseDC(IntPtr.Zero, dC);
			Win32.DeleteDC(intPtr2);
		}
	}

	private void InitializeStyles()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		UpdateStyles();
	}
}
