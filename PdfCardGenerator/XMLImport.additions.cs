
namespace PdfCardGenerator.Serilizer
{
    public partial class Project
    {
    }

    public partial class ProjectTemplate
    {
    }

    public partial class ImageElement
    {
    }

    /// <remarks/>
    public abstract partial class BaseElement : IVisible, IHavePosition
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
        string IHavePosition.height { get => this.height; set => this.height = value; }
        string IHavePosition.left { get => this.left; set => this.left = value; }
        string IHavePosition.top { get => this.top; set => this.top = value; }
        string IHavePosition.width { get => this.width; set => this.width = value; }
    }

    public partial class TextElement
    {
    }

    public partial class AbstractParagraph : IVisible
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
    }

    public partial class Run : IVisible
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
    }

    public partial class TextElementParagraphLineBreak
    {
    }

    public partial class TextElementParagraphRun
    {

    }
}
