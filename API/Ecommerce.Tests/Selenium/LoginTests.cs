using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Selenium;

public class LoginTests : IDisposable
{
    private int _actionDelay = 2000;
    private IWebDriver _driver;

    public LoginTests()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
    }

    [Fact]
    public void Login_WithValidCredentials_ShouldRedirectToHomePage()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5173/login");
        Thread.Sleep(_actionDelay);

        // Act
        var usernameField = _driver.FindElement(By.Name("username"));
        var passwordField = _driver.FindElement(By.Name("password"));
        var loginButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Đăng nhập')]"));

        usernameField.SendKeys("b@b");
        passwordField.SendKeys("Test12345*");
        Thread.Sleep(_actionDelay);
        loginButton.Click();
        Thread.Sleep(_actionDelay);

        // Assert
        var homeUrls = new List<string> { "http://localhost:5173/", "http://localhost:5173" };
        _driver.Url.ShouldBeOneOf(homeUrls.ToArray());
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5173/login");
        Thread.Sleep(_actionDelay);

        // Act
        var usernameField = _driver.FindElement(By.Name("username"));
        var passwordField = _driver.FindElement(By.Name("password"));
        var loginButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Đăng nhập')]"));

        usernameField.SendKeys("invalidUser");
        passwordField.SendKeys("invalidPassword");
        Thread.Sleep(_actionDelay);
        loginButton.Click();
        Thread.Sleep(_actionDelay);

        // Assert
        var errorElement = _driver.FindElement(
            By.XPath("(//div[contains(text(),'Thông tin đăng nhập không chính xác')])[1]")
        );
        errorElement.Displayed.ShouldBeTrue();
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
