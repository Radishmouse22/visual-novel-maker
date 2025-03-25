using UnityEngine;

public class EditorUIManager : MonoBehaviour
{
    public GameObject listView, detailView;
    public ProjectSelector selector;

    public bool listIsDefault;

    void Awake()
    {
        if (listIsDefault)
            SwitchToListView();
        else
            SwitchToDetailView();
    }

    public void SwitchToListView()
    {
        listView.SetActive(true);
        selector.RefreshProjectListings();
        detailView.SetActive(false);
    }

    public void SwitchToDetailView()
    {
        detailView.SetActive(true);
        listView.SetActive(false);
    }
}
