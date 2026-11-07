namespace it1.WF
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSelect = new Button();
            lblFile = new Label();
            btnImport = new Button();
            btnCancel = new Button();
            btnShow = new Button();
            cmbAnalytics = new ComboBox();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            dataGridView = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            SuspendLayout();
            // 
            // btnSelect
            // 
            btnSelect.Location = new Point(12, 12);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(108, 29);
            btnSelect.TabIndex = 0;
            btnSelect.Text = "Select Json";
            btnSelect.UseVisualStyleBackColor = true;
            // 
            // lblFile
            // 
            lblFile.AutoSize = true;
            lblFile.Location = new Point(12, 60);
            lblFile.Name = "lblFile";
            lblFile.Size = new Size(70, 20);
            lblFile.TabIndex = 1;
            lblFile.Text = "Show file";
            // 
            // btnImport
            // 
            btnImport.Location = new Point(141, 12);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(108, 29);
            btnImport.TabIndex = 2;
            btnImport.Text = "Start Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(141, 51);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(108, 29);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnShow
            // 
            btnShow.Location = new Point(522, 12);
            btnShow.Name = "btnShow";
            btnShow.Size = new Size(108, 29);
            btnShow.TabIndex = 8;
            btnShow.Text = "Показати";
            btnShow.UseVisualStyleBackColor = true;
            // 
            // cmbAnalytics
            // 
            cmbAnalytics.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAnalytics.FormattingEnabled = true;
            cmbAnalytics.Items.AddRange(new object[] { "Аномалії вартості та дат", "Подвійні бронювання", "Виручка за містами", "Зведення за хостами" });
            cmbAnalytics.Location = new Point(281, 12);
            cmbAnalytics.Name = "cmbAnalytics";
            cmbAnalytics.Size = new Size(230, 28);
            cmbAnalytics.TabIndex = 7;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(650, 12);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(125, 29);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 4;
            progressBar.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoEllipsis = true;
            lblStatus.Location = new Point(281, 60);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(490, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Status";
            // 
            // dataGridView
            // 
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Location = new Point(12, 104);
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersWidth = 51;
            dataGridView.Size = new Size(1086, 334);
            dataGridView.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1110, 450);
            Controls.Add(dataGridView);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnShow);
            Controls.Add(cmbAnalytics);
            Controls.Add(btnCancel);
            Controls.Add(btnImport);
            Controls.Add(lblFile);
            Controls.Add(btnSelect);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelect;
        private Label lblFile;
        private Button btnImport;
        private Button btnCancel;
        private Button btnShow;
        private ComboBox cmbAnalytics;
        private ProgressBar progressBar;
        private Label lblStatus;
        private DataGridView dataGridView;
    }
}
