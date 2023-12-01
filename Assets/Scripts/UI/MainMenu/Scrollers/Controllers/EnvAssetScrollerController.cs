using EnhancedUI.EnhancedScroller;
using UI.Scrollers;

public class EnvAssetScrollerController : ScrollerController
{
    private IEnvAssetScroller _activeSetter;

    public void SetUp(IEnvAssetScroller setter, EnhancedScroller scroller)
    {
        _activeSetter = setter;
        _scroller = scroller;
        _scroller.Delegate = this;
    }

    public override int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _activeSetter.GetAvailableAssetCount();
    }

    public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as EnvAssetRefCellView;
        cellView.SetData(_activeSetter.GetAssetRef(dataIndex), dataIndex, this);
        return cellView;
    }

    public virtual void SetActiveAsset(int environment)
    {
        _activeSetter.SetAssetIndex(environment);
        _activeSetter.DisableOptionsDisplay();
    }
}
