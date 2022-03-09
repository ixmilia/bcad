using System.Linq;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.Rpc.Test
{
    public class ServerAgentTests
    {
        [Fact]
        public void SelectedEntitiesAreUpdatedWhenSettingAProperty()
        {
            var sa = new ServerAgent(new TestWorkspace(), null);

            // add entity and select it
            var originalEntity = new Location(new Point(1.0, 1.0, 1.0));
            var untouchedEntity = new Location(new Point(2.0, 2.0, 2.0));
            sa.Workspace.Add(sa.Workspace.Drawing.GetLayers().Single(), originalEntity);
            sa.Workspace.Add(sa.Workspace.Drawing.GetLayers().Single(), untouchedEntity);
            sa.Workspace.SelectedEntities.Add(originalEntity); // selected=(1,1,1), unselected=(2,2,2)

            // update the entity
            sa.SetPropertyPaneValue(new ClientPropertyPaneValue("x", "displayName", "9"));

            // verify that the new entity is selected and unselected entity is still selected
            Assert.Equal(new Point(9.0, 1.0, 1.0), ((Location)sa.Workspace.SelectedEntities.Single()).Point);
        }
    }
}
