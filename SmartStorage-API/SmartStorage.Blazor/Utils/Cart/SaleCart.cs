using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SmartStorage.Blazor.Utils.Local_Storage;
using SmartStorage_Shared.VO;
using System.Text.Json;

namespace SmartStorage.Blazor.Utils.Cart
{
    public class SaleCart
    {
        #region Properties

        private const string StorageKeyPrefix = "saleCart";

        private readonly IJSRuntime _js;

        private readonly AuthenticationStateProvider _auth;

        private List<SaleCartItem> _items = new();

        private string _storageKey;

        public event Action Changed;

        public IReadOnlyList<SaleCartItem> Items => _items;

        public int Count => _items.Count;

        public int Units => _items.Sum(i => i.Qntd);

        public decimal Total => _items.Sum(i => i.Subtotal);

        public bool CanCheckout => _items.Count > 0 && _items.All(i => i.Qntd > 0 && !i.ExceedsAvailable);

        #endregion

        #region Constructors

        public SaleCart(IJSRuntime js, AuthenticationStateProvider auth)
        {
            _js = js;
            _auth = auth;
        }

        #endregion

        #region Methods

        public async Task LoadAsync()
        {
            var state = await _auth.GetAuthenticationStateAsync();

            var username = state.User.FindFirst("unique_name")?.Value ?? state.User.Identity?.Name;

            var key = $"{StorageKeyPrefix}:{username}";

            if (key == _storageKey)
                return;

            _storageKey = key;

            var json = await _js.GetFromLocalStorage(_storageKey);

            try
            {
                _items = string.IsNullOrWhiteSpace(json)
                    ? new()
                    : JsonSerializer.Deserialize<List<SaleCartItem>>(json) ?? new();
            }
            catch (JsonException)
            {
                _items = new();
            }

            Changed?.Invoke();
        }

        public async Task<SaleCartItem> AddAsync(EnterVO entry, int quantity = 1)
        {
            await LoadAsync();

            var item = _items.FirstOrDefault(i => i.EnterId == entry.Id);

            if (item is null)
            {
                item = new SaleCartItem { EnterId = entry.Id };

                _items.Add(item);
            }

            item.ProductId = entry.ProductId;
            item.ProductName = entry.ProductName;
            item.ShelfName = entry.ShelfName;
            item.Price = entry.ProductPrice;
            item.Available = entry.ProductQuantity;
            item.Qntd += quantity;

            await SaveAsync();

            return item;
        }

        public async Task SetQuantityAsync(int enterId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.EnterId == enterId);

            if (item is null)
                return;

            item.Qntd = Math.Max(quantity, 1);

            await SaveAsync();
        }

        public async Task RemoveAsync(int enterId)
        {
            _items.RemoveAll(i => i.EnterId == enterId);

            await SaveAsync();
        }

        public async Task RefreshAsync(IEnumerable<EnterVO> entries)
        {
            var current = entries.ToDictionary(e => e.Id);

            foreach (var item in _items)
            {
                if (current.TryGetValue(item.EnterId, out var entry))
                {
                    item.Price = entry.ProductPrice;
                    item.Available = entry.ProductQuantity;
                }
                else
                {
                    item.Available = 0;
                }
            }

            await SaveAsync();
        }

        public async Task ClearAsync()
        {
            await LoadAsync();

            _items = new();

            await _js.RemoveItem(_storageKey);

            Changed?.Invoke();
        }

        public SaleBatchVO ToBatch(DateTime dateSale)
        {
            return new SaleBatchVO
            {
                DateSale = dateSale,
                Items = _items.Select(i => new SaleBatchItemVO { IdEnter = i.EnterId, Qntd = i.Qntd }).ToList(),
            };
        }

        private async Task SaveAsync()
        {
            if (_items.Count == 0)
                await _js.RemoveItem(_storageKey);
            else
                await _js.SetInLocalStorage(_storageKey, JsonSerializer.Serialize(_items));

            Changed?.Invoke();
        }

        #endregion
    }
}
