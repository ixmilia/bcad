using BCad.UI;

namespace BCad
{
    public interface IControlFactory
    {
        BCadControl Generate();
    }
}
