using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;


[Area("admin")]
[Authorize] // All users who access this functionality must be logged in,
public class OrderController : Controller
{

    private readonly IUnitOfWork _unitOfWork;

    [BindProperty]
    public OrderVM OrderVM { get; set; }

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
        OrderVM = new()
        {
            OrderHeader = _unitOfWork.OrderHeader.Get(oh => oh.Id == orderId, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(od => od.OrderHeaderId == orderId, includeProperties: "Product")
        };

        return View(OrderVM);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee )]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(oh => oh.Id == OrderVM.OrderHeader.Id);
        orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVM.OrderHeader.City;
        orderHeaderFromDb.State = OrderVM.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
        if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
        {
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
        }
        if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
        {
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
        }
        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();
        TempData["Success"] = "Order Details Updated Successfully.";

        return RedirectToAction(nameof(Details), new { orderId= orderHeaderFromDb.Id }); //  ALWAYS PASS ARGUMENT IN A NEW OBJECT!
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
        _unitOfWork.Save();

        TempData["Success"] = "Order is now In Process.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id }); //  ALWAYS PASS ARGUMENT IN A NEW OBJECT!
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult ShipOrder()
    {
        // update tracking and carrier information in DB
        OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(oh => oh.Id == OrderVM.OrderHeader.Id);
        orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
        orderHeader.OrderStatus = SD.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;
        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)); // DateOnly is a DOTNET 8 feature!
        }

        _unitOfWork.OrderHeader.Update(orderHeader);
        _unitOfWork.Save();

        TempData["Success"] = "Order Shipped Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id }); //  ALWAYS PASS ARGUMENT IN A NEW OBJECT!
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult CancelOrder()
    {
        var orderheader = _unitOfWork.OrderHeader.Get(oh => oh.Id == OrderVM.OrderHeader.Id);

        if (orderheader.PaymentStatus == SD.PaymentStatusApproved)
        {
            // refund payment

            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent=orderheader.PaymentIntentId
            };

            var service = new RefundService();
            Refund refund = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelled, SD.StatusRefunded);

        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelled, SD.StatusCancelled);
        }
        _unitOfWork.Save();

        TempData["Success"] = "Order Cancelled Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id }); //  ALWAYS PASS ARGUMENT IN A NEW OBJECT!

    }

    [HttpPost]
    [ActionName("Details")]
    public IActionResult Details_PAY_NOW() // This is the default POST action, i.e. there os no Asp-for in the button HTML
    {
        // Populate the OrderHeader and OrderDEtails again from DB

        OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(oh => oh.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
        OrderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(od => od.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");

        //Stripe logic
        string domain = "https://localhost:7292/";

        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
            CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (OrderDetail item in OrderVM.OrderDetails)
        {
            SessionLineItemOptions sessionLineItem = new()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100), // price in cents, converted to a long

                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Count
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);
        _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId); // PaymentIntentId will remain null until payment is successful.
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303); // Redirect!  
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            // this is a company customer

            SessionService service = new();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId); // This time, PaymentIntentId will be populated. 
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        return View(orderHeaderId);
    }


    #region API CALLS

    [HttpGet]
	public IActionResult GetAll(string status)
	{
		IEnumerable<OrderHeader> objOrderHeaders;

        if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        {
            objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
        } 
        else
        {
            // determine  user identity
            ClaimsIdentity? claimsItentity = (ClaimsIdentity)User.Identity;
            string? userId = claimsItentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            objOrderHeaders = _unitOfWork.OrderHeader.GetAll( oh => oh.ApplicationUserId ==  userId , includeProperties: "ApplicationUser");
        }

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
