using Bermuda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Interfaces
{
    public interface IStatefulViewProvider
    {
        void StoreTaskViewModel(TaskViewModel taskViewModel);
        TaskViewModel GetTaskViewModel();
    }
}
