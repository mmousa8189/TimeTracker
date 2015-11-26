﻿using Domain;
using Domain.Controllers;
using Domain.Infrastructure;
using Domain.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MainApp
{
    public partial class frmCategory : frmCommonDataEditor, ICategoryView, IFormCommonOperation
    {
        public Action<Func<Category, bool>> View_QueryRecords { get; set; }
        public Action View_OnQueryRecordsCompletion { get; set; }
        public Action<Category> View_SaveRecord { get; set; }
        public Action<Func<Category, bool>> View_DeleteRecords { get; set; }
        public IEnumerable<Category> View_QueryResults { get; set; }
        public Action<IEnumerable<Category>> View_GetCategoryData { get; set; }
        public Action<dynamic, DateTime> View_OnGetCategoryDataCompletion { get; set; }

        public frmCategory(IEFRepository repository)
        {
            InitializeComponent();

            Action RegisterController = () => new CategoryController(repository, this);

            RegisterController();
            this.RegisterCommonOperation(this);

            this.View_OnQueryRecordsCompletion = this.RefreshGridData;
            this.View_OnGetCategoryDataCompletion = this.UpdateCategoryData;

            this.View_QueryRecords(null);
        }

        void RefreshGridData()
        {
            IEnumerable<Category> categories = this.View_QueryResults;

            this.View_GetCategoryData(categories);
        }

        void UpdateCategoryData(dynamic displayColumns, DateTime lastUpdatedDate)
        {
            this.dGrid.DataSource = displayColumns;

            this.dGrid.Refresh();
            this.HighlightRecordByDate(lastUpdatedDate);
        }

        void HighlightRecordByDate(DateTime recordDate)
        {
            for (int index = 0; index < this.dGrid.Rows.Count; index++)
            {
                try
                {
                    DateTime systemDate = DateTime.Parse(this.dGrid.Rows[index].Cells[HolidayController.SYSTEM_UPDATED_INDEX].Value.ToString());
                    bool identicalTime = recordDate.ToLongTimeString() == systemDate.ToLongTimeString();

                    if ((recordDate.Date == systemDate.Date) && identicalTime)
                    {
                        this.dGrid.CurrentCell = this.dGrid[HolidayController.ID_INDEX, index];
                        this.dGrid.Rows[index].Selected = true;
                        this.dGrid.Rows[index].Cells[HolidayController.ID_INDEX].Selected = true;
                        this.dGrid.FirstDisplayedScrollingRowIndex = index;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            this.dGrid.Update();
        }

        public void UpdateWindow(int rowIndex)
        {
            try
            {
                int id = int.Parse(this.dGrid.Rows[rowIndex].Cells[CategoryController.ID_INDEX].Value.ToString());
                Category category = this.View_QueryResults
                    .Where(x => x.Id == id)
                    .FirstOrDefault();

                this.lblId.Text = category.Id.ToString();
                this.txtCategoryName.Text = category.Name;
                this.chkShowInSummary.Checked = category.ShowInSummary;
            }
            catch (ArgumentOutOfRangeException) { /*Skip*/}
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EnableInputWindow(bool enable)
        {
            this.txtCategoryName.Enabled = enable;
            this.chkShowInSummary.Enabled = enable;
        }

        public void ResetInputWindow()
        {
            this.lblId.Text = string.Empty;
            this.txtCategoryName.Clear();
            this.chkShowInSummary.Checked = false;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                int id = int.Parse(this.lblId.Text);

                if (id == 0)
                    throw new FormatException();

                this.WindowInputChanges(ModifierState.Edit);
            }
            catch (FormatException)
            {
                MessageBox.Show("A record selection is required for editing");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int id = int.Parse(this.lblId.Text);

                if (id == 0)
                    throw new FormatException();

                DialogResult result = MessageBox.Show("Delete record?", "Delete Record Verification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    this.View_DeleteRecords(x => x.Id == id);
                    this.View_QueryRecords(null);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("A record selection is required for deletion");
            }

            this.WindowInputChanges(ModifierState.Delete);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Category category = new Category
            {
                System_Created = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(this.lblId.Text))
                category = this.View_QueryResults
                    .Where(x => x.Id == int.Parse(this.lblId.Text))
                    .FirstOrDefault();

            category.Name = this.txtCategoryName.Text;
            category.ShowInSummary = this.chkShowInSummary.Checked;
            category.SystemUpdateDateTime = DateTime.Now;

            this.View_SaveRecord(category);
            this.View_QueryRecords(null);
            this.WindowInputChanges(ModifierState.Save);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.WindowInputChanges(ModifierState.Add);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.WindowInputChanges(ModifierState.Cancel);
        }
    }
}
