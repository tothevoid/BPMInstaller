using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BPMInstaller.UI.Desktop.Models.Basics
{
    public class ResponsiveModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsChanged { get; private set; }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string? propName = null)
        {
            if (field != null && !field.Equals(value) || value != null && !value.Equals(field))
            {
                field = value;
                IsChanged = true;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public void CommitChanges()
        {
            IsChanged = false;
        }
    }
}
