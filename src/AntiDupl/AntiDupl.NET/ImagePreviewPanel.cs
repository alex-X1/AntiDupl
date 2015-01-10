/*
* AntiDupl.NET Program.
*
* Copyright (c) 2002-2014 Yermalayeu Ihar, 2013-2014 Borisov Dmitry.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace AntiDupl.NET
{
    public class ImagePreviewPanel : TableLayoutPanel
    {
        private const int MAX_PATH = 260;

        public enum Position
        {
            Left,
            Top,
            Right,
            Bottom
        }
        private Position m_position;

        private CoreDll.RenameCurrentType m_renameCurrentType;
        public CoreDll.RenameCurrentType RenameCurrentType {get{return m_renameCurrentType;}}
        
        private const int IBW = 1;//Internal border width
        private const int EBW = 2;//External border width

        private CoreLib m_core;
        private Options m_options;
        private ResultsListView m_resultsListView;
        
        private CoreImageInfo m_currentImageInfo;
        public CoreImageInfo CurrentImageInfo { get { return m_currentImageInfo; } }
        private CoreImageInfo m_neighbourImageInfo;
        public CoreImageInfo NeighbourImageInfo { get { return m_neighbourImageInfo; } }
        
        private PictureBoxPanel m_pictureBoxPanel;
        private Label m_fileSizeLabel;
        private Label m_imageSizeLabel;
        private Label m_imageTypeLabel;
        private Label m_imageBlocknessLabel;
        private Label m_imageBlurringLabel;
        private Label m_pathLabel;
        private ToolTip m_toolTip;

        public ImagePreviewPanel(CoreLib core, Options options, ResultsListView resultsListView, Position position)
        {
            m_core = core;
            m_options = options;
            m_resultsListView = resultsListView;
            InitializeComponents();
            SetPosition(position);
        }
        
        private void InitializeComponents()
        {
            Strings s = Resources.Strings.Current;

            Location = new System.Drawing.Point(0, 0);
            Margin = new Padding(0);
            Padding = new Padding(0);
            Dock = DockStyle.Fill;
            
            ColumnCount = 1;
            RowCount = 2;

            m_pictureBoxPanel = new PictureBoxPanel(m_core, m_options);
            m_pictureBoxPanel.ContextMenuStrip = new ImagePreviewContextMenu(m_core, m_options, this, m_resultsListView);
            
            m_fileSizeLabel = new Label();
            m_fileSizeLabel.Dock = DockStyle.Fill;
            m_fileSizeLabel.BorderStyle = BorderStyle.Fixed3D;
            m_fileSizeLabel.Padding = new Padding(1, 3, 1, 0);
            m_fileSizeLabel.TextAlign = ContentAlignment.TopCenter;
            m_fileSizeLabel.AutoSize = true;
            
            m_imageSizeLabel = new Label();
            m_imageSizeLabel.Dock = DockStyle.Fill;
            m_imageSizeLabel.BorderStyle = BorderStyle.Fixed3D;
            m_imageSizeLabel.Padding = new Padding(1, 3, 1, 0);
            m_imageSizeLabel.Margin = new Padding(IBW, 0, 0, 0);
            m_imageSizeLabel.TextAlign = ContentAlignment.TopCenter;
            m_imageSizeLabel.AutoSize = true;

            m_imageBlocknessLabel = new Label();
            m_imageBlocknessLabel.Dock = DockStyle.Fill;
            m_imageBlocknessLabel.BorderStyle = BorderStyle.Fixed3D;
            m_imageBlocknessLabel.Padding = new Padding(1, 3, 1, 0);
            m_imageBlocknessLabel.Margin = new Padding(IBW, 0, 0, 0);
            m_imageBlocknessLabel.TextAlign = ContentAlignment.TopCenter;
            m_imageBlocknessLabel.AutoSize = true;

            m_imageBlurringLabel = new Label();
            m_imageBlurringLabel.Dock = DockStyle.Fill;
            m_imageBlurringLabel.BorderStyle = BorderStyle.Fixed3D;
            m_imageBlurringLabel.Padding = new Padding(1, 3, 1, 0);
            m_imageBlurringLabel.Margin = new Padding(IBW, 0, 0, 0);
            m_imageBlurringLabel.TextAlign = ContentAlignment.TopCenter;
            m_imageBlurringLabel.AutoSize = true;

            m_imageTypeLabel = new Label();
            m_imageTypeLabel.Dock = DockStyle.Fill;
            m_imageTypeLabel.BorderStyle = BorderStyle.Fixed3D;
            m_imageTypeLabel.Padding = new Padding(1, 3, 1, 0);
            m_imageTypeLabel.Margin = new Padding(IBW, 0, 0, 0);
            m_imageTypeLabel.TextAlign = ContentAlignment.TopCenter;
            m_imageTypeLabel.AutoSize = true;

            m_pathLabel = new Label();
            m_pathLabel.Location = new Point(0, 0);
            m_pathLabel.Dock = DockStyle.Fill;
            m_pathLabel.BorderStyle = BorderStyle.Fixed3D;
            m_pathLabel.Padding = new Padding(1, 3, 1, 0);
            m_pathLabel.AutoEllipsis = true;
            m_pathLabel.DoubleClick += new EventHandler(RenameImage);

            m_toolTip = new ToolTip();
            m_toolTip.ShowAlways = true;
            m_toolTip.SetToolTip(m_imageBlocknessLabel, s.ResultsListView_Blockiness_Column_Text);
            m_toolTip.SetToolTip(m_imageBlurringLabel, s.ResultsListView_Blurring_Column_Text);
        }

        /// <summary>
        /// Set information in image panel.
        /// ��������� ���������� � ������ �����������.
        /// </summary>
        private void SetImageInfo(CoreImageInfo currentImageInfo, CoreImageInfo neighbourImageInfo)
        {
            bool updateCurrent = UpdateImageInfo(ref m_currentImageInfo, currentImageInfo);
            bool updateNeighbour = UpdateImageInfo(ref m_neighbourImageInfo, neighbourImageInfo);
            if (updateCurrent)
            {
                m_pictureBoxPanel.UpdateImage(currentImageInfo);
                m_fileSizeLabel.Text = m_currentImageInfo.GetFileSizeString();
                m_imageSizeLabel.Text = m_currentImageInfo.GetImageSizeString();
                m_imageBlocknessLabel.Text = m_currentImageInfo.GetBlockinessString();
                m_imageBlurringLabel.Text = m_currentImageInfo.GetBlurringString();
                m_imageTypeLabel.Text = m_currentImageInfo.type == CoreDll.ImageType.None ? "   " : m_currentImageInfo.GetImageTypeString();
                m_pathLabel.Text = m_currentImageInfo.path;
                if (m_neighbourImageInfo != null) //��������� highlight
                {
                    m_imageSizeLabel.ForeColor =
                            m_currentImageInfo.height * m_currentImageInfo.width < m_neighbourImageInfo.height * m_neighbourImageInfo.width ?
                            Color.Red : TableLayoutPanel.DefaultForeColor;
                    m_imageTypeLabel.ForeColor = m_currentImageInfo.type != m_neighbourImageInfo.type ?
                            Color.Red : TableLayoutPanel.DefaultForeColor;
                    m_fileSizeLabel.ForeColor = m_currentImageInfo.size < m_neighbourImageInfo.size ?
                            Color.Red : TableLayoutPanel.DefaultForeColor;
                    m_imageBlocknessLabel.ForeColor = m_currentImageInfo.blockiness > m_neighbourImageInfo.blockiness ?
                            Color.Red : TableLayoutPanel.DefaultForeColor;
                    m_imageBlurringLabel.ForeColor = m_currentImageInfo.blurring > m_neighbourImageInfo.blurring ?
                            Color.Red : TableLayoutPanel.DefaultForeColor;
                }
            }
            else if (m_neighbourImageInfo != null)
            {
               m_imageTypeLabel.ForeColor = m_currentImageInfo.type != m_neighbourImageInfo.type ?
                      Color.Red : TableLayoutPanel.DefaultForeColor;
               m_imageBlocknessLabel.ForeColor = m_currentImageInfo.blockiness > m_neighbourImageInfo.blockiness ?
                      Color.Red : TableLayoutPanel.DefaultForeColor;
               m_imageBlurringLabel.ForeColor = m_currentImageInfo.blurring > m_neighbourImageInfo.blurring ?
                      Color.Red : TableLayoutPanel.DefaultForeColor;
            }
            if (updateCurrent || updateNeighbour)
            {
                Size neighbourSizeMax = new Size(0, 0);
                if(m_neighbourImageInfo != null)
                    neighbourSizeMax = new Size((int)m_neighbourImageInfo.width, (int)m_neighbourImageInfo.height);
                m_pictureBoxPanel.UpdateImagePadding(neighbourSizeMax);
                Refresh();
            }
        }
        
        static private bool UpdateImageInfo(ref CoreImageInfo oldImageInfo, CoreImageInfo newImageInfo)
        {
            if (oldImageInfo == null || 
                oldImageInfo.path.CompareTo(newImageInfo.path) != 0 ||
                oldImageInfo.size != newImageInfo.size || 
                oldImageInfo.time != newImageInfo.time)
            {
                oldImageInfo = newImageInfo;
                return true;
            }
            return false;
        }

        public void SetResult(CoreResult result)
        {
            if(result.type == CoreDll.ResultType.None)
                throw new Exception("Bad result type!");

            switch(m_position)
            {
            case Position.Left:
            case Position.Top:
                if (result.type == CoreDll.ResultType.DuplImagePair)
                    SetImageInfo(result.first, result.second);
                else
                    SetImageInfo(result.first, null);

                break;
            case Position.Right:
            case Position.Bottom:
                if (result.type == CoreDll.ResultType.DuplImagePair)
                    SetImageInfo(result.second, result.first);
                else
                    SetImageInfo(result.second, null);
                break;
            }
        }
        
        /// <summary>
        /// Adding controls in panel
        /// ���������� ����������� �� ������
        /// </summary>
        public void SetPosition(Position position)
        {
            m_position = position;
            switch (m_position)
            {
                case Position.Left:
                case Position.Top:
                    m_renameCurrentType = CoreDll.RenameCurrentType.First;
                    break;
                case Position.Right:
                case Position.Bottom:
                    m_renameCurrentType = CoreDll.RenameCurrentType.Second;
                    break;
            }
            
            TableLayoutPanel infoLayout = InitFactory.Layout.Create(6, 1); //number of controls in panel
            infoLayout.Height = m_imageSizeLabel.Height;
            if (m_position != Position.Left)
            {
                m_pathLabel.TextAlign = ContentAlignment.TopLeft;
            
                m_fileSizeLabel.Margin = new Padding(EBW, 0, 0, 0);
                m_pathLabel.Margin = new Padding(IBW, 0, EBW, 0);

                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//fileSizeLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageSizeLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageBlocknessLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageBlurringLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageTypeLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));//pathLabel

                infoLayout.Controls.Add(m_fileSizeLabel, 0, 0);
                infoLayout.Controls.Add(m_imageSizeLabel, 1, 0);
                infoLayout.Controls.Add(m_imageBlocknessLabel, 2, 0);
                infoLayout.Controls.Add(m_imageBlurringLabel, 3, 0);
                infoLayout.Controls.Add(m_imageTypeLabel, 4, 0); 
                infoLayout.Controls.Add(m_pathLabel, 5, 0);
            }
            else
            {
                m_pathLabel.TextAlign = ContentAlignment.TopRight;
                
                m_pathLabel.Margin = new Padding(EBW, 0, 0, 0);
                m_fileSizeLabel.Margin = new Padding(IBW, 0, EBW, 0);

                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));//pathLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageTypeLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageBlurringLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageBlocknessLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//imageSizeLabel
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//fileSizeLabel

                infoLayout.Controls.Add(m_pathLabel, 0, 0);
                infoLayout.Controls.Add(m_imageTypeLabel, 1, 0);
                infoLayout.Controls.Add(m_imageBlurringLabel, 2, 0);
                infoLayout.Controls.Add(m_imageBlocknessLabel, 3, 0); 
                infoLayout.Controls.Add(m_imageSizeLabel, 4, 0);
                infoLayout.Controls.Add(m_fileSizeLabel, 5, 0);
            }

            Controls.Clear();
            RowStyles.Clear();
            if(m_position == Position.Bottom)
            {
                m_pictureBoxPanel.Margin = new Padding(EBW, IBW, EBW, EBW);
                infoLayout.Margin = new Padding(0, EBW, 0, 0);
                
                RowStyles.Add(new RowStyle(SizeType.AutoSize));
                RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                Controls.Add(infoLayout, 0, 0);
                Controls.Add(m_pictureBoxPanel, 0, 1);
            }
            else
            {
                m_pictureBoxPanel.Margin = new Padding(EBW, EBW, EBW, IBW);
                infoLayout.Margin = new Padding(0, 0, 0, EBW);
                
                RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                RowStyles.Add(new RowStyle(SizeType.AutoSize));
                Controls.Add(m_pictureBoxPanel, 0, 0);
                Controls.Add(infoLayout, 0, 1);
            }
        }

        public void RenameImage(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(m_currentImageInfo.path);
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = fileInfo.FullName;
            dialog.OverwritePrompt = false;
            dialog.AddExtension = true;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = fileInfo.Extension;
            dialog.FileOk += new System.ComponentModel.CancelEventHandler(OnRenameImageDialogFileOk);
            dialog.Title = Resources.Strings.Current.ImagePreviewContextMenu_RenameImageItem_Text;
            dialog.InitialDirectory = fileInfo.Directory.ToString();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                m_resultsListView.RenameCurrent(m_renameCurrentType, dialog.FileName);
            }
        }

        private void OnRenameImageDialogFileOk(object sender, CancelEventArgs e)
        {
            SaveFileDialog dialog = (SaveFileDialog)sender;
            FileInfo oldFileInfo = new FileInfo(m_currentImageInfo.path);
            FileInfo newFileInfo = new FileInfo(dialog.FileName);
            if (newFileInfo.FullName != oldFileInfo.FullName && newFileInfo.Exists)
            {
                MessageBox.Show(Resources.Strings.Current.ErrorMessage_FileAlreadyExists,
                    dialog.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
            else if (newFileInfo.Extension != oldFileInfo.Extension && newFileInfo.Extension.Length > 0)
            {
                e.Cancel = MessageBox.Show(Resources.Strings.Current.WarningMessage_ChangeFileExtension, 
                    dialog.Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel;
            }
        }
    }
}