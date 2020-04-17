using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace IxMilia.BCad.Plotting.Pdf
{
    public class PdfPlotterViewModel : ViewModelBase
    {
        public IWorkspace Workspace { get; }

        private PdfPageViewModel _selectedPage;
        public PdfPageViewModel SelectedPage
        {
            get => _selectedPage;
            set
            {
                SetValue(ref _selectedPage, value);
            }
        }

        public ObservableCollection<PdfPageViewModel> Pages { get; }

        private Stream _stream;
        public Stream Stream
        {
            get => _stream;
            set => SetValue(ref _stream, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetValue(ref _fileName, value);
        }

        public PdfPlotterViewModel(IWorkspace workspace)
        {
            Workspace = workspace;
            Pages = new ObservableCollection<PdfPageViewModel>();
            Pages.CollectionChanged += PagesCollectionChanged;
            Pages.Add(new PdfPageViewModel(Workspace));
            SelectedPage = Pages.First();
        }

        private void PagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int i = 1;
            foreach (var page in Pages)
            {
                page.PageNumber = i++;
            }
        }
    }
}
