using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }

    IProductRepository Product { get; }

    IProductImageRepository ProductImage { get; }

    ICompanyRepository Company { get; }

    IShoppingCartRepository ShoppingCart { get; }

    IOrderHeaderRepository OrderHeader { get; }
    IOrderDetailRepository OrderDetail { get; }

    IApplicationUserRepository ApplicationUser { get; }

    void Save();
}
