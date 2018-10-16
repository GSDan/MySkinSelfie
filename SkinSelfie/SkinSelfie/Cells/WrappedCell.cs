using System.ComponentModel;

namespace SkinSelfie.Cells
{
    public class WrappedCell<T> : INotifyPropertyChanged
    {
        public T Item { get; set; }
        bool isSelected = false;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(AppResources.Gallery_selected));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
