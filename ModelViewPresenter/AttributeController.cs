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
    public class AttributeController : BaseController<Domain.Attribute>
    {
        public const int cID = 1 << 11;
        public override int ID { get { return cID; } }
        public const int ID_INDEX = 0;
        public const int NAME_INDEX = 1;
        public const int DESCRIPTION_INDEX = 2;
        public const int LINK_INDEX = 3;
        public const int SYSTEM_CREATED_INDEX = 4;
        public const int SYSTEM_UPDATED_INDEX = 5;

        IAttributeView _view;
        IDateHelper _helper;
        public override bool HandleRequest(ModelViewPresenter.MessageDispatcher.Telegram telegram)
        {
            if (telegram.Operation == Operation.OpenView)
            {
                this._view.View_ViewReady(telegram.Data);
                this._view.View_OnShow();
            }

            return true;
        }

        public AttributeController(IEFRepository repository, IAttributeView view)
            : base(repository, view)
        {
            this._helper = DateHelper.GetInstance();
            this._view = view;
            this._view.View_GetAttributeData = this.GetAttributeData;
            this._view.View_ViewReady = ViewReady;
        }

        void ViewReady(dynamic data)
        {
            this._view.View_OnViewReady(data);
        }

        void GetAttributeData(IEnumerable<Domain.Attribute> attributes)
        {
            DateTime lastUpdatedDate = attributes
                .Select(x => x.SystemUpdateDateTime)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            this._view.View_OnGetAttributeDataCompletion(
                attributes
                .OrderBy(x => x.Id)
                .ToList()
                ,
                lastUpdatedDate);
        }

        public override void GetData(Func<Domain.Attribute, bool> criteria)
        {
            IQueryable<Domain.Attribute> attributeQuery = this._repository
                .GetEntityQuery<Domain.Attribute>();

            if (criteria == null)
                this._view.View_QueryResults = attributeQuery.Select(x => x);
            else
                this._view.View_QueryResults = attributeQuery.Where(criteria);

            this._view.View_OnQueryRecordsCompletion();
        }

        public override void SaveData(Domain.Attribute data)
        {
            this._repository.Save<Domain.Attribute>(item => item.Id, data);
        }

        public override void DeleteData(Func<Domain.Attribute, bool> criteria)
        {
            this._repository.Delete(criteria);
        }
    }
}
