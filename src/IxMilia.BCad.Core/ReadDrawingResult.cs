namespace IxMilia.BCad
{
    public struct ReadDrawingResult
    {
        public bool Success { get; }
        public Drawing Drawing { get; }
        public ViewPort ViewPort { get; }

        private ReadDrawingResult(bool success, Drawing drawing, ViewPort viewPort)
        {
            Success = success;
            Drawing = drawing;
            ViewPort = viewPort;
        }

        public static ReadDrawingResult Succeeded(Drawing drawing, ViewPort viewPort)
        {
            return new ReadDrawingResult(true, drawing, viewPort);
        }

        public static ReadDrawingResult Failed()
        {
            return new ReadDrawingResult(false, null, null);
        }
    }
}
