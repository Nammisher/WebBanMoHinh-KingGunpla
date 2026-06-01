using System.Collections.Generic;

namespace WebBanMoHinh.MVC.Models
{
    public class CheckoutViewModel
    {
        // 1. Thông tin khách hàng (Hứng dữ liệu từ form nhập)
        public CustomerInfoViewModel CustomerInfo { get; set; } = new CustomerInfoViewModel();

        // 2. Phương thức vận chuyển
        public List<ShippingMethodViewModel> ShippingMethods { get; set; } = new List<ShippingMethodViewModel>();
        public int SelectedShippingMethodId { get; set; }

        // 3. Phương thức thanh toán
        public List<PaymentMethodViewModel> PaymentMethods { get; set; } = new List<PaymentMethodViewModel>();
        public int SelectedPaymentMethodId { get; set; }

        // 4. Danh sách sản phẩm trong giỏ hàng (Hiển thị cột bên phải)
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

        // 5. Tổng tiền
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
    }

    // ====================================================================
    // CÁC CLASS PHỤ TRỢ (Nằm chung file cho dễ quản lý)
    // ====================================================================

    public class CustomerInfoViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? FullAddress { get; set; }
        public string? Note { get; set; }
    }

    public class ShippingMethodViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsSelected { get; set; }
        public bool IsFree { get; set; }
        public decimal Price { get; set; }
    }

    public class PaymentMethodViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Icon { get; set; }
        public bool IsSelected { get; set; }
    }

    public class OrderItemViewModel
    {
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}