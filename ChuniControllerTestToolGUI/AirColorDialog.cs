using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuniControllerTestToolGUI
{
    public partial class AirColorDialog : Form
    {
        /// <summary> 蓝／绿／红 三个开关状态 </summary>
        public bool BlueEnabled => chkBlue.Checked;
        public bool GreenEnabled => chkGreen.Checked;
        public bool RedEnabled => chkRed.Checked;
        /// <summary> 根据开关生成的预览色 </summary>
        public Color SelectedColor
            => Color.FromArgb(
                   RedEnabled ? 255 : 0,
                   GreenEnabled ? 255 : 0,
                   BlueEnabled ? 255 : 0
               );
        public AirColorDialog(bool initialBlue, bool initialRed, bool initialGreen)
        {
            InitializeComponent();
            chkBlue.Checked = initialBlue;
            chkRed.Checked = initialRed;
            chkGreen.Checked = initialGreen;
            UpdatePreview(this, EventArgs.Empty);
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private void UpdatePreview(object? sender, EventArgs e)
        {
            pnlPreview.BackColor = SelectedColor;
        }
    }
}
