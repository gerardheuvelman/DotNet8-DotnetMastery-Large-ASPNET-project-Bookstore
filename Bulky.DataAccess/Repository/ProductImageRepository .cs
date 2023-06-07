using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository;

public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    private ApplicationDbContext _db;

    public ProductImageRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(ProductImage obj)
    {
        var objFromDb = _db.ProductImages.FirstOrDefault(pi => pi.Id == obj.Id);
        if (objFromDb != null)
        {
            objFromDb.ImageUrl = obj.ImageUrl;
            objFromDb.ProductId = obj.ProductId;    
        }
    }
}
