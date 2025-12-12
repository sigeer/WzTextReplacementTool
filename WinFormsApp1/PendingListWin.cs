using System;
using System.Collections.Generic;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace WinFormsApp1
{
    /// <summary>
    /// 待处理的项
    /// </summary>
    internal class PendingListWin: DockContent
    {
        public PendingListWin()
        {
            Text = "待手动处理";
        }
    }
}
