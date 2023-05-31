using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BulkyWeb.Areas.Admin.Controllers;


[Area("admin")]
public class OrderController : Controller
{

	private readonly IUnitOfWork _unitOfWork;

	public OrderController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}


	public IActionResult Index()
	{
		return View();
	}

    public IActionResult Details(int orderId)
    {
        OrderVM orderVM = new()
        {
            OrderHeader = _unitOfWork.OrderHeader.Get(oh => oh.Id == orderId, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(od => od.OrderHeaderId == orderId, includeProperties: "Product")
        };

        return View(orderVM);
    }


    #region API CALLS

    [HttpGet]
	public IActionResult GetAll(string status)
	{
		IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

        switch (status)
        {
            case "pending":
                objOrderHeaders = objOrderHeaders.Where(oh => oh.PaymentStatus == SD.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                objOrderHeaders = objOrderHeaders.Where(oh => oh.OrderStatus == SD.StatusInProcess);
                break;
            case "completed":
                objOrderHeaders = objOrderHeaders.Where(oh => oh.OrderStatus == SD.StatusShipped);
                break;
            case "approved":
                objOrderHeaders = objOrderHeaders.Where(oh => oh.OrderStatus == SD.StatusApproved);
                break;
            default:
                break;

        }

        return Json(new { data = objOrderHeaders });
	}

	#endregion
}
