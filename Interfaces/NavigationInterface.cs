using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bermuda.Interfaces
{
    public interface INavigationService
    {
        string CurrentPage { get; }
        void SetCleanupRoutine(Func<bool> cleanup);
        void NavigateTo(string page);
        void NavigateTo(string page, object parameter);
        void GoBack();
        void SetFrame(Frame frame);
    }
}
