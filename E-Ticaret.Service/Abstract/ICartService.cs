using E_Ticaret.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Service.Abstract
{
    public interface ICartService
    {
        void AddToProduct(Product product, int quantity);
        void UpdateProduct(Product product, int quantity);
        void RemoveProduct(Product product);
        decimal TotalPrice();
        void ClearAll();
    }
}
