namespace VoiceCommandListener
{
    partial class FrmVoiceListener
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmVoiceListener));
            this.btn_min = new System.Windows.Forms.Button();
            this.btn_closeForm = new System.Windows.Forms.Button();
            this.lb_Time = new System.Windows.Forms.Label();
            this.pl_CenterCircle = new System.Windows.Forms.Panel();
            this.AudioNotify = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripForNofifyRight = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMenuItem_ShowForm = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMenuItem_HideForm = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMenuItem_CloseForm = new System.Windows.Forms.ToolStripMenuItem();
            this.pl_CenterCircle.SuspendLayout();
            this.contextMenuStripForNofifyRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_min
            // 
            this.btn_min.FlatAppearance.BorderSize = 0;
            this.btn_min.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_min.Image = global::VoiceCommandListener.Properties.Resources.btn_min;
            this.btn_min.Location = new System.Drawing.Point(569, 1);
            this.btn_min.Name = "btn_min";
            this.btn_min.Size = new System.Drawing.Size(30, 30);
            this.btn_min.TabIndex = 0;
            this.btn_min.UseVisualStyleBackColor = true;
            this.btn_min.Click += new System.EventHandler(this.btn_min_Click);
            // 
            // btn_closeForm
            // 
            this.btn_closeForm.FlatAppearance.BorderSize = 0;
            this.btn_closeForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_closeForm.Image = global::VoiceCommandListener.Properties.Resources.btn_close;
            this.btn_closeForm.Location = new System.Drawing.Point(605, 1);
            this.btn_closeForm.Name = "btn_closeForm";
            this.btn_closeForm.Size = new System.Drawing.Size(30, 30);
            this.btn_closeForm.TabIndex = 0;
            this.btn_closeForm.UseVisualStyleBackColor = true;
            this.btn_closeForm.Click += new System.EventHandler(this.btn_closeForm_Click);
            // 
            // lb_Time
            // 
            this.lb_Time.AutoSize = true;
            this.lb_Time.BackColor = System.Drawing.Color.Transparent;
            this.lb_Time.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_Time.Font = new System.Drawing.Font("微軟正黑體", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_Time.ForeColor = System.Drawing.Color.White;
            this.lb_Time.Location = new System.Drawing.Point(44, 91);
            this.lb_Time.Name = "lb_Time";
            this.lb_Time.Size = new System.Drawing.Size(125, 35);
            this.lb_Time.TabIndex = 3;
            this.lb_Time.Text = "00:00:00";
            this.lb_Time.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pl_CenterCircle
            // 
            this.pl_CenterCircle.BackColor = System.Drawing.Color.Transparent;
            this.pl_CenterCircle.BackgroundImage = global::VoiceCommandListener.Properties.Resources.jtjht_03;
            this.pl_CenterCircle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pl_CenterCircle.Controls.Add(this.lb_Time);
            this.pl_CenterCircle.Location = new System.Drawing.Point(209, 31);
            this.pl_CenterCircle.Name = "pl_CenterCircle";
            this.pl_CenterCircle.Size = new System.Drawing.Size(217, 212);
            this.pl_CenterCircle.TabIndex = 4;
            // 
            // AudioNotify
            // 
            this.AudioNotify.ContextMenuStrip = this.contextMenuStripForNofifyRight;
            this.AudioNotify.Icon = ((System.Drawing.Icon)(resources.GetObject("AudioNotify.Icon")));
            this.AudioNotify.Text = "远程指挥语音接收程序";
            this.AudioNotify.Visible = true;
            this.AudioNotify.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.AudioNotify_MouseClick);
            // 
            // contextMenuStripForNofifyRight
            // 
            this.contextMenuStripForNofifyRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMenuItem_ShowForm,
            this.tsMenuItem_HideForm,
            this.tsMenuItem_CloseForm});
            this.contextMenuStripForNofifyRight.Name = "contextMenuStripForNofifyRight";
            this.contextMenuStripForNofifyRight.Size = new System.Drawing.Size(153, 92);
            // 
            // tsMenuItem_ShowForm
            // 
            this.tsMenuItem_ShowForm.Name = "tsMenuItem_ShowForm";
            this.tsMenuItem_ShowForm.Size = new System.Drawing.Size(152, 22);
            this.tsMenuItem_ShowForm.Text = "显示界面";
            this.tsMenuItem_ShowForm.Click += new System.EventHandler(this.tsMenuItem_ShowForm_Click);
            // 
            // tsMenuItem_HideForm
            // 
            this.tsMenuItem_HideForm.Name = "tsMenuItem_HideForm";
            this.tsMenuItem_HideForm.Size = new System.Drawing.Size(152, 22);
            this.tsMenuItem_HideForm.Text = "隐藏界面";
            this.tsMenuItem_HideForm.Click += new System.EventHandler(this.tsMenuItem_HideForm_Click);
            // 
            // tsMenuItem_CloseForm
            // 
            this.tsMenuItem_CloseForm.Name = "tsMenuItem_CloseForm";
            this.tsMenuItem_CloseForm.Size = new System.Drawing.Size(152, 22);
            this.tsMenuItem_CloseForm.Text = "退出程序";
            this.tsMenuItem_CloseForm.Click += new System.EventHandler(this.tsMenuItem_CloseForm_Click);
            // 
            // VoiceCommandListenerDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::VoiceCommandListener.Properties.Resources.yhtfj_02;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(634, 281);
            this.ControlBox = false;
            this.Controls.Add(this.pl_CenterCircle);
            this.Controls.Add(this.btn_closeForm);
            this.Controls.Add(this.btn_min);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VoiceCommandListenerDemo";
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceCommandListenerDemo_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.VoiceCommandListenerDemo_MouseDown);
            this.MouseLeave += new System.EventHandler(this.VoiceCommandListenerDemo_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.VoiceCommandListenerDemo_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.VoiceCommandListenerDemo_MouseUp);
            this.pl_CenterCircle.ResumeLayout(false);
            this.pl_CenterCircle.PerformLayout();
            this.contextMenuStripForNofifyRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_min;
        private System.Windows.Forms.Button btn_closeForm;
        public System.Windows.Forms.Label lb_Time;
        private System.Windows.Forms.Panel pl_CenterCircle;
        private System.Windows.Forms.NotifyIcon AudioNotify;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForNofifyRight;
        private System.Windows.Forms.ToolStripMenuItem tsMenuItem_ShowForm;
        private System.Windows.Forms.ToolStripMenuItem tsMenuItem_HideForm;
        private System.Windows.Forms.ToolStripMenuItem tsMenuItem_CloseForm;
    }
}

