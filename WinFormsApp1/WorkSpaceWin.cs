using MapleLib.WzLib;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;

namespace WinFormsApp1
{
    internal class WorkSpaceWin : DockContent
    {
        FlowLayoutPanel toolPanel;
        Panel originalPanel;
        Panel newPanel;
        Panel finalPanel;
        TableLayoutPanel table;

        private DockPanel dockPanel;
        PendingListWin _pendingListWin;

        DataGridView gridA = new()
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false
        };
        DataGridView gridB = new()
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false
        };
        DataGridView gridC = new()
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
        };

        #region tools
        Label infoLabel = new Label() { AutoSize = true };
        Button btnNext = new Button() { Text = "下一条", AutoSize = true };
        Button btnPrevious = new Button() { Text = "上一条", AutoSize = true };

        Button btnNextConflict = new Button() { Text = "下一个需要处理", AutoSize = true };
        Button btnPreviousConflict = new Button() { Text = "上一个需要处理", AutoSize = true };
        #endregion


        List<string> _allNodes = [];
        WzImage _imgA;
        WzImage _imgB;
        ImageContext _imgC;
        public WorkSpaceWin(WzImage newlyImage)
        {
            _imgB = newlyImage;
            _imgA = WorkContext.Instance!.SourceFile.WzDirectory.GetImageByName(newlyImage.Name);
            _imgC = WorkContext.Instance.FinalData.GetValueOrDefault(_imgB.Name)!;
            this.Text = newlyImage.Name;

            table = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };


            // 行 50% / 50%
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            // 列 50% / 50%
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            toolPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            btnNext.Click += (o, s) =>
            {
                _currentNodeIndex++;
                ShowCurrentNode();
            };
            btnPrevious.Click += (o, s) =>
            {
                _currentNodeIndex--;
                ShowCurrentNode();
            };
            btnNextConflict.Click += HandleClickNextConflict;
            btnPreviousConflict.Click += HandleClickPreConflict;
            toolPanel.Controls.Add(infoLabel);
            toolPanel.Controls.Add(btnPrevious);
            toolPanel.Controls.Add(btnNext);
            toolPanel.Controls.Add(btnPreviousConflict);
            toolPanel.Controls.Add(btnNextConflict);
            // Panel A
            originalPanel = new Panel { BackColor = Color.LightBlue, Dock = DockStyle.Fill };

            // Panel B
            newPanel = new Panel { BackColor = Color.LightGreen, Dock = DockStyle.Fill };

            // Panel C
            finalPanel = new Panel { BackColor = Color.LightSalmon, Dock = DockStyle.Fill };

            // 添加到布局
            table.Controls.Add(toolPanel, 0, 0);
            table.SetColumnSpan(toolPanel, 2);

            table.Controls.Add(originalPanel, 0, 1);
            table.Controls.Add(newPanel, 1, 1);

            // C 占据整行
            table.Controls.Add(finalPanel, 0, 2);
            table.SetColumnSpan(finalPanel, 2);

            gridA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Type",
                Name = "Type",
                FillWeight = 20,
            });
            gridA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Path",
                Name = "Name",
                FillWeight = 30,
            });
            gridA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Value",
                Name = "Value",
                FillWeight = 50,
            });

            gridB.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Type",
                Name = "Type",
                FillWeight = 20,
            });
            gridB.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Path",
                Name = "Name",
                FillWeight = 30,
            });
            gridB.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Value",
                Name = "Value",
                FillWeight = 50,
            });

            gridC.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Type",
                Name = "Type",
                FillWeight = 20,
            });
            gridC.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Path",
                Name = "Name",
                FillWeight = 30,
            });
            gridC.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Value",
                Name = "Value",
                FillWeight = 50,
                
            });
            gridC.Columns[0]!.ReadOnly = true;
            gridC.Columns[1]!.ReadOnly = true;
            gridC.CellValueChanged += HandleFinalValueChanged;


            originalPanel.Controls.Add(gridA);

            newPanel.Controls.Add(gridB);

            finalPanel.Controls.Add(gridC);


            Controls.Add(dockPanel);

            this.Controls.Add(table);

        }

        private void HandleFinalValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            var path = gridC.Rows[e.RowIndex].Cells[1].Value!.ToString()!;
            var value = gridC.Rows[e.RowIndex].Cells[2].Value!.ToString() ?? "";
            _imgC.HandlePendingItems(_imgB.Name, path, value);

            ShowCurrentNode();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            dockPanel = new DockPanel();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.Theme = new VS2015LightTheme(); // VS 风格主题
            _pendingListWin = new PendingListWin();
            _pendingListWin.Show(dockPanel, DockState.DockLeftAutoHide);

            ResetDataSource();
        }

        public void ResetDataSource()
        {
            _imgA = WorkContext.Instance!.SourceFile.WzDirectory.GetImageByName(_imgB.Name);
            _imgC = WorkContext.Instance!.FinalData.GetValueOrDefault(_imgB.Name)!;
            _allNodes = _imgB.WzProperties.Select(x => x.Name)
                .Union(_imgA.WzProperties.Select(x => x.Name)).ToList();
            _currentNodeIndex = 0;
            ShowCurrentNode();
        }


        int _conflictIndex = 0;
        void HandleClickNextConflict(object? sender, EventArgs e)
        {
            _conflictIndex++;

            if (_imgC.TryGetPendingItemsByIndex(_conflictIndex, out var questName))
            {
                JumpTo(questName);
            }
        }

        void HandleClickPreConflict(object? sender, EventArgs e)
        {
            _conflictIndex--;

            if (_imgC.TryGetPendingItemsByIndex(_conflictIndex, out var questName))
            {
                JumpTo(questName);
            }
          
        }

        public void JumpTo(string currentName)
        {
            _currentNodeIndex = _allNodes.FindIndex(x => x == currentName);
            if (_currentNodeIndex == -1)
            {
                return;
            }
            var pendingList = _imgC.GetUnhandleItems();
            infoLabel.Text = $"共有{_allNodes.Count}条，有{pendingList.Count}条需要手动处理。当前QuestId={currentName}";

            var nodeA = _imgA.GetFromPath(currentName);

            var allA = ImageUtils.FlatSelectNode(nodeA);
            foreach (var item in allA)
            {
                item.Name = item.Name.Replace(WorkContext.Instance!.SourceFile.Name + "\\", "").ToString();
            }

            var allB = ImageUtils.FlatSelectNode(_imgB.GetFromPath(currentName));
            var allC = ImageUtils.FlatSelectNode(_imgC.Image.GetFromPath(currentName));


            gridA.Rows.Clear();
            gridB.Rows.Clear();
            gridC.Rows.Clear();

            var allProps = allA.Select(x => x.Name).Union(allB.Select(x => x.Name)).ToList();
            List<int> idxs = [];
            for (int i = 0; i < allProps.Count; i++)
            {
                var prop = allProps[i];

                var propA = allA.FirstOrDefault(x => x.Name == prop);
                var propB = allB.FirstOrDefault(x => x.Name == prop);
                var propC = allC.FirstOrDefault(x => x.Name == prop);

                var rowA = new DataGridViewRow();
                if (propA != null)
                {
                    rowA.Cells.Add(new DataGridViewTextBoxCell() { Value = propA.Type });
                    rowA.Cells.Add(new DataGridViewTextBoxCell() { Value = propA.Name });
                    rowA.Cells.Add(new DataGridViewTextBoxCell() { Value = propA.Value });
                }
                gridA.Rows.Add(rowA);

                var rowB = new DataGridViewRow();
                if (propB != null)
                {
                    rowB.Cells.Add(new DataGridViewTextBoxCell() { Value = propB.Type });
                    rowB.Cells.Add(new DataGridViewTextBoxCell() { Value = propB.Name });
                    rowB.Cells.Add(new DataGridViewTextBoxCell() { Value = propB.Value });
                }
                gridB.Rows.Add(rowB);

                if (propA?.Value != propB?.Value || propA?.Name != propB?.Name || propA?.Type != propB?.Type)
                {
                    rowA.DefaultCellStyle.BackColor = Color.LightGray;
                    rowB.DefaultCellStyle.BackColor = Color.LightGray;
                }

                var rowC = new DataGridViewRow();
                if (propC != null)
                {
                    rowC.Cells.Add(new DataGridViewTextBoxCell() { Value = propC.Type });
                    rowC.Cells.Add(new DataGridViewTextBoxCell() { Value = propC.Name });
                    rowC.Cells.Add(new DataGridViewTextBoxCell() { Value = propC.Value });

                    if (pendingList != null && pendingList.Any(x=>x.Value.SubProps.Any(y => y.FullPath == prop) && !x.Value.Processed))
                    {
                        rowC.DefaultCellStyle.BackColor = Color.LightPink;
                    }
                }
                gridC.Rows.Add(rowC);
            }
        }

        int _currentNodeIndex;
        public void ShowCurrentNode()
        {
            if (WorkContext.Instance == null)
            {
                return;
            }

            if (_allNodes.Count == 0)
            {
                return;
            }

            while (_currentNodeIndex < 0)
            {
                _currentNodeIndex += _allNodes.Count;
            }
            while (_currentNodeIndex >= _allNodes.Count)
            {
                _currentNodeIndex -= _allNodes.Count;
            }

            var questName = _allNodes[_currentNodeIndex];
            JumpTo(questName);
        }
    }
}
