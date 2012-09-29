namespace BCad.Services
{
    public interface IExportService
    {
        Drawing ProjectTo2D(Drawing drawing, ViewPort viewPort);
    }
}
