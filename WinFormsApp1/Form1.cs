using MapleLib.WzLib;
using System;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private DockPanel dockPanel;
        public Form1()
        {
            InitializeComponent();

            dockPanel = new DockPanel();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.Theme = new VS2015LightTheme(); // VS 风格主题
            this.toolStripContainer1.ContentPanel.Controls.Add(dockPanel);

            tool = new OriginalDataWin(this);
        }


        OriginalDataWin tool;
        OutputWin output = new();
        private void Form1_Load(object sender, EventArgs e)
        {
            tool.Show(dockPanel, DockState.DockLeftAutoHide);


            output.Show(dockPanel, DockState.DockBottomAutoHide);
        }

        WzFile? _workingWz;
        private void Menu_SelectWz_Click(object sender, EventArgs e)
        {
            var selectFileDialog = new OpenFileDialog()
            {
                Filter = "wz文件(*.wz)|*.wz"
            };
            var r = selectFileDialog.ShowDialog();
            if (r == DialogResult.OK)
            {
                var versionWin = new WzVersionInputWin();
                versionWin.OnSubmit += (s, o) =>
                {
                    try
                    {
                        _workingWz = new WzFile(selectFileDialog.FileName, o.GameVersion, o.Version);

                        _workingWz.ParseWzFile();
                        if (!_workingWz.WzDirectory.Name.Equals("Quest.wz", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("仅支持Quest.wz");
                        }

                        WorkContext.Instance = new WorkContext(_workingWz);

                        tool.DrawData();
                        ReloadDocuments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"仅支持Quest.wz: {ex.Message}");
                    }
                };
                versionWin.ShowDialog();
            }
        }

        Dictionary<string, WorkSpaceWin> _allDocuments = [];
        public void ShowDocument(WzImage newlyImage)
        {
            if (!_allDocuments.TryGetValue(newlyImage.Name, out var doc))
            {
                doc = new WorkSpaceWin(newlyImage) { CloseButton = false };
                
                _allDocuments[newlyImage.Name] = doc;
                doc.Show(dockPanel, DockState.Document);
            }
            else
            {
                doc.ResetDataSource();
            }
            
        }

        public void ReloadDocuments()
        {
            foreach (var item in _allDocuments)
            {
                item.Value.ResetDataSource();
            }
        }

        private void Menu_Run_Click(object sender, EventArgs e)
        {
            //if (WorkContext.Instance == null)
            //{
            //    MessageBox.Show("请先选择 开始->选择Quest.wz");
            //    return;
            //}
            //foreach (var item in WorkContext.Instance.NewData)
            //{
            //    if (item.Value == null)
            //    {
            //        continue;
            //    }
            //    var context = new ImageContext(
            //        WorkContext.Instance.SourceFile.WzDirectory.GetImageByName(item.Key).DeepClone());
            //    WorkContext.Instance.FinalData[item.Key] = context;
            //    ApplyQuestImage(context, item.Value);
            //}

            //ReloadDocuments();
        }


    }
}
