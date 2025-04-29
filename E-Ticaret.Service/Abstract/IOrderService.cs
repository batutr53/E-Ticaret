using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Service.Abstract
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrder(string userCustomerId);
        Task<Order> GetOrderById(string id);
        Task<Order> GetOrderByOId(string oid);
        Task<Order> CreateOrder(CreateOrderDTO order);
        Task<Order> UpdateOrder(Order order);
    }
}
