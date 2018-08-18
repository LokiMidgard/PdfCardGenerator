namespace Serilizer
{
    public interface IVisible
    {
        string IsVisiblePath { get; set; }
        //bool IsVisibleSpecified { get; set; }
        bool IsVisible { get; set; }
    }
}