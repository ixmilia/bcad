using BCad.Entities;

namespace BCad.ViewModels
{
    public class EditAggregateViewModel : EditEntityViewModel
    {
        private AggregateEntity aggregate;
        private Point location;

        public EditAggregateViewModel(IWorkspace workspace, AggregateEntity aggregate)
            : base(workspace)
        {
            this.aggregate = aggregate;
            location = aggregate.Location;
        }

        public Point Location
        {
            get { return location; }
            set
            {
                if (location == value)
                    return;
                location = value;
                ReplaceAggregate(aggregate.Update(location: value));
                OnPropertyChanged();
            }
        }

        private void ReplaceAggregate(AggregateEntity newAggregate)
        {
            ReplaceEntity(aggregate, newAggregate);
            aggregate = newAggregate;
        }
    }
}
