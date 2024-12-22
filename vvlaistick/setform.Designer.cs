namespace vvlaistick
{
    partial class setform
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(setform));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSelect = new DMSkin.MetroDMButton();
            this.label3 = new System.Windows.Forms.Label();
            this.txtfileurl = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSelect);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtfileurl);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(10, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(398, 88);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "录像截图";
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.Transparent;
            this.btnSelect.DownImage = ((System.Drawing.Image)(resources.GetObject("btnSelect.DownImage")));
            this.btnSelect.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSelect.ForeColor = System.Drawing.Color.Black;
            this.btnSelect.Image = null;
            this.btnSelect.IsShowBorder = true;
            this.btnSelect.Location = new System.Drawing.Point(296, 34);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelect.MoveImage = ((System.Drawing.Image)(resources.GetObject("btnSelect.MoveImage")));
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.NormalImage = ((System.Drawing.Image)(resources.GetObject("btnSelect.NormalImage")));
            this.btnSelect.Size = new System.Drawing.Size(79, 32);
            this.btnSelect.TabIndex = 15;
            this.btnSelect.Text = "选择文件夹";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(29, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "路径";
            // 
            // txtfileurl
            // 
            this.txtfileurl.BackColor = System.Drawing.SystemColors.HighlightText;
            this.txtfileurl.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtfileurl.Location = new System.Drawing.Point(72, 39);
            this.txtfileurl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtfileurl.Name = "txtfileurl";
            this.txtfileurl.ReadOnly = true;
            this.txtfileurl.Size = new System.Drawing.Size(218, 23);
            this.txtfileurl.TabIndex = 6;
            // 
            // setform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CaptionBackColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CaptionBackColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(418, 131);
            this.CloseBoxSize = new System.Drawing.Size(20, 20);
            this.CloseDownBack = ((System.Drawing.Image)(resources.GetObject("$this.CloseDownBack")));
            this.CloseMouseBack = ((System.Drawing.Image)(resources.GetObject("$this.CloseMouseBack")));
            this.CloseNormlBack = ((System.Drawing.Image)(resources.GetObject("$this.CloseNormlBack")));
            this.Controls.Add(this.groupBox2);
            this.EffectCaption = CCWin.TitleType.Title;
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "setform";
            this.ShowBorder = false;
            this.Text = "软件设置";
            this.TitleCenter = false;
            this.TitleColor = System.Drawing.Color.White;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.setform_FormClosing);
            this.Load += new System.EventHandler(this.setform_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtfileurl;
        private DMSkin.MetroDMButton btnSelect;
    }
}