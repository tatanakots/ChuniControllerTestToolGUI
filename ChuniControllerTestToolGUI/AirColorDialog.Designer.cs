namespace ChuniControllerTestToolGUI
{
    partial class AirColorDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AirColorDialog));
            chkRed = new CheckBox();
            btnCancel = new Button();
            btnOK = new Button();
            chkGreen = new CheckBox();
            chkBlue = new CheckBox();
            pnlPreview = new Panel();
            label1 = new Label();
            SuspendLayout();
            // 
            // chkRed
            // 
            chkRed.AutoSize = true;
            chkRed.Location = new Point(329, 78);
            chkRed.Name = "chkRed";
            chkRed.Size = new Size(87, 21);
            chkRed.TabIndex = 0;
            chkRed.Text = "红（RED）";
            chkRed.UseVisualStyleBackColor = true;
            chkRed.CheckedChanged += UpdatePreview;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(410, 214);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(329, 214);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "确认";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.DialogResult = DialogResult.OK;
            // 
            // chkGreen
            // 
            chkGreen.AutoSize = true;
            chkGreen.Location = new Point(329, 105);
            chkGreen.Name = "chkGreen";
            chkGreen.Size = new Size(104, 21);
            chkGreen.TabIndex = 3;
            chkGreen.Text = "绿（GREEN）";
            chkGreen.UseVisualStyleBackColor = true;
            chkGreen.CheckedChanged += UpdatePreview;
            // 
            // chkBlue
            // 
            chkBlue.AutoSize = true;
            chkBlue.Location = new Point(329, 132);
            chkBlue.Name = "chkBlue";
            chkBlue.Size = new Size(93, 21);
            chkBlue.TabIndex = 4;
            chkBlue.Text = "蓝（BLUE）";
            chkBlue.UseVisualStyleBackColor = true;
            chkBlue.CheckedChanged += UpdatePreview;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(12, 29);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(230, 208);
            pnlPreview.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(104, 17);
            label1.TabIndex = 6;
            label1.Text = "请在此预览颜色：";
            // 
            // AirColorDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(497, 249);
            Controls.Add(label1);
            Controls.Add(pnlPreview);
            Controls.Add(chkBlue);
            Controls.Add(chkGreen);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(chkRed);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "AirColorDialog";
            Text = "颜色选取器";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkRed;
        private Button btnCancel;
        private Button btnOK;
        private CheckBox chkGreen;
        private CheckBox chkBlue;
        private Panel pnlPreview;
        private Label label1;
    }
}