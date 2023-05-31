using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private ApplicationDbContext _db;

    public OrderHeaderRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderHeader orderHeader)
    {
        _db.OrderHeaders.Update(orderHeader);
        _db.SaveChanges();
    }

	public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
	{
        var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);
        if (orderFromDb != null)
        {
            orderFromDb.OrderStatus = orderStatus;
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orderFromDb.PaymentStatus = paymentStatus;
            }
            _db.SaveChanges();
        }
	}

	public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
	{
		var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);
        if(!string.IsNullOrEmpty(sessionId))
        {
            orderFromDb.SessionId = sessionId;
        }
		if (!string.IsNullOrEmpty(paymentIntentId))
		{
			orderFromDb.SessionId = paymentIntentId;
            orderFromDb.PaymentDate = DateTime.Now;
		}
	}
}
