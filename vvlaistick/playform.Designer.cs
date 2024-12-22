namespace vvlaistick
{
    partial class playform
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(playform));
            this.panelPlay1 = new CCWin.SkinControl.SkinPanel();
            this.progressBarPlay = new CCWin.SkinControl.SkinProgressBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listviewfile = new CCWin.SkinControl.SkinListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.skinPictureBox1 = new CCWin.SkinControl.SkinPictureBox();
            this.btnFindVideo = new CCWin.SkinControl.SkinButton();
            this.skinLabel4 = new CCWin.SkinControl.SkinLabel();
            this.skinLabel3 = new CCWin.SkinControl.SkinLabel();
            this.skinLabel2 = new CCWin.SkinControl.SkinLabel();
            this.btnStop = new CCWin.SkinControl.SkinButton();
            this.btnPause = new CCWin.SkinControl.SkinButton();
            this.btnPlay = new CCWin.SkinControl.SkinButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skinPictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPlay1
            // 
            this.panelPlay1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panelPlay1.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.panelPlay1.DownBack = null;
            this.panelPlay1.Location = new System.Drawing.Point(5, 4);
            this.panelPlay1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPlay1.MouseBack = null;
            this.panelPlay1.Name = "panelPlay1";
            this.panelPlay1.NormlBack = null;
            this.panelPlay1.Size = new System.Drawing.Size(831, 666);
            this.panelPlay1.TabIndex = 0;
            // 
            // progressBarPlay
            // 
            this.progressBarPlay.Back = null;
            this.progressBarPlay.BackColor = System.Drawing.Color.Transparent;
            this.progressBarPlay.BarBack = null;
            this.progressBarPlay.BarRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.progressBarPlay.Border = System.Drawing.Color.CornflowerBlue;
            this.progressBarPlay.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.progressBarPlay.ForeColor = System.Drawing.Color.Red;
            this.progressBarPlay.FormatString = "";
            this.progressBarPlay.InnerBorder = System.Drawing.Color.Transparent;
            this.progressBarPlay.Location = new System.Drawing.Point(3, 678);
            this.progressBarPlay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBarPlay.Name = "progressBarPlay";
            this.progressBarPlay.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.progressBarPlay.Size = new System.Drawing.Size(832, 11);
            this.progressBarPlay.Step = 1;
            this.progressBarPlay.TabIndex = 3;
            this.progressBarPlay.TrackBack = System.Drawing.Color.CornflowerBlue;
            this.progressBarPlay.TrackFore = System.Drawing.Color.DodgerBlue;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listviewfile);
            this.panel1.Controls.Add(this.dateTimePicker1);
            this.panel1.Controls.Add(this.skinPictureBox1);
            this.panel1.Controls.Add(this.btnFindVideo);
            this.panel1.Controls.Add(this.skinLabel4);
            this.panel1.Controls.Add(this.skinLabel3);
            this.panel1.Controls.Add(this.skinLabel2);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.btnPlay);
            this.panel1.Controls.Add(this.panelPlay1);
            this.panel1.Controls.Add(this.progressBarPlay);
            this.panel1.Location = new System.Drawing.Point(8, 50);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1039, 756);
            this.panel1.TabIndex = 4;
            // 
            // listviewfile
            // 
            this.listviewfile.BorderColor = System.Drawing.Color.Silver;
            this.listviewfile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listviewfile.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listviewfile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listviewfile.FullRowSelect = true;
            this.listviewfile.GridLines = true;
            this.listviewfile.HeadColor = System.Drawing.Color.White;
            this.listviewfile.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listviewfile.Location = new System.Drawing.Point(841, 220);
            this.listviewfile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listviewfile.Name = "listviewfile";
            this.listviewfile.OwnerDraw = true;
            this.listviewfile.RowBackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.listviewfile.SelectedColor = System.Drawing.Color.LightSteelBlue;
            this.listviewfile.Size = new System.Drawing.Size(193, 530);
            this.listviewfile.TabIndex = 22;
            this.listviewfile.UseCompatibleStateImageBehavior = false;
            this.listviewfile.View = System.Windows.Forms.View.Details;
            this.listviewfile.DoubleClick += new System.EventHandler(this.listviewfile_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "序号";
            this.columnHeader1.Width = 38;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "文件名";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 142;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CalendarFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(842, 187);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(128, 23);
            this.dateTimePicker1.TabIndex = 21;
            // 
            // skinPictureBox1
            // 
            this.skinPictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.skinPictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("skinPictureBox1.BackgroundImage")));
            this.skinPictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.skinPictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skinPictureBox1.Location = new System.Drawing.Point(842, 4);
            this.skinPictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.skinPictureBox1.Name = "skinPictureBox1";
            this.skinPictureBox1.Size = new System.Drawing.Size(193, 176);
            this.skinPictureBox1.TabIndex = 20;
            this.skinPictureBox1.TabStop = false;
            // 
            // btnFindVideo
            // 
            this.btnFindVideo.BackColor = System.Drawing.Color.Transparent;
            this.btnFindVideo.BaseColor = System.Drawing.Color.LightSteelBlue;
            this.btnFindVideo.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.btnFindVideo.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnFindVideo.DownBack = null;
            this.btnFindVideo.Image = ((System.Drawing.Image)(resources.GetObject("btnFindVideo.Image")));
            this.btnFindVideo.Location = new System.Drawing.Point(978, 186);
            this.btnFindVideo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnFindVideo.MouseBack = null;
            this.btnFindVideo.MouseBaseColor = System.Drawing.Color.CornflowerBlue;
            this.btnFindVideo.Name = "btnFindVideo";
            this.btnFindVideo.NormlBack = null;
            this.btnFindVideo.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.btnFindVideo.Size = new System.Drawing.Size(56, 25);
            this.btnFindVideo.TabIndex = 18;
            this.btnFindVideo.UseVisualStyleBackColor = false;
            this.btnFindVideo.Click += new System.EventHandler(this.btnFindVideo_Click);
            // 
            // skinLabel4
            // 
            this.skinLabel4.AutoSize = true;
            this.skinLabel4.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel4.BorderColor = System.Drawing.Color.White;
            this.skinLabel4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel4.Location = new System.Drawing.Point(474, 733);
            this.skinLabel4.Name = "skinLabel4";
            this.skinLabel4.Size = new System.Drawing.Size(32, 17);
            this.skinLabel4.TabIndex = 13;
            this.skinLabel4.Text = "停止";
            // 
            // skinLabel3
            // 
            this.skinLabel3.AutoSize = true;
            this.skinLabel3.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel3.BorderColor = System.Drawing.Color.White;
            this.skinLabel3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel3.Location = new System.Drawing.Point(404, 733);
            this.skinLabel3.Name = "skinLabel3";
            this.skinLabel3.Size = new System.Drawing.Size(32, 17);
            this.skinLabel3.TabIndex = 12;
            this.skinLabel3.Text = "暂停";
            // 
            // skinLabel2
            // 
            this.skinLabel2.AutoSize = true;
            this.skinLabel2.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel2.BorderColor = System.Drawing.Color.White;
            this.skinLabel2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel2.Location = new System.Drawing.Point(337, 733);
            this.skinLabel2.Name = "skinLabel2";
            this.skinLabel2.Size = new System.Drawing.Size(32, 17);
            this.skinLabel2.TabIndex = 11;
            this.skinLabel2.Text = "开始";
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.Transparent;
            this.btnStop.BaseColor = System.Drawing.Color.CornflowerBlue;
            this.btnStop.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.btnStop.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnStop.DownBack = null;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageSize = new System.Drawing.Size(23, 23);
            this.btnStop.Location = new System.Drawing.Point(460, 693);
            this.btnStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStop.MouseBack = null;
            this.btnStop.Name = "btnStop";
            this.btnStop.NormlBack = null;
            this.btnStop.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.btnStop.Size = new System.Drawing.Size(62, 35);
            this.btnStop.TabIndex = 6;
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPause
            // 
            this.btnPause.BackColor = System.Drawing.Color.Transparent;
            this.btnPause.BaseColor = System.Drawing.Color.CornflowerBlue;
            this.btnPause.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.btnPause.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnPause.DownBack = null;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.Location = new System.Drawing.Point(391, 693);
            this.btnPause.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPause.MouseBack = null;
            this.btnPause.Name = "btnPause";
            this.btnPause.NormlBack = null;
            this.btnPause.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.btnPause.Size = new System.Drawing.Size(62, 35);
            this.btnPause.TabIndex = 5;
            this.btnPause.UseVisualStyleBackColor = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.Transparent;
            this.btnPlay.BaseColor = System.Drawing.Color.CornflowerBlue;
            this.btnPlay.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.btnPlay.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnPlay.DownBack = null;
            this.btnPlay.Image = ((System.Drawing.Image)(resources.GetObject("btnPlay.Image")));
            this.btnPlay.Location = new System.Drawing.Point(322, 693);
            this.btnPlay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPlay.MouseBack = null;
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.NormlBack = null;
            this.btnPlay.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.btnPlay.Size = new System.Drawing.Size(62, 35);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // playform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1056, 812);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = false;
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "playform";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "录像回放";
            this.Load += new System.EventHandler(this.playform_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skinPictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinPanel panelPlay1;
        private CCWin.SkinControl.SkinProgressBar progressBarPlay;
        private System.Windows.Forms.Panel panel1;
        private CCWin.SkinControl.SkinButton btnStop;
        private CCWin.SkinControl.SkinButton btnPause;
        private CCWin.SkinControl.SkinButton btnPlay;
        private CCWin.SkinControl.SkinLabel skinLabel4;
        private CCWin.SkinControl.SkinLabel skinLabel3;
        private CCWin.SkinControl.SkinLabel skinLabel2;
        private CCWin.SkinControl.SkinPictureBox skinPictureBox1;
        private CCWin.SkinControl.SkinButton btnFindVideo;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private CCWin.SkinControl.SkinListView listviewfile;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
    }
}