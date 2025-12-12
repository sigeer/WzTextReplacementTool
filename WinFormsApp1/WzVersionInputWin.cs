using MapleLib.WzLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class WzVersionInputWin : Form
    {
        public event EventHandler<WzVersion>? OnSubmit;
        public WzVersionInputWin(bool showGameVersion = true)
        {
            InitializeComponent();

            if (!showGameVersion)
            {
                Label_GameVerion.Visible = false;
                Text_Version.Visible = false;
            }
        }

        private void Btn_Submit_Click(object sender, EventArgs e)
        {
            this.Close();

            OnSubmit?.Invoke(this, new WzVersion(WzMapleVersion.GMS, 83));
        }
    }

    public record WzVersion(WzMapleVersion Version, short GameVersion);
}
