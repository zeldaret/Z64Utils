using F3DZEX.Command;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public interface IDLViewerControlDisplayElement { }

public class DLViewerControlDListDisplayElement : IDLViewerControlDisplayElement
{
    public Dlist dList;

    public DLViewerControlDListDisplayElement(Dlist dList)
    {
        this.dList = dList;
    }
}

public class DLViewerControlDlistWithMatrixDisplayElement : IDLViewerControlDisplayElement
{
    public Dlist dList;
    public Matrix4 mtx;

    public DLViewerControlDlistWithMatrixDisplayElement(Dlist dList, Matrix4 mtx)
    {
        this.dList = dList;
        this.mtx = mtx;
    }
}
