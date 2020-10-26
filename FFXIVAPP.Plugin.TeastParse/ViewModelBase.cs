using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class ViewModelBase : ReactiveObject
    {
        protected void OnPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Execute setter and then raise property changed
        /// </summary>
        /// <param name="setter"></param>
        /// <param name="caller"></param>
        protected void Set(Action setter, [CallerMemberName] string caller = "")
        {
            setter();
            RaisePropertyChanged(caller);
        }

        protected void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            IReactiveObjectExtensions.RaisePropertyChanged(this, caller);
        }
    }
}
