using F3DZEX.Command;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public interface IDLViewerControlDisplayElement
{
    Dlist DL { get; }
    bool Highlighted { get; }
}

public class DLViewerControlDListDisplayElement : IDLViewerControlDisplayElement
{
    public Dlist DL { get; }
    public bool Highlighted { get; } = false;

    public DLViewerControlDListDisplayElement(Dlist dList)
    {
        DL = dList;
    }
}

public class DLViewerControlDlistWithMatrixDisplayElement : IDLViewerControlDisplayElement
{
    public Dlist DL { get; }
    public bool Highlighted { get; }
    public Matrix4 Mtx { get; }

    public DLViewerControlDlistWithMatrixDisplayElement(Dlist dList, bool highlighted, Matrix4 mtx)
    {
        DL = dList;
        Highlighted = highlighted;
        Mtx = mtx;
    }
}
