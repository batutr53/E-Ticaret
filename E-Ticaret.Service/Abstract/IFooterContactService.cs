using E_Ticaret.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Service.Abstract
{
    public interface IFooterContactService
    {
        Task<List<FooterContact>> GetActiveContactsAsync();
    }
}
