using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;
using BCad.Objects;

namespace BCad
{
    public delegate void ObjectAddedEventHandler(object sender, ObjectAddedEventArgs e);

    public delegate void ObjectRemovedEventHandler(object sender, ObjectRemovedEventArgs e);

    public delegate void LayerAddedEventHandler(object sender, LayerAddedEventArgs e);

    public delegate void LayerRemovedEventHandler(object sender, LayerRemovedEventArgs e);

    public delegate void LayerUpdatedEventHandler(object sender, LayerUpdatedEventArgs e);

    public delegate void CurrentLayerChangedEventHandler(object sender, CurrentLayerChangedEventArgs e);

    public delegate void DocumentDetailsChangedEventHandler(object sender, DocumentDetailsChangedEventArgs e);
}
