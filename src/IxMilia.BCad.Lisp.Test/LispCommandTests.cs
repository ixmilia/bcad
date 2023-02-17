using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.Lisp;
using Xunit;

namespace IxMilia.BCad.Lisp.Test
{
    public class LispCommandTests
    {
        public LispWorkspace Workspace = new TestLispWorkspace();

        [Fact]
        public async Task DrawLineCommandTest()
        {
            var result = await Workspace.EvaluateAsync(@"(command ""draw.line"" ""1,1"" '(2 ""2"") '(3/1 6/2))");
            Assert.IsNotType<LispError>(result);
            var entities = Workspace.Drawing.GetEntities().ToList();
            Assert.Equal(2, entities.Count);

            var firstLine = Assert.IsType<Line>(entities[0]);
            Assert.Equal(new Point(1.0, 1.0, 0.0), firstLine.P1);
            Assert.Equal(new Point(2.0, 2.0, 0.0), firstLine.P2);

            var secondLine = Assert.IsType<Line>(entities[1]);
            Assert.Equal(new Point(2.0, 2.0, 0.0), secondLine.P1);
            Assert.Equal(new Point(3.0, 3.0, 0.0), secondLine.P2);
        }
    }
}
