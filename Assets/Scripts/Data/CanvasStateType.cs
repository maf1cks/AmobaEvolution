namespace Data
{
    public enum CanvasStateType
    {
        None = 0, // мгновенно (без анимации): Show/Hide
        Show = 1, // с анимацией: закрыть -> применить Show -> открыть
        Hide = 2  // с анимацией: закрыть -> применить Hide -> открыть
    }
}