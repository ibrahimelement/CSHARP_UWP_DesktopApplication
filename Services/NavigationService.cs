using Bermuda.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bermuda.Services
{
    class NavigationService : INavigationService
    {
        private readonly IDictionary<string, Type> _pages = new ConcurrentDictionary<string, Type>();
        private static Frame AppFrame { get; set; }
        public const string RootPage = "(Root)";
        public const string UnknownPage = "(Unknown)";
        private Func<bool> _CleanupRoutine;

        public void Configure(string page, Type type)
        {
            if (_pages.Values.Any(v => v == type))
            {
                throw new ArgumentException($"The {type.Name} view has been registered under another name");
            }
            _pages.Add(page, type);
        }

        public string CurrentPage {
            get
            {
                var frame = AppFrame;
                if (frame.BackStackDepth == 0)
                {
                    return RootPage;
                }
                if (frame.Content == null)
                {
                    return UnknownPage;
                }
                var type = frame.Content.GetType();
                if (_pages.Values.All(v => v != type))
                {
                    return UnknownPage;
                }

                var item = _pages.Single(i => i.Value == type);

                return item.Key;
            }
        }

        public void NavigateTo(string page)
        {
            NavigateTo(page, null);
        }

        public void NavigateTo(string page, object parameter)
        {
            if (!_pages.ContainsKey(page))
            {
                throw new ArgumentException($"Unable to find page registered with name {page}");
            }
            if (this._CleanupRoutine != null)
            {
                this._CleanupRoutine();
                this._CleanupRoutine = null;
            }
            AppFrame.Navigate(_pages[page], null);
        }

        public void GoBack()
        {
            if (AppFrame?.CanGoBack == true)
            {
                AppFrame.GoBack();
            }
        }

        public void SetFrame(Frame frame)
        {
            AppFrame = frame;
        }

        public void SetCleanupRoutine(Func<bool> cleanCallback)
        {
            this._CleanupRoutine = cleanCallback;
        }

    }
}
