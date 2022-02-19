using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorEdgeNewTab.Pages;

public partial class QuickLinkComponent
{
    [Inject] 
    private NavigationManager NavigationManager { get; set; }
    [Parameter]
    public string QuickLinkUrl { get; set; }

    [Parameter]
    public string QuickLinkImageUrl { get; set; }

    [Parameter]
    public string QuickLinkTitle { get; set; }

    [Parameter]
    public string QuickLinkId { get; set; }

    [CascadingParameter]
    public Index NewTabPage { get; set; }

    public string ModalName => $"DeleteQuickLinkModalFor{QuickLinkId}";

    private void OnQuickLinkClicked()
    {
        NavigationManager.NavigateTo(QuickLinkUrl);
    }

    private async Task DeleteQuickLink()
    {
        await NewTabPage.DeleteQuickLink(QuickLinkId);
    }
}