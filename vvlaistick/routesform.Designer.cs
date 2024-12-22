namespace vvlaistick
{
    partial class routesform
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(routesform));
            this.btnShow = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.listViewmarkers = new System.Windows.Forms.ListView();
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderlng = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderlat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderdate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderendlng = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderendlat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnDetail = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnShow
            // 
            this.btnShow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnShow.Location = new System.Drawing.Point(88, 36);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(75, 28);
            this.btnShow.TabIndex = 5;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(7, 36);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 28);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // listViewmarkers
            // 
            this.listViewmarkers.CheckBoxes = true;
            this.listViewmarkers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderID,
            this.columnHeaderlng,
            this.columnHeaderlat,
            this.columnHeaderendlng,
            this.columnHeaderendlat,
            this.columnHeaderdate});
            this.listViewmarkers.FullRowSelect = true;
            this.listViewmarkers.GridLines = true;
            this.listViewmarkers.Location = new System.Drawing.Point(7, 70);
            this.listViewmarkers.Name = "listViewmarkers";
            this.listViewmarkers.Size = new System.Drawing.Size(805, 454);
            this.listViewmarkers.TabIndex = 3;
            this.listViewmarkers.UseCompatibleStateImageBehavior = false;
            this.listViewmarkers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderID
            // 
            this.columnHeaderID.Text = "ID";
            this.columnHeaderID.Width = 56;
            // 
            // columnHeaderlng
            // 
            this.columnHeaderlng.Text = "Start Lng";
            this.columnHeaderlng.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderlng.Width = 151;
            // 
            // columnHeaderlat
            // 
            this.columnHeaderlat.Text = "Start Lat";
            this.columnHeaderlat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderlat.Width = 143;
            // 
            // columnHeaderdate
            // 
            this.columnHeaderdate.Text = "Date";
            this.columnHeaderdate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderdate.Width = 143;
            // 
            // columnHeaderendlng
            // 
            this.columnHeaderendlng.Text = "End Lng";
            this.columnHeaderendlng.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderendlng.Width = 148;
            // 
            // columnHeaderendlat
            // 
            this.columnHeaderendlat.Text = "End Lat";
            this.columnHeaderendlat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderendlat.Width = 157;
            // 
            // btnDetail
            // 
            this.btnDetail.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDetail.Location = new System.Drawing.Point(169, 36);
            this.btnDetail.Name = "btnDetail";
            this.btnDetail.Size = new System.Drawing.Size(75, 28);
            this.btnDetail.TabIndex = 6;
            this.btnDetail.Text = "Details";
            this.btnDetail.UseVisualStyleBackColor = true;
            this.btnDetail.Click += new System.EventHandler(this.btnDetail_Click);
            // 
            // routesform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 529);
            this.Controls.Add(this.btnDetail);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.listViewmarkers);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "routesform";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Routes";
            this.TitleCenter = false;
            this.TitleOffset = new System.Drawing.Point(0, 3);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.routesform_FormClosing);
            this.Load += new System.EventHandler(this.routesform_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ListView listViewmarkers;
        private System.Windows.Forms.ColumnHeader columnHeaderID;
        private System.Windows.Forms.ColumnHeader columnHeaderlng;
        private System.Windows.Forms.ColumnHeader columnHeaderlat;
        private System.Windows.Forms.ColumnHeader columnHeaderdate;
        private System.Windows.Forms.ColumnHeader columnHeaderendlng;
        private System.Windows.Forms.ColumnHeader columnHeaderendlat;
        private System.Windows.Forms.Button btnDetail;
    }
}