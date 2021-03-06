﻿using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Views
{
    public interface IActivityView : IView<Activity>
    {
        Action<IEnumerable<Activity>> View_GetActivityData { get; set; }
        Action<dynamic, DateTime> View_OnGetActivityDataCompletion { get; set; }
    }
}
