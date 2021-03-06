﻿using Domain;
using Domain.Controllers;
using Domain.Infrastructure;
using Domain.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainApp
{
    public partial class frmDailyAttribute : frmCommonByDateDataEditor, IFormCommonOperation, IDailyAttributeView
    {
        public Action<Func<DayAttribute, bool>> View_QueryRecords { get; set; }
        public Action View_OnQueryRecordsCompletion { get; set; }
        public Action<DayAttribute> View_SaveRecord { get; set; }
        public Action<Func<DayAttribute, bool>> View_DeleteRecords { get; set; }
        public IEnumerable<DayAttribute> View_QueryResults { get; set; }
        public Action View_GetPresetAttributeData { get; set; }
        public Action<IEnumerable<Domain.Attribute>> View_OnGetPresetAttributeDataCompletion { get; set; }
        public Action<IEnumerable<DayAttribute>> View_GetDailyAttributeData { get; set; }
        public Action<dynamic, DateTime> View_OnGetDailyAttributeDataCompletion { get; set; }
        public Action<object> View_ViewReady { get; set; }
        public Action<object> View_OnViewReady { get; set; }
        public Action View_OnShow { get; set; }

        Form _parentForm;

        IEnumerable<Domain.Attribute> _presetAttributes;

        public frmDailyAttribute() : base()
        {
            InitializeComponent();
            this.RegisterCommonOperation(this);

            this.View_OnQueryRecordsCompletion = this.RefreshGridData;
            this.View_OnGetPresetAttributeDataCompletion = this.PopulateAttributePresets;
            this.View_OnGetDailyAttributeDataCompletion = this.UpdateDailyAttributeData;
            this.View_OnViewReady = OnViewReady;
            this.View_OnShow = OnShow;
        }

        void OnShow()
        {
            MethodInvoker invokeFromUI = new MethodInvoker(
               () =>
               {
                   try
                   {
                       this.ShowDialog(this._parentForm);
                   }
                   catch (Exception ex)
                   {
                       throw ex;
                   }
               }
           );

            if (this.InvokeRequired)
                this.Invoke(invokeFromUI);
            else
                invokeFromUI.Invoke();
        }

        void OnViewReady(object data)
        {
            this._parentForm = (Form)data.GetType().GetProperty("parentForm").GetValue(data, null);

            this.View_QueryRecords(null);
            this.View_GetPresetAttributeData();
        }

        void PopulateAttributePresets(IEnumerable<Domain.Attribute> attributes)
        {
            this._presetAttributes = attributes;

            List<string> comboBoxItems = new List<string>();

            comboBoxItems.Add("N/A");
            comboBoxItems.AddRange(this._presetAttributes.Select(x => x.Name).ToList());

            this.cboPresetAttributes.DataSource = comboBoxItems;
        }

        void RefreshGridData()
        {
            IEnumerable<DayAttribute> dailyAttributes = this.View_QueryResults;

            this.View_GetDailyAttributeData(dailyAttributes);
        }

        public void UpdateWindow(int rowIndex)
        {
            try
            {
                int id = int.Parse(this.recordGrid.Rows[rowIndex].Cells[DailyAttributeController.ID_INDEX].Value.ToString());
                DayAttribute dailyAttribute = this.View_QueryResults
                    .Where(x => x.Id == id)
                    .FirstOrDefault();

                this.lblId.Text = dailyAttribute.Id.ToString();
                this.date.Value = dailyAttribute.Date.Date;
                this.txtDescription.Text = dailyAttribute.Description;
                this.txtLink.Text = dailyAttribute.Link;
            }
            catch (ArgumentOutOfRangeException) { /*Skip*/}
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void UpdateDailyAttributeData(dynamic displayColumns, DateTime lastUpdatedDate)
        {
            this.recordGrid.DataSource = displayColumns;

            this.recordGrid.Refresh();
            this.HighlightRecordByDate(lastUpdatedDate);
        }

        void HighlightRecordByDate(DateTime recordDate)
        {
            for (int index = 0; index < this.recordGrid.Rows.Count; index++)
            {
                try
                {
                    DateTime systemDate = DateTime.Parse(this.recordGrid.Rows[index].Cells[DailyAttributeController.SYSTEM_UPDATED_INDEX].Value.ToString());
                    bool identicalTime = recordDate.ToLongTimeString() == systemDate.ToLongTimeString();

                    if ((recordDate.Date == systemDate.Date) && identicalTime)
                    {
                        this.recordGrid.CurrentCell = this.recordGrid[DailyAttributeController.ID_INDEX, index];
                        this.recordGrid.Rows[index].Selected = true;
                        this.recordGrid.Rows[index].Cells[DailyAttributeController.ID_INDEX].Selected = true;
                        this.recordGrid.FirstDisplayedScrollingRowIndex = index;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            this.recordGrid.Update();
        }

        public void EnableInputWindow(bool enable)
        {
            this.date.Enabled = enable;
            this.txtDescription.ReadOnly = !enable;
            this.txtLink.ReadOnly = !enable;
            this.cboPresetAttributes.Enabled = enable;
        }

        public void ResetInputWindow()
        {
            this.cboPresetAttributes.SelectedIndex = 0;
            this.lblId.Text = string.Empty;
            this.date.Value = DateTime.Now;
            this.txtDescription.Clear();
            this.txtLink.Clear();
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
            DayAttribute attribute = new DayAttribute
            {
                System_Created = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(this.lblId.Text))
                attribute = this.View_QueryResults
                    .Where(x => x.Id == int.Parse(this.lblId.Text))
                    .FirstOrDefault();

            attribute.Date = this.date.Value.Date;
            attribute.Description = this.txtDescription.Text;
            attribute.Link = this.txtLink.Text;
            attribute.SystemUpdateDateTime = DateTime.Now;

            this.View_SaveRecord(attribute);
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

        void SetControlValues(Domain.Attribute attribute)
        {
            if (attribute == null)
                return;

            this.txtLink.Text = attribute.Link;
            this.txtDescription.Text = attribute.Description;
        }

        private void cboPresetAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Domain.Attribute attribute = this._presetAttributes
                .Where(x => x.Name == (string) this.cboPresetAttributes.SelectedItem)
                .FirstOrDefault();

            this.SetControlValues(attribute);
        }

        public void DecorateGrid()
        {
            IDataGridHelper helper = DataGridHelper.GetInstance();

            helper.SetAutoResizeCells(ref this.recordGrid);
        }
    }
}
