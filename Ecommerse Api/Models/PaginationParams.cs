namespace Ecommerse_Api.Models
{
    public class PaginationParams
    {
        private int _maxItemsPerPage = 50;
        private int itemsPerPage;
        private string search;


        public int Page { get; set; } = 1;

        public int ItemsPerPage { 
            get => itemsPerPage; 
            set => itemsPerPage = value > _maxItemsPerPage ? _maxItemsPerPage : value; 
        }
        public string Search { get => search; set => search = value; }
    }
}
