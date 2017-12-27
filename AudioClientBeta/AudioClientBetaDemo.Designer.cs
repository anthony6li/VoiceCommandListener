namespace AudioClientBeta
{
    partial class AudioClientBetaDemo
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
            this.btn_min = new System.Windows.Forms.Button();
            this.btn_closeForm = new System.Windows.Forms.Button();
            this.lb_Time = new System.Windows.Forms.Label();
            this.pl_CenterCircle = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pl_CenterCircle.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_min
            // 
            this.btn_min.FlatAppearance.BorderSize = 0;
            this.btn_min.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_min.Image = global::AudioClientBeta.Properties.Resources.btn_min;
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
            this.btn_closeForm.Image = global::AudioClientBeta.Properties.Resources.btn_close;
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
            this.pl_CenterCircle.BackgroundImage = global::AudioClientBeta.Properties.Resources.jtjht_03;
            this.pl_CenterCircle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pl_CenterCircle.Controls.Add(this.lb_Time);
            this.pl_CenterCircle.Location = new System.Drawing.Point(209, 31);
            this.pl_CenterCircle.Name = "pl_CenterCircle";
            this.pl_CenterCircle.Size = new System.Drawing.Size(217, 212);
            this.pl_CenterCircle.TabIndex = 4;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(432, 37);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(190, 206);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // AudioClientBetaDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::AudioClientBeta.Properties.Resources.yhtfj_02;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(634, 281);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.pl_CenterCircle);
            this.Controls.Add(this.btn_closeForm);
            this.Controls.Add(this.btn_min);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AudioClientBetaDemo";
            this.Text = "AudioClientDemo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioClientBetaDemo_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AudioClientBetaDemo_MouseDown);
            this.MouseLeave += new System.EventHandler(this.AudioClientBetaDemo_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AudioClientBetaDemo_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AudioClientBetaDemo_MouseUp);
            this.pl_CenterCircle.ResumeLayout(false);
            this.pl_CenterCircle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_min;
        private System.Windows.Forms.Button btn_closeForm;
        private System.Windows.Forms.Label lb_Time;
        private System.Windows.Forms.Panel pl_CenterCircle;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

