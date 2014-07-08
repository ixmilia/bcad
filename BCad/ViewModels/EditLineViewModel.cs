using BCad.Entities;

namespace BCad.ViewModels
{
    public class EditLineViewModel : ViewModelBase
    {
        private IWorkspace workspace;
        private Line line;
        private Point p1;
        private Point p2;

        public EditLineViewModel(IWorkspace workspace, Line line)
        {
            this.workspace = workspace;
            this.line = line;
            p1 = line.P1;
            p2 = line.P2;
        }

        public Point P1
        {
            get { return p1; }
            set
            {
                if (p1 == value)
                    return;
                p1 = value;
                ReplaceLine(line.Update(p1: value));
                OnPropertyChanged();
            }
        }

        public Point P2
        {
            get { return p2; }
            set
            {
                if (p2 == value)
                    return;
                p2 = value;
                ReplaceLine(line.Update(p2: value));
                OnPropertyChanged();
            }
        }

        private void ReplaceLine(Line newLine)
        {
            workspace.Update(drawing: workspace.Drawing.Replace(line, newLine));
            line = newLine;
            workspace.SelectedEntities.Clear();
            workspace.SelectedEntities.Add(line);
        }
    }
}
