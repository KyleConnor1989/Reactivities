using System.Text.Json;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, 
            int currentPage, int itemsPerPage, int totalItems, int totalPages){
                var paginatioHeader = new{
                    currentPage,
                    itemsPerPage,
                    totalItems,
                    totalPages
                };
                response.Headers.Add("Pagination", JsonSerializer.Serialize(paginatioHeader));
        }
    }
}