using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly DatabaseContext _context;

        public OrderService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrder(string userCustomerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(oi => oi.CustomerId == userCustomerId)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderById(string id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(x=>x.DeliveryTimeRange)
                .FirstOrDefaultAsync(oi => oi.Oid == id);
        }

        public async Task<Order> CreateOrder(CreateOrderDTO model)
        {
            var cart = await _context.Carts
                 .Include(c => c.CartItems)
                 .ThenInclude(ci => ci.Product)
                 .FirstOrDefaultAsync(c => c.CustomerId == model.CustomerId);
            if (cart == null) { return new Order(); }
            var items = new List<OrderItem>();

            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products
          .Include(p => p.ProductImages)
          .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                var orderItems = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product!.Name,
                    ProductImage = product.ProductImages?.FirstOrDefault(x => x.IsDefault)?.ImageUrl ?? product.ProductImages?.FirstOrDefault()?.ImageUrl,
                    Price = product.Price,
                    Quantity = item.Quantity
                };

                items.Add(orderItems);
                product.Stock -= item.Quantity;
            }
            var total = items.Sum(i => i.Price * i.Quantity);
            var deliveryFree = 0;
            var order = new Order
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                City = model.City,
                AddresLine = model.AddressLine,
                CustomerId = model.CustomerId,
                OrderItems = items,
                SubTotal = total,
                DeliveryFree = deliveryFree,
                Oid = model.Oid,
                TxnNo = model.TxnNo,
                Description = model.Description,
                SenderEmail = model.SenderEmail,
                SenderFirstName = model.SenderFirstName,
                SenderLastName = model.SenderLastName,
                SenderPhone = model.SenderPhone,
                DeliveryDate = model.DeliveryDate,
                DeliveryTimeRangeId = model.DeliveryTimeRangeId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;

        }

        public async Task<Order> UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> GetOrderByOId(string oid)
        {
            if (string.IsNullOrEmpty(oid)) return null;
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Oid == oid); 
        }
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var allOrders = await _context.Orders.ToListAsync();

            var completedOrders = allOrders
                .Where(x => x.OrderStatus == OrderStatus.Completed)
                .ToList();

            var now = DateTime.UtcNow;
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var monthlyData = last6Months.Select(date =>
            {
                var monthOrders = completedOrders
                    .Where(o => o.OrderDate.Month == date.Month && o.OrderDate.Year == date.Year);
                return new
                {
                    Month = date.ToString("MMM yyyy"),
                    Total = monthOrders.Sum(o => o.SubTotal + o.DeliveryFree),
                    Completed = monthOrders.Count(o => o.OrderStatus == OrderStatus.Completed),
                    Pending = monthOrders.Count(o => o.OrderStatus == OrderStatus.Pending)
                };
            }).ToList();

            var pendingCount = allOrders.Count(o => o.OrderStatus == OrderStatus.Pending);
            var totalForRatio = completedOrders.Count + pendingCount;

            return new DashboardSummaryDto
            {
                MonthlyEarnings = monthlyData.LastOrDefault()?.Total ?? 0,
                AnnualEarnings = completedOrders
                    .Where(o => o.OrderDate.Year == now.Year)
                    .Sum(o => o.SubTotal + o.DeliveryFree),
                TaskCompletionPercent = totalForRatio > 0
                    ? (int)Math.Round((double)completedOrders.Count * 100 / totalForRatio)
                    : 0,
                TaskPendingPercent = totalForRatio > 0
                    ? (int)Math.Round((double)pendingCount * 100 / totalForRatio)
                    : 0,
                Last6MonthsLabels = monthlyData.Select(m => m.Month).ToList(),
                Last6MonthsTotals = monthlyData.Select(m => m.Total).ToList()
            };
        }

    }
}
