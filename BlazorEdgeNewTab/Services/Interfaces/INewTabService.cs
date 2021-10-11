using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Models;

namespace BlazorEdgeNewTab.Services.Interfaces
{
    public interface INewTabService
    {
        public Task <BingImageOfTheDayDto> GetImageOfDayDto();
        public Task<List<Image>> GetBingImageArchive();
    }
}