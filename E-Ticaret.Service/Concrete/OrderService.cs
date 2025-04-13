using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Order?> GetOrderById(int id, string userCustomerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(oi => oi.CustomerId == userCustomerId && oi.Id == id);
        }

        public async Task<Order> CreateOrder(CreateOrderDTO orderDTO)
        {
           var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == orderDTO.CustomerId);
            if (cart == null) { return new Order(); }
            var items = new List<OrderItem>();

            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                var orderItems = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product!.Name,
                    ProductImage = product.Image!,
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
                FirstName = orderDTO.FirstName,
                LastName = orderDTO.LastName,
                Phone = orderDTO.Phone,
                City = orderDTO.City,
                AddresLine = orderDTO.AddresLine,
                CustomerId = orderDTO.CustomerId,
                OrderItems = items,
                SubTotal = total,
                DeliveryFree = deliveryFree
            };

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return order;

        }
    }
}
