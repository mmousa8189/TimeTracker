﻿using Domain.Helpers;
using Domain.Infrastructure;
using Domain.Views;
using ModelViewPresenter.MessageDispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Controllers
{
    public class HolidayController : BaseController<Holiday>
    {
        public const int cID = 1 << 7;
        public override int ID { get { return cID; } }

        public const int ID_INDEX = 0;
        public const int DATE_INDEX = 1;
        public const int DESCRIPTION_INDEX = 2;
        public const int SYSTEM_CREATED_INDEX = 3;
        public const int SYSTEM_UPDATED_INDEX = 4;

        IHolidayView _holidayView;
        IDateHelper _helper;

        public override bool HandleRequest(ModelViewPresenter.MessageDispatcher.Telegram telegram)
        {
            if (telegram.Operation == Operation.OpenView)
            {
                this._holidayView.View_ViewReady(telegram.Data);
                this._holidayView.View_OnShow();
            }

            return true;
        }

        public HolidayController(IEFRepository repository, IHolidayView view)
            : base(repository, view)
        {
            this._helper = DateHelper.GetInstance();
            this._holidayView = view;
            this._holidayView.View_GetHolidayData = this.GetHolidayData;
            this._holidayView.View_ViewReady = ViewReady;
        }

        void ViewReady(dynamic data)
        {
            this._holidayView.View_OnViewReady(data);
        }

        void GetHolidayData(IEnumerable<Holiday> holidays)
        {
            var displayColumns = this._helper.GetHolidays(holidays);
            DateTime lastUpdatedDate = displayColumns
                .Select(x => x.SystemUpdated)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            this._holidayView.View_OnGetHolidayDataCompletion(displayColumns, lastUpdatedDate);
        }

        public override void GetData(Func<Holiday, bool> criteria)
        {
            IQueryable<Holiday> holidayQuery = this._repository
                .GetEntityQuery<Holiday>();

            if (criteria == null)
                this._view.View_QueryResults = holidayQuery.Select(x => x);
            else
                this._view.View_QueryResults = holidayQuery.Where(criteria);

            this._view.View_OnQueryRecordsCompletion();
        }

        public override void SaveData(Holiday data)
        {
            this._repository.Save<Holiday>(item => item.Id, data);
        }

        public override void DeleteData(Func<Holiday, bool> criteria)
        {
            this._repository.Delete(criteria);
        }
    }
}
