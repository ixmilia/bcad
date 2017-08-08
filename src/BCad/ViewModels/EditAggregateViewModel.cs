// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
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
