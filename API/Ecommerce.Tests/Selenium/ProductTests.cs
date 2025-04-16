using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Selenium;

public class ProductTests : IDisposable
{
    private int _actionDelay = 1500;
    private IWebDriver _driver;

    public ProductTests()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

        // _driver.Navigate().GoToUrl("http://localhost:5173/login");
        // Thread.Sleep(_actionDelay);
        //
        // var usernameField = _driver.FindElement(By.Name("username"));
        // var passwordField = _driver.FindElement(By.Name("password"));
        // var loginButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Đăng nhập')]"));
        //
        // usernameField.SendKeys("b@b");
        // passwordField.SendKeys("Test12345*");
        // Thread.Sleep(_actionDelay);
        // loginButton.Click();
        // Thread.Sleep(_actionDelay);
    }

    [Fact]
    public void HomePage_ShouldDisplayProducts()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5173/");
        Thread.Sleep(_actionDelay);

        // Act
        var productElements = _driver.FindElements(By.CssSelector(".product-container>.card"));

        // Assert
        productElements.ShouldNotBeNull().ShouldNotBeEmpty();
    }

    [Fact]
    public void FilterProducts_WithKeyWord_ShouldDisplayFilteredProducts()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5173/");
        Thread.Sleep(_actionDelay);

        // Act
        var searchField = _driver.FindElement(
            By.CssSelector("form>input[placeholder='Tìm sản phẩm']")
        );
        searchField.Click();
        searchField.Clear();
        searchField.SendKeys("Điện thoại");
        searchField.SendKeys(Keys.Enter);
        Thread.Sleep(_actionDelay);
        var productElements = _driver.FindElements(By.CssSelector(".product-container>.card"));

        // Assert
        productElements.ShouldNotBeNull().ShouldNotBeEmpty();
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
