
namespace Serilizer
{
    internal partial class Project
    {
    }

    internal partial class ProjectTemplate
    {
    }

    internal partial class ImageElement
    {
    }

    /// <remarks/>
    internal abstract partial class BaseElement : IVisible, IHavePosition
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
        string IHavePosition.height { get => this.height; set => this.height = value; }
        string IHavePosition.left { get => this.left; set => this.left = value; }
        string IHavePosition.top { get => this.top; set => this.top = value; }
        string IHavePosition.width { get => this.width; set => this.width = value; }
    }

    internal partial class TextElement
    {
    }

    internal partial class AbstractParagraph : IVisible
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
    }

    internal partial class Run : IVisible
    {
        string IVisible.IsVisiblePath { get => this.IsVisiblePath; set => this.IsVisiblePath = value; }
        bool IVisible.IsVisible { get => this.IsVisible; set => this.IsVisible = value; }
    }

    internal partial class TextElementParagraphLineBreak
    {
    }

    internal partial class TextElementParagraphRun
    {

    }
}
