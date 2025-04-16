using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Selenium;

public class OrderTests : IDisposable
{
    private int _actionDelay = 2000;
    private IWebDriver _driver;

    public OrderTests()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

        _driver.Navigate().GoToUrl("http://localhost:5173/login");
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
        Thread.Sleep(_actionDelay);

        var usernameField = _driver.FindElement(By.Name("username"));
        var passwordField = _driver.FindElement(By.Name("password"));
        var loginButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Đăng nhập')]"));

        usernameField.SendKeys("b@b");
        passwordField.SendKeys("Test12345*");
        Thread.Sleep(_actionDelay);
        loginButton.Click();
        Thread.Sleep(_actionDelay);
    }

    [Fact]
    public void OrderCheckout_WithVnpayPaymentMethod_ShouldRedirectToVnpay()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5173/");
        Thread.Sleep(_actionDelay);

        // Act
        var productElements = _driver.FindElements(By.CssSelector(".product-container>.card"));
        productElements.First().Click();
        Thread.Sleep(_actionDelay);
        var addToCartButton = _driver.FindElement(
            By.XPath("(//button[contains(text(),'Thêm vào giỏ hàng')])[1]")
        );
        addToCartButton.Click();
        Thread.Sleep(_actionDelay);
        _driver.SwitchTo().Alert().Accept();
        Thread.Sleep(_actionDelay);

        _driver.FindElement(By.CssSelector("a[href='/cart']")).Click();
        Thread.Sleep(_actionDelay);

        var checkoutButton = _driver.FindElement(
            By.XPath("//button[normalize-space()='Checkout'][1]")
        );
        checkoutButton.Click();
        Thread.Sleep(_actionDelay);

        var nameField = _driver.FindElement(By.XPath("(//input[@placeholder='Họ và tên'])[1]"));
        var phoneField = _driver.FindElement(
            By.XPath("(//input[@placeholder='Số điện thoại'])[1]")
        );
        var addressField = _driver.FindElement(
            By.XPath(
                "(//input[@placeholder='123 Lê Văn Việt, phường Tăng Nhơn Phú A, TP. Thủ Đức, TP. Hồ Chí Minh'])[1]"
            )
        );

        nameField.SendKeys("Nguyễn Văn A");
        phoneField.SendKeys("0346014141");
        addressField.SendKeys(
            "123 Lê Văn Việt, phường Tăng Nhơn Phú A, TP. Thủ Đức, TP. Hồ Chí Minh"
        );

        _driver.FindElement(By.CssSelector("#province-dropdown option[value='202']")).Click();
        Thread.Sleep(_actionDelay);

        _driver.FindElement(By.CssSelector("#district-dropdown option[value='3695']")).Click();
        Thread.Sleep(_actionDelay);

        _driver.FindElement(By.CssSelector("#ward-dropdown option[value='90755']")).Click();
        Thread.Sleep(_actionDelay);

        _driver.FindElement(By.CssSelector("input[value='Vnpay']")).Click();
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        Thread.Sleep(5000);

        // Assert
        var newWindow = _driver.WindowHandles.Last();
        _driver.SwitchTo().Window(newWindow);
        _driver.Url.ShouldContain(
            "https://sandbox.vnpayment.vn/paymentv2/Transaction/PaymentMethod.html"
        );
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
