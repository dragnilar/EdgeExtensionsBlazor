using System.Threading.Tasks;
using System.Xml.Linq;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorEdgeNewTab.Pages;

public partial class QuickLinkComponent
{
    [Inject]
    public IModalService ModalService { get; set; }
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

    public string ModalName               => $"DeleteQuickLinkModalFor{QuickLinkId}";

    private void OnQuickLinkClicked()
    {
        NavigationManager.NavigateTo(QuickLinkUrl);
    }
    

    private Task OnDeleteQuickLinkButton_Click(MouseEventArgs arg)
    {
        return ModalService.Show<DeleteQuickLinkModal>(
            param =>
            {
                param.Add(x => x.QuickLinkId, QuickLinkId);
                param.Add(x => x.QuickLinkTitle, QuickLinkTitle);
            },
            new ModalInstanceOptions
            {
                Centered = true,
                UseModalStructure = false,
            });
    }
}