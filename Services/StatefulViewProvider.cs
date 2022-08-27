using Bermuda.Interfaces;
using Bermuda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Services
{
    public class StatefulViewProvider : IStatefulViewProvider
    {

        private TaskViewModel _taskViewModel;

        public void StoreTaskViewModel(TaskViewModel taskViewModel)
        {
            this._taskViewModel = taskViewModel;
        }

        public TaskViewModel GetTaskViewModel()
        {
            return this._taskViewModel;
        }

    }
}
