namespace System.Windows
{
    public interface IDragDrop
    {
        bool CanDrag { get; }
        bool CanDrop(IDragDrop draggedObject);
        bool Drop(IDragDrop draggedObject);
    }
}
